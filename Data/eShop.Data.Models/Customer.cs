using eShop.Data.Models.Abstracts;
using eShop.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Data.Models
{
    public class Customer : BaseModel<Guid>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public Gender Gender { get; set; }


        public ICollection<Order> Orders { get; set; }
    }
}
