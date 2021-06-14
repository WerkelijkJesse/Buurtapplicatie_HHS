using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using Buurt_interactie_app_Semester3_WDPR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Buurt_interactie_app_Semester3_WDPR.Data
{
    public class BuurtAppContext : IdentityDbContext<BuurtAppUser>
    {
        public BuurtAppContext(DbContextOptions<BuurtAppContext> options)
            : base(options)
        {
        }

        public DbSet<Buurtbewoner> Buurtbewoners { get; set; }
        public DbSet<Melding> Melding { get; set; }
        public DbSet<AnoniemeMelding> AnoniemeMelding { get; set; }
        public DbSet<BuurtbewonerAno> Pseudonyms { get; set; }
        public DbSet<Reactie> Reactie { get; set; }
        public DbSet<Categorie> Categorie { get; set; }
        public DbSet<Likes> Likes { get; set; }
        public DbSet<UserView> UserViews { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<BuurtbewonerAno>()
                .HasKey(b => new { b.AnoId });

            builder.Entity<Likes>()
            .HasKey(l => new { l.MeldingId, l.BuurtbewonerId });

            builder.Entity<UserView>()
            .HasKey(l => new { l.MeldingId, l.BuurtbewonerId });

            builder.Entity<Melding>()
                .HasOne(m => m.Buurtbewoner)
                .WithMany(b => b.Meldingen)
                .HasForeignKey(m => m.BuurtbewonerId);

            builder.Entity<Reactie>()
                .HasOne(r => r.Melding)
                .WithMany(m => m.Reacties)
                .HasForeignKey(r => r.MeldingId);

            builder.Entity<Buurtbewoner>()
                .HasMany(b => b.Reacties)
                .WithOne(r => r.Buurtbewoner)
                .HasForeignKey(r => r.BuurtbewonerId);
        }
    }
}
