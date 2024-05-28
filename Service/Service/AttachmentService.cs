using Microsoft.Extensions.Logging;
using Model.Interface;
using Model.ModelSql;
using Service.Interface;

namespace Service.Service
{
    public class AttachmentService(IRepository repository, ILogger<AttachmentService> logger) : IAttachmentService
    {
        public List<Drawing> GetDrawings()
        {
            List<Drawing> response = repository.GetQueryableWithOutTracking<Drawing>()
                .OrderByDescending(itm => itm.Modified)
                .ToList();
            return response;
        }

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
                logger.LogError("Failed to insert deliverable: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public bool UpdateAttachment(Drawing currentAttachment)
        {
            try
            {
                repository.UpdateRange(new List<Drawing>() { currentAttachment });
                repository.Save();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update deliverable: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public bool DeleteAttachment(Drawing drawing)
        {
            try
            {
                if (drawing == null)
                {
                    return false;
                }

                repository.RemoveRange<Drawing>(new List<Drawing> { drawing });
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to delete deliverable: {message} {exception}", ex.Message, ex);

                return false;
            }
        }

        public bool InsertManyAttachment(List<Drawing> toInsert)
        {
            try
            {
                repository.InsertModels(toInsert);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to insert deliverable: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

    }
}
