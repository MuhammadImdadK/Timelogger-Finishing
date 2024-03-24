using Model.EntityModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ModelSql
{
    // Project entity model
    public class Project : BasicEntity
    {
        public string? ProjectName { get; set; }
        public string? ERFNumber { get; set; }
        public string? Description { get; set; }
        public ICollection<Drawing> Drawings { get; set; }
    }
}
