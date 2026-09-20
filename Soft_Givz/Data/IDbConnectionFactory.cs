using System.Data;

namespace Soft_Givz.Data;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}