﻿using Model.Interface;
using Model.ModelSql;
using Service.Interface;

namespace Service.Service
{
    public class RequestCommentService : IRequestCommentService
    {
        IRepository repository;
        public RequestCommentService(IRepository _repository)
        {
                repository=_repository;
        }
        public List<RequestComment> GetRequestCommentsByRequestId(int projectId)
        {
            List<RequestComment> comments = repository.GetQueryableWithOutTracking<RequestComment>()
                .OrderByDescending(request => request.Modified)
                .Where(itm => itm.RequestID == projectId)
                .ToList();
            return comments;
        }

        public bool InsertRequestComment(RequestComment requestComment)
        {
            try
            {
                repository.InsertModel(requestComment);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
