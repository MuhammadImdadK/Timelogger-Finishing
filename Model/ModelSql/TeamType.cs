using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum TeamType
    {
        [Description("None")]
        None=0,
        [Description("Core Team")]
        CoreTeam=1,
        [Description("Additional Team")]
        AdditionalTeam=2
    }
    public enum ApprovalState
    {
        AwaitingApproval=0,
        Accepted=1,
        Rejected=2
    }
}
