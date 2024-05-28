using Model.EntityModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Model.ModelSql
{
    public class Designation : ICloneable
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

        public string Name { get; set; }

        [NotMapped]
        public virtual List<DesignationRates> DesignationRatesTemplate { get; set; } = new() 
        {
            new() { TeamType = Common.Enums.TeamType.CoreTeam, IsActive = true, OutsideHourRate = 0, OvertimeRate = 0, BaseRate = 0 },
            new() { TeamType = Common.Enums.TeamType.AdditionalTeam, IsActive = true, OutsideHourRate = 0, OvertimeRate = 0, BaseRate = 0 },
        };

        public object Clone()
        {
            var obj = this.MemberwiseClone() as Designation;
            var cloneTemplate = new List<DesignationRates>();
            foreach(var kvp in DesignationRatesTemplate)
            {
                if (kvp == null) continue;
                cloneTemplate.Add(kvp!.Clone() as DesignationRates);
            }
            obj.DesignationRatesTemplate = cloneTemplate;
            return obj;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
