using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.ModelSql;
using NLog.Extensions.Logging;
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
        private readonly ILogger<TimeLoggerContext> logger;

        public TimeLoggerContext(DbContextOptions<TimeLoggerContext> options, IConfigurationRoot configuration, ILogger<DbContext>? logger = null) : base(options)
        {
            this.configuration = configuration;
            this.logger = LoggerFactory.Create(x => x.AddNLog()).CreateLogger<TimeLoggerContext>();
            //key = _configuration["DatabaseEncryption:Key"];
        }
        public TimeLoggerContext() : base()
        {
            this.logger = LoggerFactory.Create(x => x.AddNLog()).CreateLogger<TimeLoggerContext>();
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
        public DbSet<ActivityType> ActivityTypes { get; set; }
        public DbSet<DeliverableDrawingType> DeliverableDrawingTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString;//= "server=localhost;port=5432;User ID=postgres;password=root;database=TimeLogger8;";
            // Configure PostgreSQL connection string here
            if (ConfigurationManager.ConnectionStrings.Count > 0 && ConfigurationManager.ConnectionStrings["TimeLoggerDatabase"] != null)
            {
                connectionString = ConfigurationManager.ConnectionStrings["TimeLoggerDatabase"].ConnectionString;
                optionsBuilder.UseNpgsql();
            }
            else
            {

                connectionString = this.configuration.GetConnectionString("TimeLoggerDatabase");
                connectionString = "server=localhost;port=5432;User ID=postgres;password=root;database=TimeLogger8;";
                optionsBuilder.UseNpgsql(connectionString);
            }
            if(this.logger != null)
            {
                var split = connectionString.Split("password=").FirstOrDefault();
                this.logger.LogInformation("Configured database is {database}", split);
            }
        }
    }
}
