using Newtonsoft.Json;
using System.Collections;

namespace Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Pages.Account
{
    internal class CaptchaVerification
    {
        public CaptchaVerification()
        {
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public IList Errors { get; set; }
    }
}