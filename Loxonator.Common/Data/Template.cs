using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Loxonator.Common.Data
{
    public class Template
    {
        public string Cr;
        public string Pr;
        public string Ugr;
        public string Ugx;
        public List<XElement> OriginalNodes = new List<XElement>();
    }
}
