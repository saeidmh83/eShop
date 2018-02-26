using eShop.Data.Models.Abstracts;
using System;
using System.Collections.Generic;

namespace eShop.Data.Models
{

    public class Product : BaseModel<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public decimal Price { get; set; }
        public int UnitsInStock { get; set; }
        public bool IsActive { get; set; }
        public bool IsDiscontinued { get; set; }
        
        public Guid ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; }
        
    }
}