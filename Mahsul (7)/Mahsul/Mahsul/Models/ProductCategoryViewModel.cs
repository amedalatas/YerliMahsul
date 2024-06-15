namespace Mahsul.Models
{
    public class ProductCategoryViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public string[] SelectedCategories { get; set; }
        public int UserFarmerId {  get; set; }
        public string FarmerUsername { get; set; }
    }

}
