using Model.ModelSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface ITimeLogService
    {
        public List<TimeLog> GetTimeLogs();
        public List<TimeLog> GetTimeLogsByUserId(int userId);
        public bool InsertTimeLog(TimeLog timeLog);
    }
}
