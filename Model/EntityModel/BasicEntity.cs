using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.ModelSql;
using System.Globalization;

namespace Model.EntityModel
{
    public class BasicEntity
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

        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        // Navigation property
        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }
        [ForeignKey("ModifiedBy")]
        public virtual User? ModifiedByUser { get; set; }
    }
}
