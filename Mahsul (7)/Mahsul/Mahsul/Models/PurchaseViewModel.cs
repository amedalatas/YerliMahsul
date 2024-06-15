namespace Mahsul.Models
{
    public class PurchaseViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public int Stok { get; set; }
        public string ImagePath { get; set; } // Bu alanı hala kullanıyorsanız bırakın
        public ICollection<ProductImage> ProductImages { get; set; } // Added this line
        public string SelectedCategoryName { get; set; }
        public string Address { get; set; }
        public int FarmerID { get; set; }
        public string FarmerUsername {  get; set; }
        public int Quantity { get; set; }

        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? ExpiryDate { get; set; }
        public string? CVC { get; set; }
    }

}
