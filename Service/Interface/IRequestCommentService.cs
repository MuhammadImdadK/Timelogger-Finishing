using Model.ModelSql;

namespace Service.Interface
{
    public interface IRequestCommentService
    {
        public List<RequestComment> GetRequestCommentsByRequestId(int projectId);
        public bool InsertRequestComment(RequestComment requestComment);
    }
}
