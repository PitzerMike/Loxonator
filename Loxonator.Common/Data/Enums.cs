using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Loxonator.Common.Data
{
    public enum NodeType
    {
        [Description("Schalten/Tasten EIS1")]
        EIS1 = 0,
        [Description("Dimmen EIS2")]
        EIS2 = 1,
        [Description("Zeit EIS3")]
        EIS3 = 2,
        [Description("Datum EIS4")]
        EIS4 = 3,
        [Description("Sensor/Analogwert EIS5")]
        EIS5 = 4,
        [Description("Dimmen/Position EIS6")]
        EIS6 = 5,
        [Description("Jalousie EIS7")]
        EIS7 = 6,
        [Description("Sensor/Analogwert EIS9")]
        EIS9 = 8,
        [Description("Sensor/Analogwert EIS10")]
        EIS10 = 9,
        [Description("Sensor/Analogwert EIS11")]
        EIS11 = 10
    }
}
