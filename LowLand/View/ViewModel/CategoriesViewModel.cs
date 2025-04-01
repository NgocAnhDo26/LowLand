using System.Linq;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class CategoriesViewModel
    {
        private IDao _dao;
        public FullObservableCollection<Category> Categories { get; set; }

        public CategoriesViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Categories = new FullObservableCollection<Category>(_dao.Categories.GetAll());
        }

        /// <summary>
        /// This method is used to edit a category
        /// </summary>
        /// <param name="categoryId">The id of the category to be edited</param>
        /// <param name="newName">The new name of the category</param>
        /// <returns>
        /// Returns response code:
        /// 1: The category is updated successfully
        /// 2: The new name is empty
        /// 3: The new name already exists
        /// 4: Error occured when updating the category
        /// </returns>
        public ResponseCode EditCategory(int categoryId, string newName)
        {
            // Validate if name is not empty
            if (string.IsNullOrWhiteSpace(newName))
            {
                return ResponseCode.EmptyName;
            }

            // Check if the new name already exists in other categories
            if (Categories.Any(c => c.Name == newName && c.Id != categoryId))
            {
                return ResponseCode.NameExists;
            }

            // Update the category in database
            int result = _dao.Categories.UpdateById(categoryId.ToString(), new Category() { Id = categoryId, Name = newName });

            if (result != -1)
            {
                // Update the category in the collection
                Category category = Categories.First(c => c.Id == categoryId);
                category.Name = newName;
                return ResponseCode.Success;
            }

            return ResponseCode.Error;
        }

        /// <summary>
        /// This method is used to add a new category
        /// </summary>
        /// <param name="newName">The name of the new category</param>
        /// <returns>
        /// Returns response code:
        /// ResponseCode.: The category is added successfully
        /// 2: The category name is empty
        /// 3: The category name already exists
        /// 4: Error occured when adding the new category
        /// </returns>
        public ResponseCode AddCategory(string newName)
        {
            // Validate if name is not empty
            if (string.IsNullOrWhiteSpace(newName))
            {
                return ResponseCode.EmptyName;
            }

            // Check if the new name already exists in other categories
            if (Categories.Any(c => c.Name == newName))
            {
                return ResponseCode.NameExists;
            }

            // Create a new category
            Category newCategory = new Category()
            {
                Name = newName
            };

            // Add the new category to the database
            int result = _dao.Categories.Insert(newCategory);
            if (result == 1)
            {
                // Update the category in the collection
                newCategory.Id = _dao.Categories.GetAll().Max(r => r.Id);
                Categories.Add(newCategory);
                return ResponseCode.Success;
            }

            return ResponseCode.Error;
        }

        public ResponseCode DeleteCategory(int categoryId)
        {
            // Check if any product is using this category
            if (_dao.Products.GetAll().Any(p => (p is SingleProduct sp
                && sp.Category?.Id == categoryId)
            ))
            {
                return ResponseCode.ItemHaveDependency;
            }

            // Delete the category in the database
            int result = _dao.Categories.DeleteById(categoryId.ToString());
            if (result != -1)
            {
                // Delete the category in the collection
                Category category = Categories.First(c => c.Id == categoryId);
                Categories.Remove(category);
                return ResponseCode.Success;
            }

            return ResponseCode.Error;
        }
    }
}
