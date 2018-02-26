using eShop.Data.Common;
using eShop.Data.Contracts;
using eShop.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace eShop.Data
{
    public class AccountManager : IAccountManager
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;


        public AccountManager(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await this.userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser> GetUserByUserNameAsync(string userName)
        {
            return await this.userManager.FindByNameAsync(userName);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await this.userManager.FindByEmailAsync(email);
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await this.userManager.GetRolesAsync(user);
        }


        public async Task<Tuple<ApplicationUser, string[]>> GetUserAndRolesAsync(string userId)
        {
            var user = await this.context.Users
                .Include(u => u.Roles)
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            var userRoleIds = user.Roles.Select(r => r.RoleId).ToList();

            var roles = await this.context.Roles
                .Where(r => userRoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToArrayAsync();

            return Tuple.Create(user, roles);
        }


        public async Task<List<Tuple<ApplicationUser, string[]>>> GetUsersAndRolesAsync(int page, int pageSize)
        {
            IQueryable<ApplicationUser> usersQuery = this.context.Users
                .Include(u => u.Roles)
                .OrderBy(u => u.UserName);

            if (page != -1)
                usersQuery = usersQuery.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                usersQuery = usersQuery.Take(pageSize);

            var users = await usersQuery.ToListAsync();

            var userRoleIds = users.SelectMany(u => u.Roles.Select(r => r.RoleId)).ToList();

            var roles = await this.context.Roles
                .Where(r => userRoleIds.Contains(r.Id))
                .ToArrayAsync();

            return users.Select(u => Tuple.Create(u,
                roles.Where(r => u.Roles.Select(ur => ur.RoleId).Contains(r.Id)).Select(r => r.Name).ToArray()))
                .ToList();
        }


        public async Task<Tuple<bool, string[]>> CreateUserAsync(ApplicationUser user, IEnumerable<string> roles, string password)
        {
            var result = await this.userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());


            user = await this.userManager.FindByNameAsync(user.UserName);

            try
            {
                result = await this.userManager.AddToRolesAsync(user, roles.Distinct());
            }
            catch
            {
                await DeleteUserAsync(user);
                throw;
            }

            if (!result.Succeeded)
            {
                await DeleteUserAsync(user);
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
            }

            return Tuple.Create(true, new string[] { });
        }


        public async Task<Tuple<bool, string[]>> UpdateUserAsync(ApplicationUser user)
        {
            return await UpdateUserAsync(user, null);
        }


        public async Task<Tuple<bool, string[]>> UpdateUserAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            var result = await this.userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());


            if (roles != null)
            {
                var userRoles = await this.userManager.GetRolesAsync(user);

                var rolesToRemove = userRoles.Except(roles).ToArray();
                var rolesToAdd = roles.Except(userRoles).Distinct().ToArray();

                if (rolesToRemove.Any())
                {
                    result = await this.userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!result.Succeeded)
                        return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                }

                if (rolesToAdd.Any())
                {
                    result = await this.userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!result.Succeeded)
                        return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                }
            }

            return Tuple.Create(true, new string[] { });
        }


        public async Task<Tuple<bool, string[]>> ResetPasswordAsync(ApplicationUser user, string newPassword)
        {
            string resetToken = await this.userManager.GeneratePasswordResetTokenAsync(user);

            var result = await this.userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());

            return Tuple.Create(true, new string[] { });
        }

        public async Task<Tuple<bool, string[]>> UpdatePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            var result = await this.userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());

            return Tuple.Create(true, new string[] { });
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            if (!await this.userManager.CheckPasswordAsync(user, password))
            {
                if (!this.userManager.SupportsUserLockout)
                    await this.userManager.AccessFailedAsync(user);

                return false;
            }

            return true;
        }
    


        public async Task<Tuple<bool, string[]>> DeleteUserAsync(string userId)
        {
            var user = await this.userManager.FindByIdAsync(userId);

            if (user != null)
                return await DeleteUserAsync(user);

            return Tuple.Create(true, new string[] { });
        }


        public async Task<Tuple<bool, string[]>> DeleteUserAsync(ApplicationUser user)
        {
            var result = await this.userManager.DeleteAsync(user);
            return Tuple.Create(result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }






        public async Task<ApplicationRole> GetRoleByIdAsync(string roleId)
        {
            return await this.roleManager.FindByIdAsync(roleId);
        }


        public async Task<ApplicationRole> GetRoleByNameAsync(string roleName)
        {
            return await this.roleManager.FindByNameAsync(roleName);
        }


        public async Task<ApplicationRole> GetRoleLoadRelatedAsync(string roleName)
        {
            var role = await this.context.Roles
                .Include(r => r.Claims)
                .Include(r => r.Users)
                .Where(r => r.Name == roleName)
                .FirstOrDefaultAsync();

            return role;
        }


        public async Task<List<ApplicationRole>> GetRolesLoadRelatedAsync(int page, int pageSize)
        {
            IQueryable<ApplicationRole> rolesQuery = this.context.Roles
                .Include(r => r.Claims)
                .Include(r => r.Users)
                .OrderBy(r => r.Name);

            if (page != -1)
                rolesQuery = rolesQuery.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                rolesQuery = rolesQuery.Take(pageSize);

            var roles = await rolesQuery.ToListAsync();

            return roles;
        }


        public async Task<Tuple<bool, string[]>> CreateRoleAsync(ApplicationRole role, IEnumerable<string> claims)
        {
            if (claims == null)
                claims = new string[] { };

            string[] invalidClaims = claims.Where(c => PermissionConstants.GetPermissionByValue(c) == null).ToArray();
            if (invalidClaims.Any())
                return Tuple.Create(false, new[] { "The following claim types are invalid: " + string.Join(", ", invalidClaims) });


            var result = await this.roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());


            role = await this.roleManager.FindByNameAsync(role.Name);

            foreach (string claim in claims.Distinct())
            {
                result = await this.roleManager.AddClaimAsync(role, new Claim("permission", PermissionConstants.GetPermissionByValue(claim)));

                if (!result.Succeeded)
                {
                    await DeleteRoleAsync(role);
                    return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                }
            }

            return Tuple.Create(true, new string[] { });
        }

        public async Task<Tuple<bool, string[]>> UpdateRoleAsync(ApplicationRole role, IEnumerable<string> claims)
        {
            if (claims != null)
            {
                string[] invalidClaims = claims.Where(c => PermissionConstants.GetPermissionByValue(c) == null).ToArray();
                if (invalidClaims.Any())
                    return Tuple.Create(false, new[] { "The following claim types are invalid: " + string.Join(", ", invalidClaims) });
            }


            var result = await this.roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());


            if (claims != null)
            {
                var roleClaims = (await this.roleManager.GetClaimsAsync(role)).Where(c => c.Type == "permission");
                var roleClaimValues = roleClaims.Select(c => c.Value).ToArray();

                var claimsToRemove = roleClaimValues.Except(claims).ToArray();
                var claimsToAdd = claims.Except(roleClaimValues).Distinct().ToArray();

                if (claimsToRemove.Any())
                {
                    foreach (string claim in claimsToRemove)
                    {
                        result = await this.roleManager.RemoveClaimAsync(role, roleClaims.Where(c => c.Value == claim).FirstOrDefault());
                        if (!result.Succeeded)
                            return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                    }
                }

                if (claimsToAdd.Any())
                {
                    foreach (string claim in claimsToAdd)
                    {
                        result = await this.roleManager.AddClaimAsync(role, new Claim("permission", PermissionConstants.GetPermissionByValue(claim)));
                        if (!result.Succeeded)
                            return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                    }
                }
            }

            return Tuple.Create(true, new string[] { });
        }


        public async Task<bool> TestCanDeleteRoleAsync(string roleId)
        {
            return !await this.context.UserRoles.Where(r => r.RoleId == roleId).AnyAsync();
        }


        public async Task<Tuple<bool, string[]>> DeleteRoleAsync(string roleName)
        {
            var role = await this.roleManager.FindByNameAsync(roleName);

            if (role != null)
                return await DeleteRoleAsync(role);

            return Tuple.Create(true, new string[] { });
        }


        public async Task<Tuple<bool, string[]>> DeleteRoleAsync(ApplicationRole role)
        {
            var result = await this.roleManager.DeleteAsync(role);
            return Tuple.Create(result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }
    }
}
