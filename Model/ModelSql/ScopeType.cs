using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum ScopeType
    {
        None = 0,
        Installation = 1,
        Demolation = 2,
        [Description("As Built")]
        AsBuilt = 3,
        Relocation = 4,
        Standard = 5,
    }
}
