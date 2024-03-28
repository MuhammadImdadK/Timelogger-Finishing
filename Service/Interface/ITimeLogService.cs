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
        public TimeLog? GetTimeLogById(int id);
        public List<TimeLog> GetTimeLogsByUserId(int userId);
        public bool InsertTimeLog(TimeLog timeLog);
        public bool UpdateTimeLog(TimeLog timeLog);
    }
}
