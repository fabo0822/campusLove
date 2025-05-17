using System;
using System.Data;

namespace campusLove.domain.ports
{
    public interface IDbFactory
    {
        IDbConnection CreateConnection();
    }
} 