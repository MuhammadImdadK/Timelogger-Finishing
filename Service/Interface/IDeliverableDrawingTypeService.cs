using Model.ModelSql;

namespace Service.Interface
{
    public interface IDeliverableDrawingTypeService
    {
        public bool InsertDeliverableDrawingType(DeliverableDrawingType deliveryDrawingType);
        public bool UpdateDeliverableDrawingType(DeliverableDrawingType deliveryDrawingType);
        public bool DeleteDeliverableDrawingType(DeliverableDrawingType deliveryDrawingType);
        public List<DeliverableDrawingType> GetDeliverableDrawingTypes();
    }
}
