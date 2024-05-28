using Microsoft.Extensions.Logging;
using Model.Interface;
using Model.ModelSql;
using Service.Interface;

namespace Service.Service
{
    public class DesignationService(IRepository repository, ILogger<DesignationService> logger) : IDesignationService
    {
        public bool DeleteDesignation(Designation designation)
        {
            try
            {
                if (designation == null)
                {
                    return true;
                }

                repository.RemoveRange(new List<Designation> { designation });
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to delete designation: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public List<Designation> GetAllDesignations()
        {
            List<Designation> response = repository.GetQueryableWithOutTracking<Designation>()
                .OrderByDescending(x => x.Modified)
                .ToList();
            return response;
        }

        public bool InsertDesignation(Designation designation)
        {
            try
            {
                repository.InsertModel(designation);
                return repository.Save() > 0;
            } catch (Exception ex)
            {
                logger.LogError("Failed to insert designation: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public bool UpdateDesignation(Designation designation)
        {
            try
            {
                repository.UpdateRange(new List<Designation>() { designation });
                repository.Save();
                return true;
            } catch (Exception ex)
            {
                logger.LogError("Failed to update designation: {message} {exception}", ex.Message, ex);
                return false;
            }
        }
    }
}
