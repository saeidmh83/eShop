using eShop.Common;
using eShop.Data.Common;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShop.Web.Infrastructure.Extensions
{
    public class AssignRolesRequirement : IAuthorizationRequirement
    {

    }

    public class AssignRolesHandler : AuthorizationHandler<AssignRolesRequirement, Tuple<string[], string[]>>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AssignRolesRequirement requirement, Tuple<string[], string[]> newAndCurrentRoles)
        {
            if (context.User.HasClaim(GlobalConstants.Permission, GlobalConstants.AssignRoles) || !GetIsRolesChanged(newAndCurrentRoles.Item1, newAndCurrentRoles.Item2))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }   

        private bool GetIsRolesChanged(string[] newRoles, string[] currentRoles)
        {
            if (newRoles == null)
                newRoles = new string[] { };

            if (currentRoles == null)
                currentRoles = new string[] { };


            bool roleAdded = newRoles.Except(currentRoles).Any();
            bool roleRemoved = currentRoles.Except(newRoles).Any();

            return roleAdded || roleRemoved;
        }
    }
}
