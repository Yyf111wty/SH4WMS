using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
{
    public class MenusModel
    {
        public Menus header { get; set; }
    }

    public class Menus
    {
        public List<Modular> menus { get; set; }
    }

    public class Modular
    {
        public string menuid { get; set; }
        public string icon { get; set; }
        public string menuname { get; set; }
        public string NodeType { get; set; }
        public List<Page> menus { get; set; }
    }

    public class Page
    {
        public string menuid { get; set; }
        public string menuname { get; set; }
        public string icon { get; set; }
        public string url { get; set; }
        public string NodeType { get; set; }
    }
}