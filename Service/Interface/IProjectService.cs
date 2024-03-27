using Model.ModelSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IProjectService
    {
        public List<Project> GetProjects();
        public Project GetProjectById(int id);
        public int? GetLatestProjectId();
        public int GetProjectCount();
        public bool InsertProject(Project project);
        public bool UpdateProject(Project project);
    }
}
