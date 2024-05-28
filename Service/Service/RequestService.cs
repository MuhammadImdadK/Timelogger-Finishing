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
    public class RequestService: IRequestService
    {
        IRepository repository;
        ILogger<RequestService> logger;
        public RequestService(IRepository _repository, ILogger<RequestService> _logger)
        {
            repository = _repository;
            logger = _logger;
        }
        public Request? GetRequestById(int id)
        {
            return GetRequests().FirstOrDefault(r => r.Id == id);
        }

        public List<Request> GetRequests()
        {
            List<Request> requests = repository.GetQueryableWithOutTracking<Request>()
                .OrderByDescending(itm => itm.Modified)
                .ToList();
            return requests;
        }

        public bool InsertRequest(Request request)
        {
            try
            {
                repository.InsertModel(request);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to insert request: {message} {exception}", ex.Message, ex);

                return false;
            }
        }

        public bool UpdateRequest(Request currentRequest)
        {
            try
            {
                repository.UpdateRange(new List<Request> { currentRequest });
                repository.Save();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update request: {message} {exception}", ex.Message, ex);

                return false;
            }
        }
    }
}
