
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mahsul.Models
{

    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public int ?Productid { get; set; }
        public Product ?Products { get; set; }
    }
}


        
