using Microsoft.Extensions.Logging;
using Model.Interface;
using Model.ModelSql;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class ProjectService : IProjectService
    {
        IRepository repository;
        ILogger<ProjectService> logger;
        public ProjectService(IRepository _repository, ILogger<ProjectService> _logger)
        {
            repository = _repository;
            logger = _logger;
        }
        public int? GetLatestProjectId()
        {
            return GetProjects().Max(itm => itm.Id);
        }

        public Project GetProjectById(int id)
        {
            return GetProjects().FirstOrDefault(p => p.Id == id);
        }

        public int GetProjectCount()
        {
            return GetProjects().Count();
        }

        public List<Project> GetProjects()
        {
            List<Project> response = repository.GetQueryableWithOutTracking<Project>().ToList()
                .OrderByDescending(x => x.Modified)
                .ToList();
            return response;
        }

        public bool InsertProject(Project project)
        {
            try
            {
                repository.InsertModel(project);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to insert project: {message} {exception}", ex.Message, ex);

                return false;
            }
        }

        public bool UpdateProject(Project project)
        {
            try
            {
                repository.UpdateRange(new List<Project>() { project });
                repository.Save();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update project: {message} {exception}", ex.Message, ex);

                return false;
            }
        }
    }
}
