using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.Validation
{
    public class SegbroekValidationAttribute : ValidationAttribute
    {
        public SegbroekValidationAttribute()
        {

        }

        protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
        {
            var postcode = value as string;
            if (postcode != null)
            {
                if(postcode.Contains("2561") || postcode.Contains("2562") || postcode.Contains("2563"))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(GetErrorMessage());
        }

        public string GetErrorMessage()
        {
            return $"Sorry, op dit moment is de BuurtApp alleen beschikbaar voor inwoners van het Valkenbos- en Regentessekwartier.";
        }
    }
}
