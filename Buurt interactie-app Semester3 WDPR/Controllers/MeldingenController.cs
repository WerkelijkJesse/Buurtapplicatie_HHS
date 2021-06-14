using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Buurt_interactie_app_Semester3_WDPR.Data;
using Buurt_interactie_app_Semester3_WDPR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Buurt_interactie_app_Semester3_WDPR.ViewModel;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Buurt_interactie_app_Semester3_WDPR
{
    [Authorize(Roles = "Default, Moderator, Administrator")]
    public class MeldingenController : Controller
    {
        private readonly BuurtAppContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<BuurtAppUser> _um;
        private readonly SignInManager<BuurtAppUser> _sm;
        private readonly ILogger<MeldingenController> _logger;

        public MeldingenController(
            UserManager<BuurtAppUser> userManager,
            SignInManager<BuurtAppUser> signInManager,
            ILogger<MeldingenController> logger, BuurtAppContext context, IWebHostEnvironment webHostEnvironment)
        {
            _um = userManager;
            _sm = signInManager;
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            DBInitializer.Seed(_context, _um, _sm);
        }

        //Neemt pagineer, sorteer en filter attributen op, geeft een gefilterde gesorteerde en gepagineerde lijst terug van alle publieke meldingen.
        public async Task<IActionResult> Index(int? pagina, string? sort, DateTime? from, DateTime? to, string? zoek, bool likes, bool closed)
        {
            int page = pagina ?? 0;
            string sorting = sort ?? "nieuwste";
            Filter filt = new Filter(closed, likes, zoek ?? "", new DateRange(from ?? new DateTime(2021, 1, 1), to ?? DateTime.Today));
            ViewData["Pagina"] = page;
            ViewData["Sort"] = sorting;
            ViewData["Filter"] = filt;
            IQueryable<Melding> lijst = _context.Melding.Include(m => m.Categorie)
                .Include(m => m.Likes)
                .Include(m => m.AantalViews)
                .Include(m => m.Buurtbewoner);

            lijst = await FilterAsync(lijst, filt);
            lijst = Sort(lijst, sorting);

            return View(await ViewModel.GepagineerdeLijst<Melding>.CreateAsync(lijst, page));
        }

        //Neemt een lijst en een sorteervolgorde op, geeft gesorteerde lijst terug
        private IQueryable<Melding> Sort(IQueryable<Melding> lijst, string sort)
        {
            switch (sort)
            {
                case ("nieuwste"): lijst = lijst.OrderByDescending(m => m.Tijdstip); break;
                case ("oudste"): lijst = lijst.OrderBy(m => m.Tijdstip); break;
                case ("meestelikes"): lijst = lijst.OrderByDescending(m => m.Likes.Count()); break;
                case ("meesteviews"): lijst = lijst.OrderByDescending(m => m.AantalViews.Count()); break;
                case ("alfabetisch"): lijst = lijst.OrderBy(m => m.Titel); break;
                default: lijst = lijst.OrderByDescending(m => m.Tijdstip); break;
            }
            return lijst;
        }

        //Neemt een lijst van meldingen en een zoekterm en geeft een lijst gefiltert daarop terug
        public IQueryable<Melding> Zoek (IQueryable<Melding> lijst, string zoekterm)
        {
            lijst = lijst.Where(m => m.Titel.ToLower().Contains(zoekterm.ToLower()) || m.Tekst.ToLower().Contains(zoekterm.ToLower()) || m.CategorieNaam.ToLower().Contains(zoekterm.ToLower()));
            return lijst;
        }

        //Neemt een lijst op en een filter object, geeft gefilterde lijst terug
        public async Task<IQueryable<Melding>> FilterAsync(IQueryable<Melding> lijst, Filter filter)
        {
            lijst = lijst.Where(m => m.Open == !filter.Gesloten && m.Tijdstip <= filter.Range.Eind.Add(new TimeSpan(23, 59, 59)) && m.Tijdstip >= filter.Range.Start);
            lijst = Zoek(lijst, filter.Zoekterm);

            if ((bool)filter.Geliket)
            {
                var buurtuser = await _um.GetUserAsync(HttpContext.User);
                var buurtbew = _context.Buurtbewoners
                    .Include(b => b.Liked)
                    .ThenInclude(l => l.Melding)
                    .Single(b => b.Id == buurtuser.Id);

                var lijstsize = lijst.Count();
                var zoeklijst = lijst;
                var newlijst = lijst.Take(0);
                foreach(var l in buurtbew.Liked)
                {
                    newlijst = newlijst.Concat(zoeklijst.Where(m => m.Likes.SingleOrDefault(like => like.BuurtbewonerId == l.BuurtbewonerId && like.MeldingId == l.MeldingId) != null));
                }
                return newlijst;
                
            }

            return lijst;
        }

        //GET: Meldingen/Create
        //Returnt de create view met de lijst van categoriën
        public IActionResult Create()
        {
            ViewData["CategorieNaam"] = new SelectList(_context.Set<Categorie>(), "Naam", "Naam");
            return View();
        }

        // POST: Meldingen/Create
        // Stopt alle input velden in een melding object en savet naar de database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titel,Tekst,CategorieNaam,Open,Tijdstip,Afbeelding")] Melding melding)
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
                melding.Reacties = new List<Reactie>();
                melding.Likes = new List<Likes>();
                melding.AantalViews = new List<UserView>();
                melding.Buurtbewoner = (Buurtbewoner)await _um.GetUserAsync(HttpContext.User);
                _context.Add(melding);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategorieNaam"] = new SelectList(_context.Set<Categorie>(), "Naam", "Naam", melding.CategorieNaam);
            return View(melding);
        }


        // GET: Meldingen/Details/5
        //Neemt een id op, returnt indien gevonden de details view met de desbetreffende melding
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var melding = await _context.Melding
                .Include(m => m.Categorie)
                .Include(m => m.Buurtbewoner)
                .Include(m => m.Reacties)
                .ThenInclude(r => r.Buurtbewoner)
                .Include(m => m.Likes)
                .Include(m => m.AantalViews)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (melding == null)
            {
                return NotFound();
            }

            return View(melding);
        }

        public async Task<IActionResult> MijnMeldingen(int? pagina, string? sort, DateTime? from, DateTime? to, string? zoek, bool likes, bool closed)
        {
            int page = pagina ?? 0;
            string sorting = sort ?? "nieuwste";
            Filter filt = new Filter(closed, likes, zoek ?? "", new DateRange(from ?? new DateTime(2021, 1, 1), to ?? DateTime.Today));
            ViewData["Pagina"] = page;
            ViewData["Sort"] = sorting;
            ViewData["Filter"] = filt;
            var user = await _um.GetUserAsync(HttpContext.User);
            IQueryable<Melding> lijst = _context.Melding.Where(m => m.BuurtbewonerId == user.Id).Include(m => m.Categorie)
                .Include(m => m.Likes)
                .Include(m => m.AantalViews)
                .Include(m => m.Buurtbewoner);

            lijst = await FilterAsync(lijst, filt);
            lijst = Sort(lijst, sorting);

            return View(await ViewModel.GepagineerdeLijst<Melding>.CreateAsync(lijst, page));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LikeMelding(int id, [FromBody] Likes like)
        {
            if(like.MeldingId != id)
            {
                return NotFound();
            }
            var melding = _context.Melding.Include(m => m.Likes).SingleOrDefault(m => m.Id == id);
            if (melding == null)
            {
                return NotFound();
            }
            if (_context.Melding.Find(like.MeldingId).BuurtbewonerId == like.BuurtbewonerId) //gebruiker mag zijn eigen post niet liken
            {
                return NotFound();
            }
            

            melding.Likes.Add(like);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception)
            {
                if (!MeldingExists(id))
                {
                    return NotFound();
                }
                else if (melding.Likes.Where(l => l.BuurtbewonerId == like.BuurtbewonerId).Count() > 1) //de like staat al in de context dus als er al een like vd gebruiker was zijn er nu  > 1
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Json(like);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReageerMelding(int id, [FromBody] Reactie reactie)
        {
            var melding = _context.Melding.Include(m => m.Reacties).SingleOrDefault(m => m.Id == id);
            if(melding == null)
            {
                return NotFound();
            }
            else if(melding.Id != reactie.MeldingId)
            {
                return NotFound();
            }
            reactie.Tijdstip = DateTime.Now;
            melding.Reacties.Add(reactie);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeldingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Json(reactie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReactie(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if(_context.Reactie.FirstOrDefault(r => r.Id == id) == null)
            {
                return NotFound();
            }
            int idn = id ?? 0;
            var roles = await _um.GetRolesAsync(await _um.GetUserAsync(HttpContext.User));
            if(roles.Contains("Moderator") || roles.Contains("Administrator") || ((await _context.Reactie.FindAsync(id)).BuurtbewonerId == (await _um.GetUserAsync(HttpContext.User)).Id)) //alleen de admin, mods of owner van melding
            {
                    _context.Reactie.Remove(await _context.Reactie.FirstOrDefaultAsync(r => r.Id == idn));
                    await _context.SaveChangesAsync();
                    return Ok();
            }
            return NotFound();

        }

        // GET: Meldingen/Edit/5
        //Neemt een id op en returnt de Edit view met de desbetreffende melding
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if ((await _context.Melding.FindAsync(id)).BuurtbewonerId != _um.GetUserId(HttpContext.User))
            {
                return RedirectToAction(nameof(Index));
            }

            var melding = await _context.Melding.FindAsync(id);
            if (melding == null)
            {
                return NotFound();
            }
            ViewData["CategorieNaam"] = new SelectList(_context.Set<Categorie>(), "Naam", "Naam", melding.CategorieNaam);
            return View(melding);
        }

        // POST: Meldingen/Edit/5
        //Neemt een id en een melding op en bewerkt deze in de database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Tekst,CategorieNaam,Open,Tijdstip,Afbeelding")] Melding melding)
        {
            var loggedIn = await _um.GetUserAsync(HttpContext.User);
            if ((await _context.Melding.FindAsync(id)).BuurtbewonerId != loggedIn.Id && !(await _um.GetRolesAsync(loggedIn)).Contains("Moderator") && !(await _um.GetRolesAsync(loggedIn)).Contains("Administrator"))
            {
                return RedirectToAction(nameof(MijnMeldingen));
            }
            if (id != melding.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var og = await _context.Melding.FirstOrDefaultAsync(m => m.Id == id);
                if (!_context.Melding.First(m => m.Id == id).Open)
                {
                    return RedirectToAction(nameof(MijnMeldingen));
                }
                try
                {
                    if (melding.Afbeelding != null)
                    {
                        var uniqueFileName = IMelding.GetUniqueFileName(melding.Afbeelding.FileName);
                        var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        var filePath = Path.Combine(uploads, uniqueFileName);
                        await melding.Afbeelding.CopyToAsync(new FileStream(filePath, FileMode.Create));
                        melding.AfbeeldingURL = uniqueFileName;
                    }
                    else
                    {
                        melding.Afbeelding = og.Afbeelding;
                        melding.AfbeeldingURL = og.AfbeeldingURL;
                    }
                    melding.Tijdstip = og.Tijdstip;
                    melding.Buurtbewoner = og.Buurtbewoner;
                    melding.Open = true;
                    melding.Reacties = og.Reacties;
                    melding.Likes = og.Likes;
                    melding.AantalViews = og.AantalViews;
                    melding.Categorie = _context.Categorie.SingleOrDefault(c => c.Naam == melding.CategorieNaam);
                    var local = _context.Set<Melding>()
                    .Local
                    .FirstOrDefault(entry => entry.Id.Equals(id));
                    if (local != null)
                    {
                        // detach
                        _context.Entry(local).State = EntityState.Detached;
                    }
                    _context.Entry(melding).State = EntityState.Modified;
                    _context.Update(melding);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if (!MeldingExists(melding.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["CategorieNaam"] = new SelectList(_context.Set<Categorie>(), "Naam", "Naam", melding.CategorieNaam);
            return RedirectToAction(nameof(MijnMeldingen));
        }

        // GET: Meldingen/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var melding = await _context.Melding
                .Include(m => m.Categorie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (melding == null)
            {
                return NotFound();
            }
            var loggedIn = await _um.GetUserAsync(HttpContext.User);
            if ((await _context.Melding.FindAsync(id)).BuurtbewonerId != loggedIn.Id && !(await _um.GetRolesAsync(loggedIn)).Contains("Moderator") && !(await _um.GetRolesAsync(loggedIn)).Contains("Administrator"))
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
            var loggedIn = await _um.GetUserAsync(HttpContext.User);
            if ((await _context.Melding.FindAsync(id)).BuurtbewonerId != loggedIn.Id && !(await _um.GetRolesAsync(loggedIn)).Contains("Moderator") && !(await _um.GetRolesAsync(loggedIn)).Contains("Administrator"))
            {
                return RedirectToAction(nameof(Index));
            }
            var melding = _context.Melding.FirstOrDefault(m => m.Id == id);
            if(melding == null)
            {
                return NotFound();
            }
            _context.Melding.Remove(melding);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MeldingExists(int id)
        {
            return _context.Melding.Any(e => e.Id == id);
        }
    }
}
