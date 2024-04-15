using Model.ModelSql;

namespace Service.Interface
{
    public interface IDesignationService
    {
        public List<Designation> GetAllDesignations();
        public bool InsertDesignation(Designation designation);
        public bool UpdateDesignation(Designation designation);
        public bool DeleteDesignation(Designation designation);
    }
}
