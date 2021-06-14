using Buurt_interactie_app_Semester3_WDPR.Data;
using Buurt_interactie_app_Semester3_WDPR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Buurt_Test
{
    public class DBInitializer
    {
        private string databaseName;

        public BuurtAppContext GetNewInMemoryDatabase(bool NewDb)
        {
            if (NewDb) this.databaseName = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<BuurtAppContext>()
                .UseInMemoryDatabase(this.databaseName)
                .Options;

            return new BuurtAppContext(options);
        }
    }
}
