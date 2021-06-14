using Buurt_interactie_app_Semester3_WDPR.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data
{
    public class BuurtContextFactory : IDesignTimeDbContextFactory<BuurtAppContext>
    {
        public BuurtAppContext CreateDbContext(string[] args)
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json")
                .Build(); //get configurations
            string conn = configuration.GetConnectionString("BuurtAppContextConnection"); //get connection string from configurations
            var optionsBuilder = new DbContextOptionsBuilder<BuurtAppContext>();
            optionsBuilder.UseMySql(conn); //build options

            return new BuurtAppContext(optionsBuilder.Options); //return clean context with options parameter
        }
    }
}
