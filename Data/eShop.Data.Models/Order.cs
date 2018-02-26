using eShop.Data.Models.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Data.Models
{
    public class Order : BaseModel<Guid>
    {
        public decimal Discount { get; set; }
        public string Comments { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }


        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

}
