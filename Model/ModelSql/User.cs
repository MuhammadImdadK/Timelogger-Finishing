using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.EntityModel;
using Common.Enums;

namespace Model.ModelSql
{

    // User entity model
    public class User: BasicEntity, ICloneable
    {
        public int RoleID { get; set; }
        public string EmployeeNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Designation { get; set; }
        public string Email { get; set; }
        public TeamType TeamType { get; set; } = TeamType.None;

        // Navigation property
        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
