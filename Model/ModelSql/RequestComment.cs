using System.ComponentModel.DataAnnotations.Schema;
using Model.EntityModel;

namespace Model.ModelSql
{
    public class RequestComment : BasicEntity
    {
        public int UserID { get; set; }
        public int RequestID { get; set; }
        public string Comment { get; set; } = string.Empty;

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("RequestID")]
        public virtual Request Request { get; set; }
    }
}
