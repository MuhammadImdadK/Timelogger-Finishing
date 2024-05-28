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
        [NotMapped]
        public TimeLog? timeLog;
        [NotMapped]
        private Drawing? deliverable;
        [NotMapped]
        private User? planningEngineer;
        [NotMapped]
        private User user;

        public int UserID { get; set; }
        public int? PlanningEngineerID { get; set; }
        public int? ProjectID { get; set; }
        public int? TimeLogID { get; set; }
        public int? DeliverableID { get; set; }
        public RequestType RequestType { get; set; } = RequestType.Project;

        [NotMapped]
        public DateTimeOffset? StartTimeOffset { get; set; } = DateTimeOffset.Now;

        public bool IsNewTimeLog { get; set; }

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
        public virtual User User { get => user; set => user = value; }

        [ForeignKey("PlanningEngineerID")]
        public virtual User? PlanningEngineer { get => planningEngineer; set => planningEngineer = value; }

        [ForeignKey("ProjectID")]
        public virtual Project? Project { get; set; }
        [ForeignKey("TimeLogID")]
        public virtual TimeLog? TimeLog { get => timeLog; set => timeLog = value; }

        [ForeignKey("DeliverableID")]
        public virtual Drawing? Deliverable { get => deliverable; set => deliverable = value; }

        [NotMapped]
        public List<RequestComment> RequestComments { get; set; }
        
        [NotMapped]
        public string PendingComment { get; set; }

        [NotMapped]
        public bool IsUpdateRequested => RequestStatus == RequestStatus.UpdateRequested;

        public override string ToString()
        {
            if(User == null && TimeLog?.User == null && Project?.CreatedByUser == null)
            {
                return "Unknown request";
            }
            var timelog = TimeLog?.ToString().Replace("Not set", string.Empty);
            var project = Project?.ToString() ?? "";
            var user = User?.ToString() ?? "";

            return $"{user} {project} {timelog}".Trim();
        }
    }
}
