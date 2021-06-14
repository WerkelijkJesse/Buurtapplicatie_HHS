using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.Validation
{
    public class MeldingValidationAttribute : ValidationAttribute
    {
        public MeldingValidationAttribute()
        {

        }

        //Returnt of een string alleen alphanumerics bevat (True)
        private bool IsAlphaNumeric(string str)
        {
            Regex rx = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return rx.IsMatch(str);
        }

        protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
        {
            var tekst = value as string;
            if (tekst != null)
            {
                if (!(tekst.Any(char.IsLower) && tekst.Any(char.IsLower) && !IsAlphaNumeric(tekst)) || tekst.Any(char.IsWhiteSpace))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Wachtwoord moet voldoen aan de eisen: Minimaal 1 hoofdletter, minimaal 1 kleine letter, minimaal 1 cijfer en minimaal 1 speciaal teken(!-'?&-_*#$/)";
        }
    }
}
