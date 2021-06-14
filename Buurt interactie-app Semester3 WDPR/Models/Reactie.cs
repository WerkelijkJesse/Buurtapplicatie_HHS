using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.Models
{
    public class Reactie
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^(?:.*[a-z]){3,}$", ErrorMessage = "Het bericht moet minimaal 3 karakters bevatten.")]
        public string Tekst { get; set; }

        public DateTime Tijdstip { get; set; }

        public Melding Melding { get; set; }

        public int MeldingId { get; set; }

        public Buurtbewoner Buurtbewoner { get; set; }

        public string BuurtbewonerId { get; set; }
    }
}
