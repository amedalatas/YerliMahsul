using Microsoft.AspNetCore.Identity;

namespace Mahsul.Models
{
    public class Farmer
    {
        public int FarmerID { get; set; }
        public string Address { get; set; }
        // Diğer çiftçi bilgileri eklenebilir

        public string UserID { get; set; }
        public string ?Username { get; set; }

        public IdentityUser User { get; set; }

        public List<Product> Products { get; set; }     
    }
}
