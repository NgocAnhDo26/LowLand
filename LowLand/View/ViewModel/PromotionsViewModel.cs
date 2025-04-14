using LowLand.Model.Discount;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class PromotionsViewModel
    {
        private IDao _dao;

        public FullObservableCollection<Promotion> Promotions { get; set; }

        public PromotionsViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Promotions = new FullObservableCollection<Promotion>(_dao.Promotions.GetAll());
        }

        public ResponseCode DeletePromotion(Promotion promotion)
        {
            var result = _dao.Promotions.DeleteById(promotion.Id.ToString());

            if (result != -1)
            {
                Promotions.Remove(promotion);
                return ResponseCode.Success;
            }

            return ResponseCode.Error;
        }
    }
}
