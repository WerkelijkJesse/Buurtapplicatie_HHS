using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buurt_interactie_app_Semester3_WDPR.ViewModel
{
    public class DateRange
    {
        public DateTime Start { get; set; }
        public DateTime Eind { get; set; }

        public DateRange(DateTime start, DateTime eind)
        {
            this.Start = start;
            this.Eind = eind;
        }

    }
}
