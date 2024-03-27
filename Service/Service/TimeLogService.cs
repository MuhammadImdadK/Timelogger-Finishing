using Model.Interface;
using Model.ModelSql;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class TimeLogService(IRepository repository) : ITimeLogService
    {
        public List<TimeLog> GetTimeLogs()
        {
            List<TimeLog> timeLogs = repository.GetQueryableWithOutTracking<TimeLog>()
                .OrderByDescending(itm => itm.Modified)
                .ToList();
            return timeLogs;
        }

        public List<TimeLog> GetTimeLogsByUserId(int userId)
        {
            return GetTimeLogs().Where(itm => itm.UserID == userId).ToList();
        }

        public bool InsertTimeLog(TimeLog timeLog)
        {
            try
            {
                repository.InsertModel(timeLog);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
