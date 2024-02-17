using LM.API.Services;
namespace LM.API.Commons
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IDocumentService, DocumentService>();
            return services;
        }
    }
}
