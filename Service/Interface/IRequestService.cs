using Model.ModelSql;
using Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IRequestService
    {
        public List<Request> GetRequests();
        public Request? GetRequestById(int id);
        public bool InsertRequest(Request request);
    }
}
