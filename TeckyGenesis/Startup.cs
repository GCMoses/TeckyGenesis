using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TechStaticTools;
using TechStaticTools.BrainTree;
using Tecky.DataFiles.AppData;
using Tecky.DataFiles.Initializer;
using Tecky.DataFiles.Repo_s.GenRepo;
using Tecky.DataFiles.Repo_s.IRepo;

namespace TeckyGenesis
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(
               Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                 .AddDefaultTokenProviders().AddDefaultUI()
                 .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddHttpContextAccessor();
            services.AddSession(Options =>
            {
                Options.IdleTimeout = TimeSpan.FromMinutes(10);
                Options.Cookie.HttpOnly = true;
                Options.Cookie.IsEssential = true;
            });


            services.Configure<BrainTreeSettings>(Configuration.GetSection("BrainTree"));
            services.AddSingleton<IBrainTreeGate, BrainTreeGate>();
            services.AddScoped<IOrderHeaderRepo, OrderHeaderRepo>();
            services.AddScoped<IOrderDetailRepo, OrderDetailRepo>();
            services.AddScoped<IInquiryHeaderRepo, InquiryHeaderRepo>();
            services.AddScoped<IInquiryDetailRepo, InquiryDetailRepo>();
            services.AddScoped<ICategoryRepo, CategoryRepo>();
            services.AddScoped<IApplicationTypeRepo, ApplicationTypeRepo>();
            services.AddScoped<IProductRepo, ProductRepo>();
            services.AddScoped<IAppUserRepo, AppUserRepo>();
            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration.GetSection("Facebook")["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration.GetSection("Facebook")["Authentication:Facebook:AppSecret"];
            });

            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration.GetSection("Google")["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = Configuration.GetSection("Google")["Authentication:Google:ClientSecret"];
            });

            services.AddScoped<IDbInitializer, DbInitializer>();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            dbInitializer.Initialize();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
