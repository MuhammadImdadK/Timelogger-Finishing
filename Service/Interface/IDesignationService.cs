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
    public interface IDesignationRateService
    {
        public List<DesignationRates> GetAllDesignationsRates();
        public List<DesignationRates> GetAllRatesByDesignationId(int id);
        public bool InsertDesignationRate(DesignationRates designationRate);
        public bool UpdateDesignationRate(DesignationRates designationRate);
        public bool DeleteDesignationRate(DesignationRates designationRates);
    }
}
