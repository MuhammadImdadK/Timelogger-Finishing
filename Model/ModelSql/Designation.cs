using Model.EntityModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Model.ModelSql
{
    public class Designation
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


        public override string ToString()
        {
            return Name;
        }
    }
}
