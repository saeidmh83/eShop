using eShop.Data.Models.Abstracts;
using System;
using System.Collections.Generic;

namespace eShop.Data.Models
{
    public class ProductCategory : BaseModel<Guid>
    {
       
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }


        public ICollection<Product> Products { get; set; }
    }
}