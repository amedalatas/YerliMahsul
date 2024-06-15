using Microsoft.AspNetCore.Identity;
using System;

namespace Mahsul.Models
{
    public class Purchase
    {
        public int PurchaseID { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int Quantity { get; set; } // Satın alınan miktar
        public int Price { get; set; }

        public string UserID { get; set; }
        public IdentityUser User { get; set; }

        public int ProductID { get; set; }
        public Product Product { get; set; }
    }
}
