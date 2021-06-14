using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Buurt_interactie_app_Semester3_WDPR.Data;
using Buurt_interactie_app_Semester3_WDPR.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace Buurt_interactie_app_Semester3_WDPR.Controllers
{
    [Authorize(Roles = "Administrator, Moderator, Default")]
    public class AnoniemeMeldingsController : Controller
    {
        private readonly BuurtAppContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<BuurtAppUser> _um;
        private readonly SignInManager<BuurtAppUser> _sm;
        private readonly ILogger<AnoniemeMeldingsController> _logger;

        public AnoniemeMeldingsController(
            UserManager<BuurtAppUser> userManager,
            SignInManager<BuurtAppUser> signInManager,
            ILogger<AnoniemeMeldingsController> logger, BuurtAppContext context, IWebHostEnvironment webHostEnvironment)
        {
            _um = userManager;
            _sm = signInManager;
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            DBInitializer.Seed(_context, _um, _sm);
        }

        // GET: Meldingen
        //Neemt paginanummerop en returnt de mijnmeldingen view met een GepagineerdeLijst van anonieme meldingen van de ingelogde gebruiker.
        [Authorize (Roles = "Default")]
        public async Task<IActionResult> MijnMeldingen(int? pagina)
        {
            var loggedIn = await _um.GetUserAsync(HttpContext.User);
            int page = pagina ?? 0;
            ViewData["Pagina"] = page;
            var lijst = _context.AnoniemeMelding.Where(am => am.Sudo == _context.Pseudonyms.FirstOrDefault(p => p.AnoId == loggedIn.AnoNummer).SudoId).OrderByDescending(am => am.Tijdstip);

            return View(await ViewModel.GepagineerdeLijst<AnoniemeMelding>.CreateAsync(lijst, page));
        }
    

        //GET: Meldingen/Create
        //Returnt de create view met de lijst van categoriën
        [Authorize (Roles = "Default")]
        public IActionResult Create()
        {
            ViewData["CategorieNaam"] = new SelectList(_context.Set<Categorie>(), "Naam", "Naam");
            return View();
        }

        // POST: Meldingen/Create
        // Stopt alle input velden in een melding object en savet naar de database
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Default")]
        public async Task<IActionResult> Create([Bind("Id,Titel,Tekst,CategorieNaam,Open,Tijdstip,Afbeelding")] AnoniemeMelding melding)
        {
            if (ModelState.IsValid)
            {
                if (melding.Afbeelding != null)
                {
                    var uniqueFileName = IMelding.GetUniqueFileName(melding.Afbeelding.FileName);
                    var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    var filePath = Path.Combine(uploads, uniqueFileName);
                    melding.Afbeelding.CopyTo(new FileStream(filePath, FileMode.Create));
                    melding.AfbeeldingURL = uniqueFileName;
                }
                melding.Tijdstip = DateTime.Now;
                melding.Open = true;
                melding.Categorie = _context.Categorie.SingleOrDefault(c => c.Naam == melding.CategorieNaam);
                var signedIn = (Buurtbewoner)await _um.GetUserAsync(HttpContext.User);
                melding.Sudo = _context.Pseudonyms.SingleOrDefault( p => p.AnoId == signedIn.AnoNummer).SudoId;
                _context.Add(melding);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MijnMeldingen));
            }
            ViewData["CategorieNaam"] = new SelectList(_context.Set<Categorie>(), "Naam", "Naam", melding.CategorieNaam);
            return View(melding);
        }

        //Neemt een paginanummer op, returnt een gepagineerdelijst van alle anonieme meldingen
        [Authorize (Roles = "Moderator, Administrator")]
        public async Task<IActionResult> Review(int? pagina)
        {
            int page = pagina ?? 0;
            var lijst = _context.AnoniemeMelding.AsQueryable<AnoniemeMelding>();
            return View(await ViewModel.GepagineerdeLijst<AnoniemeMelding>.CreateAsync(lijst, page));
        }

        //Neemt een paginanummer op, returnt een lijst met een koppeling tussen anonieme identiteit van buurtbewoner en pseudoniem
        [Authorize (Roles = "Administrator")]
        public async Task<IActionResult> Pseudo(int? pagina)
        {
            int page = pagina ?? 0;
            var lijst = _context.Pseudonyms.AsQueryable<BuurtbewonerAno>();
            return View(await ViewModel.GepagineerdeLijst<BuurtbewonerAno>.CreateAsync(lijst, page));
        }

        //Neemt een id op en geeft een koppeling weer tussen dat anonieme id en het pseudoniem
        //werkt nog nie
        [HttpPost]
        [Authorize (Roles = "Administrator")]
        public async Task<IActionResult> PseudoDetails(string id)
        {
            if (id == null) { return NotFound(); }

            var anogebruiker = await _context.Pseudonyms.FirstOrDefaultAsync(p => p.AnoId == id);

            if (anogebruiker == null)
            {
                return NotFound();
            }

            var realgebruiker = await _context.Buurtbewoners.FirstOrDefaultAsync(b => b.AnoNummer == id);
            if (realgebruiker != null)
            {
                ViewData["gebr"] = realgebruiker.UserName;
            }
            else ViewData["gebr"] = "NOT FOUND";

            return View(anogebruiker);
        }
        
        // GET: Meldingen/Details/5
        //Neemt een id op, returnt indien gevonden de details view met de desbetreffende melding
        [Authorize (Roles = "Administrator, Moderator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var melding = await _context.AnoniemeMelding
                .Include(am => am.Categorie)
                .FirstOrDefaultAsync(am => am.Id == id);
            if (melding == null)
            {
                return NotFound();
            }

            return View(melding);
        }

        // GET: Meldingen/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var melding = await _context.AnoniemeMelding
                .Include(m => m.Categorie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (melding == null)
            {
                return NotFound();
            }

            var loggedIn = await _um.GetUserAsync(HttpContext.User);
            var sudo = (await _context.AnoniemeMelding.FindAsync(id)).Sudo;

            if (_context.Pseudonyms.FirstOrDefault(p => p.SudoId == sudo).AnoId != loggedIn.AnoNummer && !(await _um.GetRolesAsync(loggedIn)).Contains("Moderator") && !(await _um.GetRolesAsync(loggedIn)).Contains("Administrator"))
            {
                return RedirectToAction(nameof(Index));
            }


            return View(melding);
        }

        // POST: Meldingen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if(_context.AnoniemeMelding.FirstOrDefault(m => m.Id == id) == null)
            {
                return NotFound();
            }
            var loggedIn = await _um.GetUserAsync(HttpContext.User);
            var sudo = (await _context.AnoniemeMelding.FindAsync(id)).Sudo;

            if (_context.Pseudonyms.FirstOrDefault(p => p.SudoId == sudo).AnoId != loggedIn.AnoNummer && !(await _um.GetRolesAsync(loggedIn)).Contains("Moderator") && !(await _um.GetRolesAsync(loggedIn)).Contains("Administrator"))
            {
                return RedirectToAction(nameof(Index));
            }
            var melding = await _context.AnoniemeMelding.FindAsync(id);
            _context.AnoniemeMelding.Remove(melding);
            await _context.SaveChangesAsync();
            var roles = await _um.GetRolesAsync(await _um.GetUserAsync(HttpContext.User));
            if (roles.Contains("Administrator"))
            {
                return RedirectToAction(nameof(Review));
            }
            return RedirectToAction(nameof(MijnMeldingen));
        }

        private bool AnoniemeMeldingExists(int id)
        {
            return _context.AnoniemeMelding.Any(e => e.Id == id);
        }
}
}
