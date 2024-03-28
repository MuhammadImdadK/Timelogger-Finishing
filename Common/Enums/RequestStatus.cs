using System;
using System.Collections.Generic;
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
        UpdateRequested=4
    }

    public enum RequestType
    {
        Project = 0,
        TimeLog = 1
    }
}
