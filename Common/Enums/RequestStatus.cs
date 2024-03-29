using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum RequestStatus
    {
        None = 0,
        Open = 1,
        Accepted = 2,
        Rejected = 3,
        [Description("Update Requested")]
        UpdateRequested=4
    }

    public enum RequestType
    {
        Project = 0,
        [Description("Time Log")]
        TimeLog = 1
    }
}
