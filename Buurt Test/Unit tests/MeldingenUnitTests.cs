using Buurt_interactie_app_Semester3_WDPR;
using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using Buurt_interactie_app_Semester3_WDPR.Data;
using Buurt_interactie_app_Semester3_WDPR.Models;
using Buurt_Test.Unit_tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Buurt_Test
{
    public class MeldingenUnitTests
    {
        private readonly SignInManager<BuurtAppUser> _sm = new MockSignInManager();
        private readonly UserManager<BuurtAppUser> _um = new MockUserManager();
        private readonly Mock<ILogger<MeldingenController>> _logger = new Mock<ILogger<MeldingenController>>();
        private readonly Mock<IWebHostEnvironment> _webHostEnvironment = new Mock<IWebHostEnvironment>();

        public static BuurtAppContext GetInMemoryDBMetData()
        {
            var db = new DBInitializer();
            BuurtAppContext context = db.GetNewInMemoryDatabase(true);
            Buurtbewoner vlad = new Buurtbewoner { Id = "1", Naam = "Vladimir", Email = "lenin@ussr.ru" };
            Buurtbewoner jozef = new Buurtbewoner { Id = "2", Naam = "Jozef", Email = "stalin@ussr.ru" };
            Melding melding1 = new Melding {
                Buurtbewoner = vlad,
                Categorie = new Categorie { Naam = "overlast" },
                Titel = "Titeltje",
                Tekst = "Tekstje",
                Tijdstip = new DateTime(2021, 1, 1, 12, 0, 0),
                Likes = new List<Likes>()
                };
            context.Add(vlad);
            context.Add(jozef);
            context.Add(melding1);
            context.SaveChanges();
            return db.GetNewInMemoryDatabase(false); // gebruik een nieuw (clean) object voor de context
        }

        //Return een controllercontext nagebootst met een role aan de user
        public static ControllerContext getFakeHttpContext(string role)
        {
            var conContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "IngelogdeTester"),
                            new Claim(ClaimTypes.Role, role)
                        }))
                }
            };
            return conContext;
        }


        [Fact]
        public async Task CreateReturnView_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var result = c.Create();
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Null(viewResult.ViewName);
            }
        }

        //Test het returnen van een redirect in het geval van een valide model
        [Fact]
        public async Task Valid_Melding_Create()
        {

           using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                c.ControllerContext = getFakeHttpContext("Default");
                Melding createMelding = new Melding
                {
                    Buurtbewoner = context.Buurtbewoners.Find("1"),
                    Categorie = new Categorie { Naam = "overlast" },
                    Titel = "Titeltje",
                    Tekst = "Tekstje",
                    Tijdstip = new DateTime(2021, 1, 1, 12, 0, 0),
                };
                var result = await c.Create(createMelding);
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result); //bij een valide create wordt geredirect naar Index
                Assert.Null(redirectToActionResult.ControllerName); //we blijven in dezelfde controller
                Assert.Equal("Index", redirectToActionResult.ActionName);
            }

        }

        //test het returnen van viewresult in het geval van een invalide model
        [Fact]
        public async Task Invalid_Melding_Create()
        {

            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                Melding createMelding = new Melding
                {
                    Buurtbewoner = context.Buurtbewoners.Find("1"),
                    Categorie = new Categorie { Naam = "overlast" },
                    Titel = "Titeltje",
                    Tekst = "Tekstje",
                    Tijdstip = new DateTime(2021, 1, 1, 12, 0, 0),
                };
                c.ModelState.AddModelError("Error", "Tekst te kort");
                var result = await c.Create(createMelding);
                var viewResult = Assert.IsType<ViewResult>(result); //bij een invalide create wordt de view gereturnt
                Assert.Null(viewResult.ViewName); //we blijven in dezelfde action
            }

        }

        //test het returnen van viewresult met verschillende parameters vor index
        [Theory]
        [InlineData(1, "nieuwste", null, null, "zoek", false, false)]
        [InlineData(0, "", null, null, "", false, false)]
        [InlineData(0, "nieuwste", null, null, "zoek", false, false)]
        [InlineData(1, "", null, null, "1234", false, false)]
        [InlineData(0, "nieuwste", null, null, "", false, false)]
        public async Task IndexResult_Test(int pagina, string sort, DateTime? from, DateTime? to, string zoek, bool likes, bool closed) 
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                c.ControllerContext = getFakeHttpContext("Default");
                var result = await c.Index(pagina, sort, from, to, zoek, likes, closed);
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Null(viewResult.ViewName); //we blijven in dezelfde action
            }
        }

        //Test het returnen van notfoundresult zonder id of met een niet bestaand id
        [Theory]
        [InlineData(null)]
        [InlineData(123456)]
        public async Task Details_IdNullResult_Test(int? id)
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var result = await c.Details(id);
                var notFoundResult = Assert.IsType<NotFoundResult>(result);
            }
        }

        //Test het returnen van een viewresult Details met een bestaand id
        [Fact]
        public async Task Details_ExistingIdResult_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var result = await c.Details(1);
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Null(viewResult.ViewName);
            }
        }


        //Test het returnen van notfoundresult in het geval dat de owner van melding ook de owner van de like is
        [Fact]
        public void LikeMeldingInvalidByOwner_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var like = new Likes { 
                    BuurtbewonerId = "1",
                    MeldingId = 1
                };
                var result = c.LikeMelding(1, like);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public void LikeMeldingInvalidNonExistingId_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var like = new Likes { BuurtbewonerId = "2", MeldingId = 123456 };
                var result = c.LikeMelding(123456, like);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public void LikeMeldingInvalidId_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var like = new Likes { BuurtbewonerId = "2", MeldingId = 1 };
                var result = c.LikeMelding(123456, like);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public void LikeMeldingValid_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var like = new Likes { BuurtbewonerId = "2", MeldingId = 1 };
                var result = c.LikeMelding(1, like);
                Assert.IsType<JsonResult>(result);
            }
        }

        [Fact]
        public void ReageerMeldingInvalidNonExistingId_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var reactie = new Reactie { BuurtbewonerId = "2", MeldingId = 123456 };
                var result = c.ReageerMelding(123456, reactie);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public void ReageerMeldingInvalidReactie_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var reactie = new Reactie { BuurtbewonerId = "2", MeldingId = 3 };
                var result = c.ReageerMelding(1, reactie);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public void ReageerMeldingValid_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                var reactie = new Reactie { BuurtbewonerId = "2", MeldingId = 1 };
                var result = c.ReageerMelding(1, reactie);
                Assert.IsType<JsonResult>(result);
            }
        }

        [Fact]
        public async Task DeleteReactieValid_Test()
        {
            using (var context = GetInMemoryDBMetData())
            {
                var c = new MeldingenController(_um, _sm, _logger.Object, context, _webHostEnvironment.Object);
                c.ControllerContext = getFakeHttpContext("Moderator");
                context.Reactie.Add(new Reactie {
                    BuurtbewonerId = "2",
                    MeldingId = 1,
                    Tekst = "Reactietje"
                });
                await context.SaveChangesAsync();

                var result = await c.DeleteReactie(1);
                Assert.IsType<OkResult>(result);

            }
        }

    }
}
