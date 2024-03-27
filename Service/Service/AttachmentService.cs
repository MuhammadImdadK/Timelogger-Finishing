using Model.Interface;
using Model.ModelSql;
using Service.Interface;

namespace Service.Service
{
    public class AttachmentService(IRepository repository) : IAttachmentService
    {
        public List<Drawing> GetDrawingsByProjectId(int projectId)
        {
            List<Drawing> response = repository.GetQueryableWithOutTracking<Drawing>()
                .OrderByDescending(itm => itm.Modified)
                .Where(itm => itm.ProjectId == projectId)
                .ToList();
            return response;
        }

        public bool InsertAttachment(Drawing drawing)
        {
            try
            {
                repository.InsertModel(drawing);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
