using System.Diagnostics.CodeAnalysis;

namespace FiapSrvAuthManager.Infrastructure.Mappings;

[ExcludeFromCodeCoverage]
public static class MongoMappings
{
    public static void ConfigureMappings() 
    {
        UserMapping.Configure();
    }
}