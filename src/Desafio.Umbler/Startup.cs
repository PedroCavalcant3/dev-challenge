using Desafio.Umbler.Application.Interfaces;
using Desafio.Umbler.Application.Services;
using Desafio.Umbler.Infrastructure.Context;
using Desafio.Umbler.Infrastructure.DataAccess;
using Desafio.Umbler.Infrastructure.ExternalServices;
using DnsClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Desafio.Umbler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            // Replace with your server version and type.
            // Use 'MariaDbServerVersion' for MariaDB.
            // Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
            // For common usages, see pull request #1233.
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 27));

            // Replace 'YourDbContext' with the name of your own DbContext derived class.
            services.AddDbContext<DatabaseContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(connectionString,
                    serverVersion,
                    mysql => mysql.MigrationsAssembly(typeof(DatabaseContext).Assembly.FullName))
                    // The following three options help with debugging, but should
                    // be changed or removed for production.
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            );

            // DNS client (dependência de infra)
            services.AddSingleton<ILookupClient>(_ =>
            {
                var options = new LookupClientOptions(

                /* Tive um timeout de DNS por conta do meu ambiente local (IPv6/DNS), pesquisando, foi-me recomendado forçar DNS público para estabilizar.
                Mas, a melhor solução foi modificar o parâmetro da função DnsClient.QueryType.ANY para DnsClient.QueryType.A na função GetARecordAsync da classe DnsService.cs, Assim, essa mudança 
                resulta em uma consulta DNS mais rápida e específica, reduzindo a probabilidade de timeouts em ambientes com configurações de rede variadas.         
                var result = await _lookup.QueryAsync(domainName, DnsClient.QueryType.A, DnsClient.QueryClass.IN, ct);
                */
                //   new NameServer(IPAddress.Parse("1.1.1.1")), // Cloudflare
                //    new NameServer(IPAddress.Parse("8.8.8.8"))  // Google
                //)
                //{
                //    Timeout = TimeSpan.FromSeconds(5),
                //    UseCache = true
                //};
                );
                return new LookupClient(options);
            });
            //Application Services
            services.AddScoped<IDomainService, DomainService>();
            //Infra External Services
            services.AddScoped<IDnsService, DnsService>();
            services.AddScoped<IWhoisService, WhoisService>();
            //Infra Data Access 
            services.AddScoped<IDomainDataAccess, DomainDataAccess>();
            services.AddControllersWithViews();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
