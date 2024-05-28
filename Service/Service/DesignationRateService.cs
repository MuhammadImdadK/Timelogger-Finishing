using Microsoft.Extensions.Logging;
using Model.Interface;
using Model.ModelSql;
using Service.Interface;

namespace Service.Service
{
    public class DesignationRateService(IRepository repository, ILogger<DesignationRateService> logger) : IDesignationRateService
    {
        public bool DeleteDesignationRate(DesignationRates designationRates)
        {
            try
            {
                if (designationRates == null)
                {
                    return true;
                }

                repository.RemoveRange(new List<DesignationRates> { designationRates });
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to delete designation rate: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public List<DesignationRates> GetAllDesignationsRates()
        {
            List<DesignationRates> response = repository.GetQueryableWithOutTracking<DesignationRates>()
                .OrderByDescending(x => x.Modified)
                .ToList();
            return response;
        }

        public List<DesignationRates> GetAllRatesByDesignationId(int id)
        {
            return GetAllDesignationsRates().Where(itm => itm.DesignationID == id).ToList();
        }

        public bool InsertDesignationRate(DesignationRates designationRate)
        {
            try
            {
                repository.InsertModel(designationRate);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to insert designation rate: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public bool UpdateDesignationRate(DesignationRates designationRate)
        {
            try
            {
                repository.UpdateRange(new List<DesignationRates>() { designationRate });
                repository.Save();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update designation rate: {message} {exception}", ex.Message, ex);
                return false;
            }
        }
    }
}
