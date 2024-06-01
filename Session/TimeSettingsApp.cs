using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Session
{
    public  class TimeSettingsApp : ApplicationSettingsBase
    {
        [UserScopedSetting()]
        [DefaultSettingValue("30")]
        public  int Time
        {
            get
            {
                return (int)this["Time"];
            }
            set
            {
                this["Time"] = value;
            }
        }
    }
}
