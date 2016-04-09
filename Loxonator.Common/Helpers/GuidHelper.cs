using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loxonator.Common.Helpers
{
    public static class GuidHelper
    {
        public static string NewGuid()
        {
            return Guid.NewGuid().ToString().Remove(23, 1); // komishes Format, wo der letzte - fehlt
        }
    }
}
