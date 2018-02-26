using eShop.Data.Models.Abstracts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Data.Models
{
    public class ApplicationRole : IdentityRole, IAuditable
    {
        public ApplicationRole()
        {

        }

        public ApplicationRole(string roleName) : base(roleName)
        {

        }
        
        public ApplicationRole(string roleName, string description) : base(roleName)
        {
            Description = description;
        }

        public string Description { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }
        
        public virtual ICollection<IdentityUserRole<string>> Users { get; set; }

        public virtual ICollection<IdentityRoleClaim<string>> Claims { get; set; }
    }
}
