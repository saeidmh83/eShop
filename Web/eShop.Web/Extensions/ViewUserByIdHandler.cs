using eShop.Common;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace eShop.Web.Infrastructure.Extensions
{
    public class ViewUserByIdRequirement : IAuthorizationRequirement
    {

    }


    public class ViewUserByIdHandler : AuthorizationHandler<ViewUserByIdRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewUserByIdRequirement requirement, string targetUserId)
        {
            if (context.User.HasClaim(GlobalConstants.Permission, GlobalConstants.ViewUsers) || GetIsSameUser(context.User, targetUserId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }


        private bool GetIsSameUser(ClaimsPrincipal user, string targetUserId)
        {
            return Utilities.GetUserId(user) == targetUserId;
        }
    }
}
