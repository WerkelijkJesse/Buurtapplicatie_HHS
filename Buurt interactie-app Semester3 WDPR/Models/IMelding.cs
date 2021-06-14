using Buurt_interactie_app_Semester3_WDPR.Validation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.Models
{
    public class IMelding
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^(?:.*[a-z])$", ErrorMessage = "Beëindig uw titel niet met speciale tekens.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage ="De titel moet ten minste {2} en maximaal {1} karakters bevatten.")]
        public string Titel { get; set; }

        [Required]
        [RegularExpression(@"[a-zA-Z0-9@#$%&*+\-_(),+':;?.,![\]\s\\/]+$", ErrorMessage = "Het bericht mag bestaan uit: Letters A t/m z, Cijfers (0-9) en speciale tekens (@#$%&*()-_+][';:?.,!)")]
        [StringLength(450, MinimumLength = 10, ErrorMessage = "De tekst moet minimaal {2} en maximaal {1} karakters bevatten.")]
        public string Tekst { get; set; }

        public string AfbeeldingURL { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        [MaxFileSize(3 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
        public IFormFile Afbeelding { get; set; }

        [Required]
        public DateTime Tijdstip { get; set; }

        public Categorie Categorie { get; set; }

        [Required]
        public string CategorieNaam { get; set; }

        public bool Open { get; set; }

        //Neemt een filename in en returnt een unieke filename m.b.v. een Globally Unique Identifier
        public static string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }
    }
}
