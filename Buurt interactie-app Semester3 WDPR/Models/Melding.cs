using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buurt_interactie_app_Semester3_WDPR.Models
{
    public class Melding : IMelding
    {
        public Buurtbewoner Buurtbewoner { get; set; }

        public string BuurtbewonerId { get; set; }

        public List<Likes> Likes { get; set; }

        public List<UserView> AantalViews { get; set; }

        public List<Reactie> Reacties { get; set; }
    }

    public class Categorie
    {
        [Key]
        public string Naam { get; set; }
    }

    public class Likes
    {
        public Melding Melding { get; set; }
        public int MeldingId { get; set; }

        public Buurtbewoner Buurtbewoner { get; set; }
        public string BuurtbewonerId { get; set; }

    }

    public class UserView
    {
        public Melding Melding { get; set; }
        public int MeldingId { get; set; }

        public Buurtbewoner Buurtbewoner { get; set; }
        public string BuurtbewonerId { get; set; }
    }
}
