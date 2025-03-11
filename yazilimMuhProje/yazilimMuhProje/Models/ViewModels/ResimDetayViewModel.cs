using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using yazilimMuhProje.Models.Entities;

namespace yazilimMuhProje.Models.ViewModels
{
    public class ResimDetayViewModel
    {
        public Resimler Resim { get; set; }
        public List<Yorumlar> Yorumlar { get; set; }
    }
}