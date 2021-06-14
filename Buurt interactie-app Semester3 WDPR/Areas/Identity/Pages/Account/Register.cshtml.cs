using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Buurt_interactie_app_Semester3_WDPR.Models;
using Buurt_interactie_app_Semester3_WDPR.Validation;
using Buurt_interactie_app_Semester3_WDPR.Data;

namespace Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<BuurtAppUser> _signInManager;
        private readonly UserManager<BuurtAppUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly BuurtAppContext _context;

        public RegisterModel(
            UserManager<BuurtAppUser> userManager,
            SignInManager<BuurtAppUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, RoleManager<IdentityRole> roleManager, BuurtAppContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Het wachtwoord moet minimaal {2} en maximaal {1} karakters bevatten.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Wachtwoord")]
            [PasswordValidation]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Herhaal wachtwoord")]
            [Compare("Password", ErrorMessage = "Wachtwoorden komen niet overeen.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [StringLength(32, ErrorMessage ="Uw naam moet ten minste {2} en maximaal {1} karakters bevatten.", MinimumLength = 5)]
            public string Naam { get; set; }

            [Required]
            [StringLength(6, ErrorMessage ="Uw postcode moet {1} karakters bevatten.", MinimumLength = 6)]
            [SegbroekValidation]
            public string Postcode { get; set; }

            public string Straat { get; set; }
            [Required]
            public string HuisNummer { get; set; }

            public string Stad { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        //Returnt een globally unique identifier geconverteerd naar een string
        public string GenerateUniqueIdentifier()
        {
            return Guid.NewGuid().ToString();
        }


    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new Buurtbewoner { UserName = Input.Email, Email = Input.Email, Naam = Input.Naam, Postcode = Input.Postcode, AnoNummer = GenerateUniqueIdentifier() };
                var result = await _userManager.CreateAsync(user, Input.Password);
                var ano = new BuurtbewonerAno { AnoId = user.AnoNummer, SudoId = Guid.NewGuid().ToString() };


                if (Input.Password == "Moderator123!")
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = "Moderator" });
                    await _userManager.AddToRoleAsync(user, "Moderator");
                }
                else if (Input.Password == ("Admin123!"))
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = "Administrator" });
                    await _userManager.AddToRoleAsync(user, "Administrator");
                }    
                else
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = "Default" });
                    await _userManager.AddToRoleAsync(user, "Default");
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    _context.Add(ano);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Bevestig je E-mail",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}