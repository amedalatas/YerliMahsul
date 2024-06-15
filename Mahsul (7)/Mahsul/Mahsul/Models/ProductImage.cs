using System.ComponentModel.DataAnnotations;

namespace Mahsul.Models
{
    public class ProductImage
    {
        [Key]
        public int ImageID { get; set; }
        public int ProductID { get; set; }
        public string ImagePath { get; set; }
        public Product Product { get; set; }
    }

}
