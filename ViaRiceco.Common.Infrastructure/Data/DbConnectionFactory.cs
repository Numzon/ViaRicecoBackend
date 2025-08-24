using System.Data.Common;
using Npgsql;
using ViaRiceco.Common.Application.Data;

namespace ViaRiceco.Common.Infrastructure.Data;

public class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await dataSource.OpenConnectionAsync();
    }
}
