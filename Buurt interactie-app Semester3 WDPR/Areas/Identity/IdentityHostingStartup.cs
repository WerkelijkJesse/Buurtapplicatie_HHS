using System;
using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using Buurt_interactie_app_Semester3_WDPR.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Buurt_interactie_app_Semester3_WDPR.Areas.Identity.IdentityHostingStartup))]
namespace Buurt_interactie_app_Semester3_WDPR.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddIdentity<BuurtAppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddDefaultUI()
                    .AddEntityFrameworkStores<BuurtAppContext>()
                    .AddRoles<IdentityRole>()
                    .AddRoleManager<RoleManager<IdentityRole>>()
                    .AddDefaultTokenProviders()
                    .AddEntityFrameworkStores<BuurtAppContext>();
            });
        }
    }
}