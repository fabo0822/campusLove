using System;
using System.Data;
using MySql.Data.MySqlClient;
using campusLove.domain.ports;

namespace campusLove.infraestructure.mysql
{
    public class MySqlDbFactory : IDbFactory
    {
        private readonly string _connectionString;

        public MySqlDbFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}