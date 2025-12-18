using AI.Invoicer.Domain.Model.Structure;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Invoicer.Infrastructure.Persistance
{
    internal class DatabaseService(string _dbPath)
    {
        private SQLiteAsyncConnection? _connection;
        public async Task<SQLiteAsyncConnection> GetConnection()
        {
            if (_connection == null)
                _connection = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

            await _connection.CreateTableAsync<Invoice>();
            await _connection.CreateTableAsync<InvoiceItem>();
            await _connection.CreateTableAsync<Customer>();

            return _connection;
        }
    }
}
