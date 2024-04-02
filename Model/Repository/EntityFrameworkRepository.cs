using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.Database;
using Model.EntityModel;
using Model.Interface;
using Model.ModelSql;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Repository
{
    public class EntityFrameworkRepository : EntityFrameworkRepositoryReadOnly, IRepository
    {
        private readonly TimeLoggerContext _DbContext;
        private readonly ILogger<EntityFrameworkRepository> logger;

        public EntityFrameworkRepository(
         TimeLoggerContext context,
         ILogger<EntityFrameworkRepository> logger
         )
         : base(context)
        {
            _DbContext = context;
            this.logger = logger;
        }

        public void DeleteModel<T>(int modelId) where T : class
        {
            throw new NotImplementedException();
        }

        public void DeleteModel<T>(string modelId) where T : class
        {
            throw new NotImplementedException();
        }

        public void ExecuteRawSql(string sql)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> ExecuteRawSqlAsync<T>(string sql) where T : class
        {
            throw new NotImplementedException();
        }

        public void InsertModel<T>(T model) where T : class
        {
            try
            {
                _DbContext.Set<T>().Add(model);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to insert {type}: {message} {exception}", typeof(T), ex.Message, ex);
                throw;
            }
        }


        public void ResetChangeTracker()
        {
            _DbContext.ChangeTracker.Clear();
        }



        public void AddRange<T>(IEnumerable<T> objects, bool IsAuditable = true) where T : class
        {
            try
            {
                if (objects.Count() > 0)
                {
                    _DbContext.AddRange(objects);
                    int count = Save();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to insert multiple {type}: {message} {exception}", typeof(T), ex.Message, ex);

                throw;
            }
        }
        public void RemoveRange<T>(IEnumerable<T> objects, bool IsAuditable = true) where T : class
        {
            try
            {
                _DbContext.RemoveRange(objects);
                Save();
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to remove {type}: {message} {exception} {collection}", typeof(T), ex.Message, ex, objects);
            }
        }
        public void UpdateRange<T>(IEnumerable<T> objects, bool IsAuditable = true) where T : class
        {
            try
            {
                _DbContext.UpdateRange(objects);
                Save();
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to update {type}: {message} {exception} {coll}", typeof(T), ex.Message, ex, objects);
            }
        }

        public int Save()
        {
            try
            {
                var save = _DbContext.SaveChanges();
                _DbContext.ChangeTracker.AcceptAllChanges();
                _DbContext.ChangeTracker.Clear();
                return save;
            }
            catch (DbUpdateException ex)
            {
                this.logger.LogError("Failed to save: {message} {exception}", ex.Message, ex);
                return 0;
            }
        }

        public async Task<int> SaveAsync()
        {
            try
            {
                return await _DbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                this.logger.LogError("Failed to save: {message} {exception}", ex.Message, ex);

                throw;
            }
        }

        public bool RunMigrations(IConfigurationRoot configuration, ILogger<DbContext> logger)
        {
            try
            {
                new TimeLoggerContext(new(), configuration, logger).Database.Migrate();

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to run migrations: {message} {exception}", ex.Message, ex);

                return false;
            }
        }

        private void SaveChangesWithAudit(object entity, ActionType actionType, EventType eventType)
        {
            // Save changes to the database
            _DbContext.SaveChanges();

            // Create audit entry
            var auditEntry = CreateAuditEntry(entity, actionType,  eventType);
            _DbContext.Set<Audit>().Add(auditEntry);
            _DbContext.SaveChanges();
        }

        private Audit CreateAuditEntry(object entity, ActionType actionType, EventType eventType)
        {
            // Create audit entry based on the entity and action type
            // You need to implement this method according to your audit model
            var auditEntry = new Audit
            {
                UserID = Convert.ToInt32(entity.GetType().GetProperty("ModifiedBy")),
                Name = Convert.ToString(entity.GetType().GetProperty("Name")),
                ActionType = actionType,
                EventType = eventType,
                RecordID = Convert.ToInt32(entity.GetType().GetProperty("ID")),
                ActionDateTime = DateTime.Now,
                OldValuesJson = actionType != ActionType.Insert ? JsonConvert.SerializeObject(_DbContext.Entry(entity).OriginalValues) : null,
                NewValuesJson = actionType != ActionType.Delete ? JsonConvert.SerializeObject(entity) : null
            };
            return auditEntry;

            // Implement this method according to your audit model
            throw new NotImplementedException();
        }
    }
}
