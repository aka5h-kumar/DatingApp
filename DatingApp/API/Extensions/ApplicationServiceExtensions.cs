

using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection ApplicationServices(this IServiceCollection services,
                IConfiguration config)
                {
                        services.AddDbContext<DataContext>(opt =>
                            {
                                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
                            });
                        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

                        services.AddCors();
                        services.AddScoped<ITokenServices,TokenService>();

                        return services;
                }
    }
}