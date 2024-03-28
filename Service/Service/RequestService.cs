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
    public class RequestService(IRepository repository) : IRequestService
    {
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
            catch
            {
                return false;
            }
        }
    }
}
