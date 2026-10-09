using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Data;
using PasswordGenerator.Domain;

namespace PasswordGenerator.Tests.Data;

public class PasswordDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PasswordDbContext _db;

    public PasswordDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PasswordDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new PasswordDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Insert_DuplicateGuid_Fails()
    {
        var guid = Guid.NewGuid();
        _db.Passwords.Add(new PasswordEntry { Guid = guid, Password = "Primeira#Senha1234" });
        await _db.SaveChangesAsync();

        _db.Passwords.Add(new PasswordEntry { Guid = guid, Password = "Segunda#Senha1234" });

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
    }

    [Fact]
    public async Task Insert_DuplicatePassword_Fails()
    {
        _db.Passwords.Add(new PasswordEntry { Guid = Guid.NewGuid(), Password = "Mesma#Senha12345" });
        await _db.SaveChangesAsync();

        _db.Passwords.Add(new PasswordEntry { Guid = Guid.NewGuid(), Password = "Mesma#Senha12345" });

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
    }

    [Fact]
    public async Task Insert_NullPassword_Fails()
    {
        _db.Passwords.Add(new PasswordEntry { Guid = Guid.NewGuid(), Password = null! });

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
    }
}
