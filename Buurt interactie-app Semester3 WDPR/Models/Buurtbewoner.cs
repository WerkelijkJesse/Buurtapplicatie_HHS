using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.Models
{
    public class Buurtbewoner : BuurtAppUser
    {
        public List<Reactie> Reacties { get; set; }
        public List<Melding> Meldingen { get; set; }
        public List<Likes> Liked { get; set; }
        public List<UserView> Viewed { get; set; }
    }
}
