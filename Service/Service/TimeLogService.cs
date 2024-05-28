using Microsoft.Extensions.Logging;
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
    public class TimeLogService(IRepository repository, ILogger<TimeLogService> logger) : ITimeLogService
    {
        public List<TimeLog> GetTimeLogs()
        {
            List<TimeLog> timeLogs = repository.GetQueryableWithOutTracking<TimeLog>()
                .OrderByDescending(itm => itm.Modified)
                .ToList();
            return timeLogs;
        }

        public TimeLog? GetTimeLogById(int id)
        {
            return GetTimeLogs().First(itm => itm.Id == id);
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
                logger.LogError("Failed to insert time log: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public bool UpdateTimeLog(TimeLog timeLog)
        {
            try
            {
                repository.UpdateRange(new List<TimeLog> { timeLog });
                repository.Save();
                return true;
            }
            catch(Exception ex)
            {
                logger.LogError("Failed to update time log: {message} {exception}", ex.Message, ex);

                return false;
            }
        }
    }
}
