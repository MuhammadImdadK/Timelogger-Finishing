using Common.Enums;
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
    public class Project : BasicEntity, ICloneable
    {
        public string? ProjectName { get; set; }
        public string? ERFNumber { get; set; }
        public string? Description { get; set; }
        public float ManhourBudget { get; set; }
        public RequestStatus? ApprovalState { get; set; }
        public ICollection<Drawing> Drawings { get; set; }

        public object Clone()
        {
            return base.MemberwiseClone();
        }

        public override string ToString()
        {
            return $"ERF-{ERFNumber} - {ProjectName}";
        }
    }
}
