using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Windows.Storage;

namespace LowLand.Services
{
    public class RawSalesData
    {
        public int ProductID { get; set; }
        public DateTime OrderDay { get; set; }
        public float TotalQuantity { get; set; }
    }

    public class SalesData
    {
        public int ProductID { get; set; }
        public DateTime OrderDay { get; set; }
        public int DayOfWeek { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public float SalesLag1 { get; set; }
        public float SalesLag7 { get; set; }
        public float TotalQuantity { get; set; }
    }

    // Class to hold input data
    public class SalesInput
    {
        public int ProductID { get; set; }
        public int DayOfWeek { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public float SalesLag1 { get; set; }
        public float SalesLag7 { get; set; }
    }

    public class SalesPrediction
    {
        [ColumnName("Score")]
        public float PredictedSales { get; set; }
    }

    public class ForecastPredictionService
    {
        private IDao _dao;
        private readonly MLContext _mlContext;

        public ForecastPredictionService()
        {
            _dao = Services.GetKeyedSingleton<IDao>();
            _mlContext = new MLContext(seed: 0);
        }

        public async Task RetrainModelAsync()
        {
            var rawSalesData = GetRawSalesDataFromDB();
            var salesData = TransformRawDataToSalesData(rawSalesData);

            IDataView trainingDataView = _mlContext.Data.LoadFromEnumerable(salesData);

            // SỬA ĐỔI PIPELINE ĐỂ CHUYỂN ĐỔI KIỂU DỮ LIỆU
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "ProductIDEncoded",
                    inputColumnName: "ProductID") // Input: int, Output: Vector<float>
                                                  // Thêm các bước ConvertType để chuyển đổi int sang float (Single)
                .Append(_mlContext.Transforms.Conversion.ConvertType("DayOfWeek", outputKind: DataKind.Single))
                .Append(_mlContext.Transforms.Conversion.ConvertType("Month", outputKind: DataKind.Single))
                .Append(_mlContext.Transforms.Conversion.ConvertType("Year", outputKind: DataKind.Single))
                // Concatenate sử dụng các cột đã được chuyển đổi (cùng tên) và các cột float khác
                .Append(_mlContext.Transforms.Concatenate("Features",
                    "ProductIDEncoded", // Vector<float>
                    "DayOfWeek",        // Giờ là float (Single)
                    "Month",            // Giờ là float (Single)
                    "Year",             // Giờ là float (Single)
                    "SalesLag1",        // float (Single)
                    "SalesLag7"))       // float (Single)
                .Append(_mlContext.Transforms.NormalizeMinMax("Features", "Features")) // Normalize vector float
                .Append(_mlContext.Regression.Trainers.LightGbm(
                    labelColumnName: "TotalQuantity", // float (Single)
                    featureColumnName: "Features"));   // Vector<float>

            Debug.WriteLine("Training pipeline defined.");

            // Split data for validation
            var trainTestSplit = _mlContext.Data.TrainTestSplit(trainingDataView, testFraction: 0.2);
            Debug.WriteLine("Data split complete.");
            //Debug.WriteLine("Inspecting Test Set Target Values:");
            //try
            //{
            //    // Lấy cột TotalQuantity từ TestSet
            //    var testSetTargetColumn = trainTestSplit.TestSet.GetColumn<float>("TotalQuantity").ToList();

            //    if (testSetTargetColumn.Any())
            //    {
            //        // Tìm các giá trị duy nhất
            //        var distinctValues = testSetTargetColumn.Distinct().ToList();
            //        var minValue = testSetTargetColumn.Min();
            //        var maxValue = testSetTargetColumn.Max();
            //        var avgValue = testSetTargetColumn.Average();

            //        Debug.WriteLine($"  Number of test samples: {testSetTargetColumn.Count}");
            //        Debug.WriteLine($"  Min target value in test set: {minValue}");
            //        Debug.WriteLine($"  Max target value in test set: {maxValue}");
            //        Debug.WriteLine($"  Average target value in test set: {avgValue}");
            //        Debug.WriteLine($"  Distinct target values in test set: ({distinctValues.Count} unique): {string.Join(", ", distinctValues.Take(10))} {(distinctValues.Count > 10 ? "..." : "")}"); // Chỉ in vài giá trị đầu

            //        // Kiểm tra phương sai
            //        if (distinctValues.Count <= 1)
            //        {
            //            Debug.WriteLine("  ----> WARNING: Test set target variable has NO variance! This causes R²=NaN.");
            //        }
            //        else
            //        {
            //            // Tính phương sai mẫu (sample variance) nếu có nhiều hơn 1 giá trị
            //            double sumOfSquares = testSetTargetColumn.Sum(val => Math.Pow(val - avgValue, 2));
            //            double variance = sumOfSquares / (testSetTargetColumn.Count - 1);
            //            Debug.WriteLine($"  Sample Variance of target in test set: {variance}");
            //            if (variance < 0.0001) // Ngưỡng nhỏ tùy chọn
            //            {
            //                Debug.WriteLine("  ----> WARNING: Test set target variable has VERY LOW variance.");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Debug.WriteLine("  Test set is empty.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"  Error inspecting test set: {ex.Message}");
            //}

            ITransformer model = pipeline.Fit(trainTestSplit.TrainSet);

            // Evaluate the model
            var predictions = model.Transform(trainTestSplit.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "TotalQuantity");
            Debug.WriteLine($"R²: {metrics.RSquared}, RMSE: {metrics.RootMeanSquaredError}");

            // Save the model
            var schema = trainingDataView.Schema;
            await SaveModelAsync(model, schema);
            Debug.WriteLine("Model retrained and saved successfully.");
        }

        public List<RawSalesData> GetRawSalesDataFromDB()
        {
            // Extract the date and its order-details of the orders
            var orders = _dao.Orders.GetAll()
                .Where(order => order.Status == "Hoàn thành")
                .Select(order => new
                {
                    order.Date,
                    order.Details
                })
                .ToList();

            // Flatten the order details and group by product and date
            var rawSalesData = orders
                .SelectMany(order => order.Details
                .Where(detail => detail.ProductId.HasValue)
                .Select(detail => new RawSalesData
                {
                    ProductID = detail.ProductId.Value,
                    OrderDay = order.Date.Date,
                    TotalQuantity = detail.quantity
                }))
                .GroupBy(data => new { data.ProductID, data.OrderDay })
                .Select(group => new RawSalesData
                {
                    ProductID = group.Key.ProductID,
                    OrderDay = group.Key.OrderDay,
                    TotalQuantity = group.Sum(data => data.TotalQuantity)
                })
                .ToList();

            return rawSalesData ?? new List<RawSalesData>();
        }

        public List<SalesData> TransformRawDataToSalesData(List<RawSalesData> rawSalesData)
        {
            var salesDataList = new List<SalesData>();
            // Nhóm theo ProductID để tính lag dễ dàng
            var groupedData = rawSalesData.GroupBy(d => d.ProductID);

            foreach (var group in groupedData)
            {
                var productSales = group.OrderBy(d => d.OrderDay).ToList(); // Đảm bảo đã sắp xếp theo ngày

                for (int i = 0; i < productSales.Count; i++)
                {
                    var current = productSales[i];
                    var salesRecord = new SalesData
                    {
                        ProductID = current.ProductID,       // int
                        OrderDay = current.OrderDay,         // DateTime
                        TotalQuantity = current.TotalQuantity, // float
                        // Tính toán các đặc trưng thời gian là int
                        DayOfWeek = (int)current.OrderDay.DayOfWeek, // int (Sunday = 0, ..., Saturday = 6)
                        Month = current.OrderDay.Month,        // int
                        Year = current.OrderDay.Year,          // int
                        // Tính toán lag (float)
                        SalesLag1 = i > 0 ? productSales[i - 1].TotalQuantity : 0f,
                        SalesLag7 = i >= 7 ? productSales[i - 7].TotalQuantity : 0f
                    };
                    salesDataList.Add(salesRecord);
                }
            }
            return salesDataList;
        }

        private async Task SaveModelAsync(ITransformer model, DataViewSchema schema)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                Debug.WriteLine($"Local folder path: {localFolder.Path}");
                Debug.WriteLine("Creating model.zip file...");
                var file = await localFolder.CreateFileAsync("model.zip", CreationCollisionOption.ReplaceExisting);
                using var randomAccessStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                using var stream = randomAccessStream.AsStreamForWrite();
                _mlContext.Model.Save(model, schema, stream);
                Debug.WriteLine("Model saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving model: {ex.Message}\nStack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Method to load the model
        public async Task<ITransformer> LoadModelAsync()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                Debug.WriteLine("Reading model from: " + localFolder.Path);

                var modelPath = Path.Combine(localFolder.Path, "model.zip");
                Debug.WriteLine($"Checking for model at: {modelPath}");

                if (!File.Exists(modelPath))
                {
                    Debug.WriteLine("Model file not found. Retraining the model.");
                    await RetrainModelAsync();

                    if (!File.Exists(modelPath))
                    {
                        Debug.WriteLine("Model file still not found after retraining. Cannot load model.");
                        return null;
                    }
                }

                StorageFile file = null;
                try
                {
                    file = await localFolder.GetFileAsync("model.zip");
                }
                catch (FileNotFoundException)
                {
                    Debug.WriteLine("GetFileAsync failed even after checking File.Exists. Retraining again or error.");
                    await RetrainModelAsync();
                    file = await localFolder.GetFileAsync("model.zip");
                }


                using var randomAccessStream = await file.OpenAsync(FileAccessMode.Read);
                using var stream = randomAccessStream.AsStreamForRead();

                DataViewSchema schema;
                var loadedModel = _mlContext.Model.Load(stream, out schema);
                Debug.WriteLine("Model loaded successfully.");
                return loadedModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading model: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<Dictionary<int, float>> MakePredictionAsync(int topN = 5)
        {
            var loadedModel = await LoadModelAsync();
            if (loadedModel == null)
            {
                Debug.WriteLine("Model not loaded. Cannot make predictions.");
                return null;
            }

            var rawSalesData = GetRawSalesDataFromDB();
            if (rawSalesData == null || !rawSalesData.Any())
            {
                Debug.WriteLine("No sales data found in DB. Cannot make predictions.");
                return new Dictionary<int, float>(); // Trả về rỗng nếu không có dữ liệu
            }

            // Không cần tạo lại predictionPipeline ở đây vì model đã load bao gồm các bước biến đổi
            // PredictionEngine sẽ được tạo trực tiếp từ model đã load.
            // Schema của model đã load phải tương thích với SalesInput.
            PredictionEngine<SalesInput, SalesPrediction> predictionEngine;
            try
            {
                predictionEngine = _mlContext.Model.CreatePredictionEngine<SalesInput, SalesPrediction>(loadedModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating PredictionEngine. Schema mismatch?: {ex.Message}");
                // Thử Retrain nếu lỗi có thể do schema cũ
                Debug.WriteLine("Attempting to retrain model due to PredictionEngine creation error.");
                await RetrainModelAsync();
                loadedModel = await LoadModelAsync(); // Load lại model mới
                if (loadedModel == null) return null;
                try
                {
                    predictionEngine = _mlContext.Model.CreatePredictionEngine<SalesInput, SalesPrediction>(loadedModel);
                }
                catch (Exception innerEx)
                {
                    Debug.WriteLine($"Still cannot create PredictionEngine after retraining: {innerEx.Message}");
                    return null;
                }
            }


            // Tổ chức lịch sử bán hàng theo sản phẩm và ngày gần nhất để lấy lag
            var salesHistory = rawSalesData
                .GroupBy(d => d.ProductID)
                .ToDictionary(
                    g => g.Key, // ProductID (int)
                    g => g.OrderByDescending(d => d.OrderDay) // Sắp xếp giảm dần theo ngày
                          .ToDictionary(d => d.OrderDay.Date, d => d.TotalQuantity) // Lấy TotalQuantity theo ngày
                );


            // Xác định ngày cuối cùng có dữ liệu (D)
            var D = rawSalesData.Max(d => d.OrderDay.Date);

            // Tạo các ngày tương lai (D+1 đến D+7)
            var futureDates = Enumerable.Range(1, 7).Select(i => D.AddDays(i)).ToList();

            var productPredictions = new Dictionary<int, float>();

            // Xử lý từng sản phẩm có trong lịch sử
            foreach (var productId in salesHistory.Keys)
            {
                var history = salesHistory[productId];
                var predictedSalesForProduct = new List<float>();
                var currentLagSales = history.ToDictionary(kv => kv.Key, kv => kv.Value); // Bản sao để cập nhật dự đoán

                // Lấy giá trị lag ban đầu từ dữ liệu thực tế
                float lastKnownSales = 0;
                if (history.TryGetValue(D, out var salesOnD))
                {
                    lastKnownSales = salesOnD;
                }

                // Dự đoán cho từng ngày trong tương lai
                for (int i = 0; i < futureDates.Count; i++)
                {
                    var futureDate = futureDates[i];
                    var lag1Date = futureDate.AddDays(-1);
                    var lag7Date = futureDate.AddDays(-7);

                    // Lấy SalesLag1: Ưu tiên lấy từ dự đoán trước đó, nếu không thì lấy từ lịch sử
                    float salesLag1;
                    if (i > 0) // Nếu không phải ngày đầu tiên dự đoán (F = D+1)
                    {
                        salesLag1 = predictedSalesForProduct[i - 1]; // Lấy từ dự đoán của ngày D+i
                    }
                    else // Nếu là ngày đầu tiên (F = D+1), lag 1 là ngày D
                    {
                        if (!currentLagSales.TryGetValue(D, out salesLag1))
                        {
                            salesLag1 = 0f; // Hoặc một giá trị mặc định khác nếu ngày D không có data
                            //Debug.WriteLine($"Warning: Missing sales data for ProductID {productId} on day {D} for Lag1 calculation.");
                        }
                    }


                    // Lấy SalesLag7: Ưu tiên lấy từ dự đoán nếu lag 7 rơi vào khoảng đã dự đoán, nếu không lấy từ lịch sử
                    float salesLag7;
                    if (!currentLagSales.TryGetValue(lag7Date, out salesLag7))
                    {
                        // Nếu không có trong lịch sử, có thể sản phẩm mới hoặc thiếu dữ liệu
                        salesLag7 = 0f; // Hoặc một giá trị mặc định/trung bình khác
                                        // Debug.WriteLine($"Warning: Missing sales data for ProductID {productId} on day {lag7Date} for Lag7 calculation.");
                    }


                    var input = new SalesInput
                    {
                        ProductID = productId,
                        // *** SỬA ĐỔI: Tính toán và gán kiểu int ***
                        DayOfWeek = (int)futureDate.DayOfWeek, // int
                        Month = futureDate.Month,           // int
                        Year = futureDate.Year,             // int
                        SalesLag1 = salesLag1,              // float
                        SalesLag7 = salesLag7               // float
                    };

                    var prediction = predictionEngine.Predict(input);
                    var predictedValue = Math.Max(0, prediction.PredictedSales); // Đảm bảo dự đoán không âm

                    predictedSalesForProduct.Add(predictedValue);

                    // Cập nhật giá trị dự đoán vào dictionary để các ngày sau có thể dùng làm lag
                    currentLagSales[futureDate] = predictedValue;
                }

                // Tính tổng doanh số dự đoán cho tuần tới
                var totalSales = predictedSalesForProduct.Sum();
                productPredictions[productId] = totalSales;
            }

            // Lấy top N sản phẩm
            var topProducts = productPredictions
                .OrderByDescending(kv => kv.Value)
                .Take(topN)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return topProducts;
        }
    }
}
