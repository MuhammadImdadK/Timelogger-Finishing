using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Model.ModelSql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Database
{
    public class TimeLoggerContext : DbContext
    {
        private readonly IConfigurationRoot configuration;

        public TimeLoggerContext(DbContextOptions<TimeLoggerContext> options, IConfigurationRoot configuration) : base(options)
        {
            this.configuration = configuration;
            //key = _configuration["DatabaseEncryption:Key"];
        }
        public TimeLoggerContext() : base()
        {
        }

        public DbSet<Audit> Audits { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestComment> RequestComments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<DesignationRates> DesignationRates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure PostgreSQL connection string here
            if (ConfigurationManager.ConnectionStrings.Count > 0 && ConfigurationManager.ConnectionStrings["TimeLoggerDatabase"] != null)
            {
                optionsBuilder.UseNpgsql(ConfigurationManager.ConnectionStrings["TimeLoggerDatabase"].ConnectionString);
            }
            else
            {
                var connectionString = this.configuration.GetConnectionString("TimeLoggerDatabase");
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }
}
