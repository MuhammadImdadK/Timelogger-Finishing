using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Common.Enums;

namespace Model.ModelSql
{
    public class DesignationRates
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Created { get; set; }

        [NotMapped]
        public string CreatedLocalString => Created != null
            ? ((DateTime)Created).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
            : "Unknown";

        public DateTime? Modified { get; set; }

        [NotMapped]
        public string ModifiedLocalString => Modified != null
            ? ((DateTime)Modified).ToLocalTime().ToString("f", CultureInfo.CurrentCulture)
            : "Unknown";

        public float BaseRate { get; set; }
        public float OvertimeRate { get; set; }
        public float OutsideHourRate { get; set; }
        public int DesignationID { get; set; }
        public TeamType TeamType { get; set; }

        [ForeignKey("DesignationID")]
        public virtual Designation Designation { get; set; }
    }
}
