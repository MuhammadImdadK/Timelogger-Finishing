﻿using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interface
{
    public interface IRepository : IRepositoryReadOnly
    {
        bool RunMigrations(IConfigurationRoot config, ILogger<DbContext> logger);
        void InsertModel<T>(T model) where T : class;
        void InsertModels<T>(IEnumerable<T> model) where T : class;

        void ResetChangeTracker();
        void DeleteModel<T>(int modelId) where T : class;
        void DeleteModel<T>(string modelId) where T : class;
        int Save();
        Task<int> SaveAsync();
        void ExecuteRawSql(string sql);
        Task<IEnumerable<T>> ExecuteRawSqlAsync<T>(string sql) where T : class;
        void AddRange<T>(IEnumerable<T> objects, bool IsAuditable = true) where T : class;
        void RemoveRange<T>(IEnumerable<T> objects, bool IsAuditable = true) where T : class;
        void UpdateRange<T>(IEnumerable<T> objects, bool IsAuditable = true) where T : class;
    }
}
