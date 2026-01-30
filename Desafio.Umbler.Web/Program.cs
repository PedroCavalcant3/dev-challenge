using Desafio.Umbler.Application.Interfaces;
using Desafio.Umbler.Application.Services;
using Desafio.Umbler.Infrastructure.Context;
using Desafio.Umbler.Infrastructure.DataAccess;
using Desafio.Umbler.Infrastructure.ExternalServices;
using DnsClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Replace with your server version and type.
// Use 'MariaDbServerVersion' for MariaDB.
// Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
// For common usages, see pull request #1233.
var serverVersion = new MySqlServerVersion(new Version(8, 0, 27));

// Replace 'YourDbContext' with the name of your own DbContext derived class.
builder.Services.AddDbContext<DatabaseContext>(
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
builder.Services.AddSingleton<ILookupClient>(_ =>
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
builder.Services.AddScoped<IDomainService, DomainService>();
//Infra External Services
builder.Services.AddScoped<IDnsService, DnsService>();
builder.Services.AddScoped<IWhoisService, WhoisService>();
//Infra Data Access 
builder.Services.AddScoped<IDomainDataAccess, DomainDataAccess>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
