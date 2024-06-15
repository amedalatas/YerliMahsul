using System.ComponentModel.DataAnnotations;

namespace Mahsul.Models
{
    public class RatingViewModel
    {
        [Required]
        public int ProductID { get; set; }
        public int RatingID { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int RatingValue { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir.")]
        public string Comment { get; set; }
        public int ?FarmerID { get; set; }
        public string ?Username { get; set; }

    }
}
