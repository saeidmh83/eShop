using eShop.Data.Models.Abstracts;
using System;

namespace eShop.Data.Models
{
    public class OrderDetail : BaseModel<Guid>
    {
       
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }


        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}