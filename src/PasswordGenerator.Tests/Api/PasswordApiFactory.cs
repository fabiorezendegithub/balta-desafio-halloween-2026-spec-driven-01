using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Application;

namespace PasswordGenerator.Tests.Api;

public class PasswordApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath;
    private readonly IPasswordGenerator? _passwordGenerator;

    public PasswordApiFactory(string databasePath, IPasswordGenerator? passwordGenerator = null)
    {
        _databasePath = databasePath;
        _passwordGenerator = passwordGenerator;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Passwords", $"Data Source={_databasePath}");

        if (_passwordGenerator is not null)
        {
            builder.ConfigureServices(services =>
                services.AddSingleton(_passwordGenerator));
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            SqliteConnection.ClearAllPools();
        }
    }
}

public sealed class FailingPasswordGenerator : IPasswordGenerator
{
    public string Generate() => throw new InvalidOperationException("indisponibilidade simulada");
}
