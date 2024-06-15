using Microsoft.AspNetCore.Identity;

namespace Mahsul.Models
{
    public class Rating
    {
        public int RatingID { get; set; }
        public int RatingValue { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserID { get; set; }
        public string ?Username { get; set; }

        public IdentityUser User { get; set; }

        public int ProductID { get; set; }
        public Product Product { get; set; }
    }
}
