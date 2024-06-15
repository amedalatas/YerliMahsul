using System.ComponentModel.DataAnnotations;

namespace Mahsul.Models
{
    public class ProductViewModel
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
        [Display(Name = "Stok")]
        public int Stok { get; set; }

        [Required]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        public List<Category>? categories { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        public List<IFormFile>? photos { get; set; } // Çoklu resim yükleme için güncellendi
        public List<string>? ImagePaths { get; set; }
        public int SelectedCategoryId { get; set; }
       // public string SelectedCategoryName { get; set; }  // Eklendi


        [Required]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime EndDate { get; set; }
    }

}
