using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Model.EntityModel;
using System.Globalization;

namespace Model.ModelSql
{
    // TimeLog entity model
    public class TimeLog: BasicEntity
    {
        public int UserID { get; set; }
        public int ProjectID { get; set; }
        public DateTime StartDateTime { get; set; }
        [NotMapped]
        public string StartDateTimeLocalString => StartDateTime != null
            ? ((DateTime)StartDateTime).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
            : "Unknown";
        public DateTime? EndDateTime { get; set; }
        [NotMapped]
        public string EndDateTimeLocalString => EndDateTime != null
        ? ((DateTime)EndDateTime).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
        : "Unknown";
        public TimeSpan Duration { get; set; }
        public TimeLogStatus? TimeLogStatus { get; set; }
        public string? Comment { get; set; }

        //Other Fields
        public DisciplineType  DisciplineType{ get; set; }
        public DrawingType DrawingType { get; set; }
        public ScopeType ScopeType { get; set; }
        public TeamType TeamType { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        public override string ToString()
        {
            return TimeLogStatus == null ? "Not set" : $"ERF-{ProjectID + 10001} - {StartDateTimeLocalString}";
        }
    }
}
