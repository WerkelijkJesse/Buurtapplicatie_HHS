using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Buurt_interactie_app_Semester3_WDPR.ViewModel
{
    public class GepagineerdeLijst<T> : List<T>
    {
        public int Pagina { get; private set; }
        public int PaginaAantal { get; private set; }
        public GepagineerdeLijst(List<T> lijstDeel, int totaalAantal, int pagina)
        {
            Pagina = pagina;
            PaginaAantal = (int)Math.Ceiling(totaalAantal / (double)10);
            this.AddRange(lijstDeel);
        }

        public bool HeeftVorige() { return Pagina > 0; }
        public bool HeeftVolgende() { return Pagina < PaginaAantal - 1; }
        public static async Task<GepagineerdeLijst<T>> CreateAsync(IQueryable<T> lijst, int pagina)
        {
            return new GepagineerdeLijst<T>(
                await lijst.Skip(pagina * 10).Take(10).ToListAsync(),
                await lijst.CountAsync(),
                pagina);
        }
    }
}