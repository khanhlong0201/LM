using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM.Models
{
    public class PaginationModel
    {
        public int CurrentPage { get; set; }
        public int Count { get; set; }
        public int Pagsize { get; set; }
        public int TotalPage { get; set; }
        public int IndexOne { get; set; }
        public int IndexTwo { get; set; }
        public bool ShowPrevious => CurrentPage > 1;
        public bool ShowFirst => CurrentPage != 1;
        public bool ShowLast => CurrentPage != TotalPage;
    }
}
