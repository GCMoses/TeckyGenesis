using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechStaticTools;
using Tecky.Core.Models;
using Tecky.DataFiles.AppData;

namespace Tecky.DataFiles.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public IConfiguration Configuration { get; }

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {   
            _db = db;
            _roleManager = roleManager;
            Configuration = configuration;
            _userManager = userManager;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }


            if (!_roleManager.RoleExistsAsync(StaticFiles.AdminRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticFiles.AdminRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticFiles.CustomerRole)).GetAwaiter().GetResult();
            }
            else
            {
                return;
            }
            _userManager.CreateAsync(new AppUser
            {
                UserName = Configuration.GetSection("BongoMan")["UserName"],
                Email = Configuration.GetSection("BongoMan")["Email"],
                EmailConfirmed = true,
                FullName = Configuration.GetSection("BongoMan")["FullName"],
                PhoneNumber = Configuration.GetSection("BongoMan")["PhoneNumber"],
            }, "Kimberly1!").GetAwaiter().GetResult();

            AppUser user = _db.AppUser.FirstOrDefault(u => u.Email == Configuration.GetSection("BongoMan")["Email"]);
            _userManager.AddToRoleAsync(user, StaticFiles.AdminRole).GetAwaiter().GetResult();

        }
    }
}