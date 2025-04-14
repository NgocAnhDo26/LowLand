using System;
using System.Linq;
using LowLand.Model.Discount;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class AddPromotionViewModel
    {
        private IDao _dao;
        public Promotion Promotion { get; set; }
        public bool IsUpdateMode { get; set; } = false;

        public AddPromotionViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
        }

        public void LoadPromotion(Promotion promotion)
        {
            Promotion = new Promotion()
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                Amount = promotion.Amount,
                Type = promotion.Type,
                MinimumOrderValue = promotion.MinimumOrderValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive
            };

            IsUpdateMode = promotion.Id != -1;
        }

        public void ChangePromotionType(PromotionType type)
        {
            if (Promotion == null)
            {
                return;
            }

            Promotion.Type = type;
        }

        public ResponseCode SavePromotion()
        {
            if (string.IsNullOrEmpty(Promotion.Name))
            {
                return ResponseCode.EmptyName;
            }

            // Check if promotion name is already in use
            var existingPromotion = _dao.Promotions.GetAll()
                .FirstOrDefault(p => p.Name.Equals(Promotion.Name, StringComparison.OrdinalIgnoreCase) && p.Id != Promotion.Id);

            if (existingPromotion != null)
            {
                return ResponseCode.NameExists;
            }

            if (Promotion.Type == PromotionType.Percentage && Promotion.Amount > 100)
            {
                return ResponseCode.InvalidValue;
            }

            // Check if both dates are valid 
            if (Promotion.StartDate == default || Promotion.EndDate == default)
            {
                return ResponseCode.EmptyDate;
            }

            if (Promotion.StartDate > Promotion.EndDate)
            {
                return ResponseCode.InvalidDate;
            }

            // Check if IsActive is true and both dates are valid
            if (Promotion.IsActive)
            {
                if (Promotion.StartDate > DateOnly.FromDateTime(DateTime.Now) || Promotion.EndDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    return ResponseCode.InvalidStatus;
                }
            }

            int result = IsUpdateMode ? _dao.Promotions.UpdateById(Promotion.Id.ToString(), Promotion) : _dao.Promotions.Insert(Promotion);

            if (result == -1)
            {
                return ResponseCode.Error;
            }

            return ResponseCode.Success;
        }
    }
}
