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
    public class ProjectService(IRepository repository) : IProjectService
    {
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
            List<Project> response = repository.GetQueryableWithOutTracking<Project>()
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
            catch
            {
                return false;
            }
        }
    }
}
