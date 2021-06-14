using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Buurt_interactie_app_Semester3_WDPR.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the BuurtAppUser class
    public class BuurtAppUser : IdentityUser
    {
        [Required]
        [StringLength(32, ErrorMessage = "Uw naam moet ten minste {2} en maximaal {1} karakters bevatten.", MinimumLength = 5)]
        public string Naam { get; set; }

        [StringLength(6, ErrorMessage = "Uw postcode moet {1} karakters bevatten.", MinimumLength = 6)]
        [SegbroekValidation]
        public string Postcode { get; set; }

        [Required]
        public string AnoNummer { get; set; }
        public bool Deleted { get; set; }
        public DateTime DeleteDate { get; set; }
        
    }
}
