﻿using eShop.Data.Common;
using eShop.Data.Contracts;
using eShop.Data.Models;
using eShop.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eShop.Data.Seeding
{
    public class DatabaseInitializerSeed : IDatabaseInitializerSeed
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountManager _accountManager;
        private readonly ILogger _logger;
        private List<string> permissionValues = new List<string>()
        {
            "users.view",
             "users.manage",
             "roles.view",
             "roles.manage",
             "roles.assign"
        };

        public DatabaseInitializerSeed(ApplicationDbContext context, AccountManager accountManager, ILogger<DatabaseInitializerSeed> logger)
        {
            _accountManager = accountManager;
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync().ConfigureAwait(false);

            if (!await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Generating inbuilt accounts");

                const string adminRoleName = "administrator";
                const string userRoleName = "user";

                await EnsureRoleAsync(adminRoleName, "Default administrator", permissionValues.ToArray());
                await EnsureRoleAsync(userRoleName, "Default user", new string[] { });

                await CreateUserAsync("admin", "tempP@ss123", "Inbuilt Administrator", "admin@ebenmonney.com", "+1 (123) 000-0000", new string[] { adminRoleName });
                await CreateUserAsync("user", "tempP@ss123", "Inbuilt Standard User", "user@ebenmonney.com", "+1 (123) 000-0001", new string[] { userRoleName });

                _logger.LogInformation("Inbuilt account generation completed");
            }



            if (!await _context.Customers.AnyAsync() && !await _context.ProductCategories.AnyAsync())
            {
                _logger.LogInformation("Seeding initial data");

                Customer cust_1 = new Customer
                {
                    Name = "Ebenezer Monney",
                    Email = "contact@ebenmonney.com",
                    Gender = Gender.Male,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };

                Customer cust_2 = new Customer
                {
                    Name = "Itachi Uchiha",
                    Email = "uchiha@narutoverse.com",
                    PhoneNumber = "+81123456789",
                    Address = "Some fictional Address, Street 123, Konoha",
                    City = "Konoha",
                    Gender = Gender.Male,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };

                Customer cust_3 = new Customer
                {
                    Name = "John Doe",
                    Email = "johndoe@anonymous.com",
                    PhoneNumber = "+18585858",
                    Address = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio.
                    Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet",
                    City = "Lorem Ipsum",
                    Gender = Gender.Male,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn= DateTime.UtcNow
                };

                Customer cust_4 = new Customer
                {
                    Name = "Jane Doe",
                    Email = "Janedoe@anonymous.com",
                    PhoneNumber = "+18585858",
                    Address = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio.
                    Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet",
                    City = "Lorem Ipsum",
                    Gender = Gender.Male,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };

                


                _context.Customers.Add(cust_1);
                _context.Customers.Add(cust_2);
                _context.Customers.Add(cust_3);
                _context.Customers.Add(cust_4);
                

                await _context.SaveChangesAsync();

                _logger.LogInformation("Seeding initial data completed");
            }
        }



        private async Task EnsureRoleAsync(string roleName, string description, string[] claims)
        {
            if ((await _accountManager.GetRoleByNameAsync(roleName)) == null)
            {
                ApplicationRole applicationRole = new ApplicationRole(roleName, description);

                var result = await this._accountManager.CreateRoleAsync(applicationRole, claims);

                if (!result.Item1)
                    throw new Exception($"Seeding \"{description}\" role failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");
            }
        }

        private async Task<ApplicationUser> CreateUserAsync(string userName, string password, string fullName, string email, string phoneNumber, string[] roles)
        {
            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = userName,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true,
                IsEnabled = true
            };

            var result = await _accountManager.CreateUserAsync(applicationUser, roles, password);

            if (!result.Item1)
                throw new Exception($"Seeding \"{userName}\" user failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");


            return applicationUser;
        }
    }
}
