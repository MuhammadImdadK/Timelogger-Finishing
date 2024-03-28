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

    public class Request : BasicEntity
    {
        public int UserID { get; set; }
        public int? PlanningEngineerID { get; set; }
        public int? ProjectID { get; set; }
        public int? TimeLogID { get; set; }

        public RequestType RequestType { get; set; } = RequestType.Project;

        [NotMapped]
        public DateTimeOffset? StartTimeOffset { get; set; } = DateTimeOffset.Now;
        public DateTime StartTime { get; set; }
        [NotMapped]
        public string StartTimeLocalString => StartTime != null
            ? ((DateTime)StartTime).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
            : "Unknown";
        [NotMapped]
        public DateTimeOffset? EndTimeOffset { get; set; } = DateTimeOffset.Now;
        public DateTime? EndTime { get; set; }
        [NotMapped]
        public string EndTimeLocalString => EndTime != null
            ? ((DateTime)EndTime).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
            : "Unknown";
        public RequestStatus RequestStatus { get; set; }
        public TimeSpan? Timestamp { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("PlanningEngineerID")]
        public virtual User? PlanningEngineer { get; set; }

        [ForeignKey("ProjectID")]
        public virtual Project? Project { get; set; }
        [ForeignKey("TimeLogID")]
        public virtual TimeLog? TimeLog { get; set; }

        [NotMapped]
        public List<RequestComment> RequestComments { get; set; }
        
        [NotMapped]
        public string PendingComment { get; set; }


        public override string ToString()
        {
            if(User == null && TimeLog?.User == null && Project?.CreatedByUser == null)
            {
                return "Unknown request";
            }
            var timelog = TimeLog?.ToString().Replace("Not set", string.Empty);
            var project = Project.ToString();
            var user = User?.ToString();

            return $"{user} {project} {timelog}".Trim();
        }
    }
}
