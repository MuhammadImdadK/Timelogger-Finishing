using Model.Interface;
using Model.ModelSql;
using Service.Interface;

namespace Service.Service
{
    public class DesignationService(IRepository repository) : IDesignationService
    {
        public bool DeleteDesignation(Designation designation)
        {
            throw new NotImplementedException();
        }

        public List<Designation> GetAllDesignations()
        {
            throw new NotImplementedException();
        }

        public bool InsertDesignation(Designation designation)
        {
            throw new NotImplementedException();
        }

        public bool UpdateDesignation(Designation designation)
        {
            throw new NotImplementedException();
        }
    }
}
