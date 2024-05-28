using Microsoft.Extensions.Logging;
using Model.Interface;
using Model.ModelSql;
using Service.Interface;

namespace Service.Service
{
    public class DeliverableDrawingTypeService(IRepository repository, ILogger<AttachmentService> logger) : IDeliverableDrawingTypeService
    {
        public bool DeleteDeliverableDrawingType(DeliverableDrawingType deliverableType)
        {
            try
            {
                if (deliverableType == null)
                {
                    return false;
                }

                repository.RemoveRange<DeliverableDrawingType>(new List<DeliverableDrawingType> { deliverableType });
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to delete Deliverable Type: {message} {exception}", ex.Message, ex);

                return false;
            }
        }

        public List<DeliverableDrawingType> GetDeliverableDrawingTypes()
        {
            var response = repository.GetQueryableWithOutTracking<DeliverableDrawingType>().OrderByDescending(x => x.Modified).ToList();

            return response;
        }

        public bool InsertDeliverableDrawingType(DeliverableDrawingType deliverableType)
        {
            try
            {
                repository.InsertModel(deliverableType);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to insert Deliverable Type: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public bool UpdateDeliverableDrawingType(DeliverableDrawingType deliverableType)
        {
            try
            {
                repository.UpdateRange(new List<DeliverableDrawingType>() { deliverableType });
                repository.Save();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update Deliverable Type: {message} {exception}", ex.Message, ex);
                return false;
            }
        }
    }
}
