using Microsoft.Extensions.Logging;
using Model.Interface;
using Model.ModelSql;
using Service.Interface;

namespace Service.Service
{
    public class ActivityTypeService(IRepository repository, ILogger<AttachmentService> logger) : IActivityTypeService
    {
        public bool DeleteActivityType(ActivityType activityType)
        {
            try
            {
                if (activityType == null)
                {
                    return false;
                }

                repository.RemoveRange<ActivityType>(new List<ActivityType> { activityType });
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to delete Activity Type: {message} {exception}", ex.Message, ex);

                return false;
            }
        }

        public List<ActivityType> GetActivityTypes()
        {
            var response = repository.GetQueryableWithOutTracking<ActivityType>().ToList();
            var defaults = response.Where(x => x.IsDefault).OrderByDescending(x => x.Modified);
            var nonDefaults = response.Where(x => !x.IsDefault).OrderByDescending(x => x.Modified);
            var compiled = new List<ActivityType>();
            compiled.AddRange(defaults);
            compiled.AddRange(nonDefaults);
            return compiled;
        }

        public bool InsertActivityType(ActivityType activityType)
        {
            try
            {
                repository.InsertModel(activityType);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to insert Activity Type: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public bool UpdateActivityType(ActivityType activityType)
        {
            try
            {
                repository.UpdateRange(new List<ActivityType>() { activityType });
                repository.Save();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update Activity Type: {message} {exception}", ex.Message, ex);
                return false;
            }
        }
    }
}
