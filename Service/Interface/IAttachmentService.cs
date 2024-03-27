using Model.ModelSql;

namespace Service.Interface
{
    public interface IAttachmentService
    {
        public bool InsertAttachment(Drawing drawing);
        public List<Drawing> GetDrawingsByProjectId(int projectId);
    }
}
