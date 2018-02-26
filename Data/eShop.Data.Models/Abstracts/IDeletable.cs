using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Data.Models.Abstracts
{
    public interface IDeletable
    {
        bool IsDeleted { get; set; }

        DateTime? DeletedOn { get; set; }
    }
}
