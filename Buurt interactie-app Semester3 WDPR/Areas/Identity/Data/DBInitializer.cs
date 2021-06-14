using Buurt_interactie_app_Semester3_WDPR.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data
{
    public class DBInitializer
    {

        public static void Seed(BuurtAppContext context, UserManager<BuurtAppUser> um, SignInManager<BuurtAppUser> sm)
        {
            if (!context.Categorie.Any())
            {
                context.Categorie.Add(new Models.Categorie() { Naam = "Verkeer" });
                context.Categorie.Add(new Models.Categorie() { Naam = "Vandalisme" });
                context.Categorie.Add(new Models.Categorie() { Naam = "Geluidsoverlast" });
                context.Categorie.Add(new Models.Categorie() { Naam = "Ander soort overlast" });
                context.SaveChanges();
            }
            //var reactie = new Models.Reactie() { Melding = context.Melding.Find(1), Tekst = "1Dit is een test reactie", Tijdstip = DateTime.Now };
            //reactie.Buurtbewoner = (Models.Buurtbewoner) await um.FindByIdAsync("1");
            //context.Add(reactie);
                
            //context.Add(new Models.Reactie() { Buurtbewoner = context.Buurtbewoners.Find(1), Id = 2, Melding = context.Melding.Find(1), Tekst = "2Dit is een test reactie", Tijdstip = DateTime.Now });
            //context.Add(new Models.Reactie() { Buurtbewoner = context.Buurtbewoners.Find(1), Id = 3, Melding = context.Melding.Find(1), Tekst = "3Dit is een test reactie", Tijdstip = DateTime.Now });
            context.SaveChanges();
        }
    }
}
