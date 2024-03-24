using Model.EntityModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ModelSql
{
    public class Drawing:BasicEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProjectId{ get; set; }

        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }
    }
}
