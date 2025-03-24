using LowLand.Model.Product;
using LowLand.Services;

namespace LowLand.View.ViewModel
{
    public class ProductOptionViewModel
    {
        private IDao _dao;
        public ProductOption Option { get; set; }

        public ProductOptionViewModel(int productId, int optionId)
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();

            // Create new option
            if (optionId == -1)
            {
                Option = new ProductOption()
                {
                    OptionId = -1,
                    ProductId = productId,
                    Name = "",
                };

                return;
            }

            Option = _dao.ProductOptions.GetById(optionId.ToString());
        }
    }
}
