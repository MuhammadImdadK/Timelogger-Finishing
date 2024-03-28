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
        private const int UserRoleId = 3;
        public int RoleID { get; set; } = UserRoleId;
        public string EmployeeNumber { get; set; }
        public new bool IsActive { get; set; } = true;
        public string Username { get; set; }
        [NotMapped]
        public string NewPassword { get; set; }
        public string Password { get; set; }
        public byte[] Salt { get; set; } = Array.Empty<byte>();
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
        public override string ToString()
        {
            var lastName = this.LastName != null ? $" {this.LastName}" : string.Empty;
            return $"{this.FirstName}{lastName}";
        }
    }
}
