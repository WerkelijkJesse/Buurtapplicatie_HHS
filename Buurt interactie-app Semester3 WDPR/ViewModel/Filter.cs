using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.ViewModel
{
    public class Filter
    {
        public bool Gesloten { get; set; }
        public bool Geliket { get; set; }
        public string Zoekterm { get; set; }
        public DateRange Range { get; set; }

        public Filter()
        {
            if (Zoekterm == null) Zoekterm = "";
            if (Range == null) Range = new DateRange(new DateTime(2021, 1, 1), DateTime.Now);
        }

        public Filter(bool gesloten, bool geliket, string zoekterm, DateRange range)
        {
            this.Gesloten = gesloten;
            this.Geliket = geliket;
            this.Zoekterm = zoekterm;
            this.Range = range;
        }
    }
}
