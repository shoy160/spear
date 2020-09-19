using Microsoft.Extensions.Logging;
using Spear.Core.Data;
using Spear.Core.Dependency;
using Spear.Core.Domain;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;

namespace Spear.Dapper
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnectionProvider _connProvider;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<int, Lazy<IDbConnection>> _connections;

        private bool _closeabel;

        /// <inheritdoc />
        public Guid Id { get; }
        private readonly string _configName;

        private readonly string _connectionString;
        private readonly string _providerName;

        //private static readonly object SyncObj = new object();

        private UnitOfWork()
        {
            Id = Guid.NewGuid();
            _connProvider = CurrentIocManager.Resolve<IDbConnectionProvider>();
            _connections = new ConcurrentDictionary<int, Lazy<IDbConnection>>();
            _logger = CurrentIocManager.CreateLogger(GetType());
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"{GetType().Name}:{Id} Create");
        }

        public UnitOfWork(string configName = null) : this()
        {
            _configName = configName;
        }

        public UnitOfWork(string connectionString, string providerName) : this()
        {
            _connectionString = connectionString;
            _providerName = providerName;
        }

        /// <summary> 当前数据库连接 </summary>
        public IDbConnection Connection
        {
            get
            {
                //return CreateConnection();
                var key = Thread.CurrentThread.ManagedThreadId;
                var lazy = _connections.GetOrAdd(key, k => new Lazy<IDbConnection>(CreateConnection));
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"{GetType().Name}[{Id}],Current Thread:{key},{_connections.Count}");
                return lazy.Value;
            }
        }


        /// <summary> 当前事务 </summary>
        public IDbTransaction Transaction { get; protected set; }

        /// <summary> 开启事务的连接或者当前连接 </summary>
        public IDbConnection TransConnection => IsTransaction ? Transaction.Connection : Connection;

        /// <summary> 是否开启事务 </summary>
        public bool IsTransaction => Transaction != null;

        /// <summary> 开始事务 </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool Begin(IsolationLevel? level = null)
        {
            if (IsTransaction)
                return false;
            var conn = CreateConnection();
            _closeabel = conn.State == ConnectionState.Closed;
            if (_closeabel)
                conn.Open();
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"{GetType().Name}[{Id}] Begin Transaction");
            Transaction = level.HasValue
                ? conn.BeginTransaction(level.Value)
                : conn.BeginTransaction();
            return true;
        }

        /// <summary> 创建连接 </summary>
        /// <returns></returns>
        public IDbConnection CreateConnection()
        {
            return string.IsNullOrWhiteSpace(_connectionString)
                ? _connProvider.Connection(_configName)
                : _connProvider.Connection(_connectionString, _providerName);
        }

        /// <summary> 提交事务 </summary>
        public virtual void Commit()
        {
            Transaction?.Commit();
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"{GetType().Name}[{Id}] Commit Transaction");
            TransDispose();
        }

        /// <summary> 回滚事务 </summary>
        public virtual void Rollback()
        {
            Transaction?.Rollback();
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"{GetType().Name}[{Id}] Rollback Transaction");

            TransDispose();
        }

        /// <summary> 释放事务 </summary>
        protected virtual void TransDispose()
        {
            if (Transaction == null) return;
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"{GetType().Name}[{Id}] Dispose Transaction");
            var conn = Transaction.Connection;
            Transaction.Dispose();
            if (conn != null && conn.State == ConnectionState.Open)
                conn.Close();
            Transaction = null;
        }

        /// <summary> 资源释放 </summary>
        public virtual void Dispose()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug(
                $"Dispose {GetType().Name}[{Id}],[{string.Join(",", _connections.Keys)}],{_connections.Count}");
            TransDispose();
            if (_connections.Count > 0)
            {
                foreach (var conn in _connections.Values)
                {
                    conn.Value.Close();
                }
            }

            _connections.Clear();
            if (_closeabel && Connection.State != ConnectionState.Closed)
                Connection.Close();
        }
    }
}
