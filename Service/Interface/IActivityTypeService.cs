using Model.ModelSql;

namespace Service.Interface
{
    public interface IActivityTypeService
    {
        public bool InsertActivityType(ActivityType activityType);
        public List <ActivityType> GetActivityTypes();
        public bool UpdateActivityType(ActivityType activityType);
        public bool DeleteActivityType(ActivityType activityType);
    }
}
