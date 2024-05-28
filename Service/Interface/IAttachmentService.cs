using Model.ModelSql;

namespace Service.Interface
{
    public interface IAttachmentService
    {
        public bool InsertAttachment(Drawing drawing);
        public List<Drawing> GetDrawingsByProjectId(int projectId);
        public List<Drawing> GetDrawings();
        public bool UpdateAttachment(Drawing currentAttachment);
        public bool DeleteAttachment(Drawing drawing);
        public bool InsertManyAttachment(List<Drawing> toInsert);
    }
}
