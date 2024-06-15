using System;
using System.ComponentModel.DataAnnotations;

namespace Mahsul.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Ürün Adı")]
        public string ProductName { get; set; }

        [Required]
        [Display(Name = "Fiyat")]
        public int Price { get; set; }

        [Required]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Stok")]
        public int Stok { get; set; }

        public string ?ImagePath { get; set; }

        public string ?Address { get; set; }
        public int FarmerID { get; set; }
        public Farmer ?Farmer { get; set; }

        public List<Purchase> ?Purchases { get; set; }
        public List<Rating> ?Ratings { get; set; }

        public bool isActive { get; set; }

        public List<Category> ?Categories { get; set; }
        public int ?SelectedCategoryId { get; set; }

        [Display(Name = "Seçili Kategori Adı")]
        public string SelectedCategoryName { get; set; }


        [Required]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; } 

        [Required]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime EndDate { get; set; }

        public ICollection<ProductImage> ?ProductImage { get; set; }

    }
}
