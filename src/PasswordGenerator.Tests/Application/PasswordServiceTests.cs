using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Application;
using PasswordGenerator.Data;
using PasswordGenerator.Domain;

namespace PasswordGenerator.Tests.Application;

public class PasswordServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PasswordDbContext _db;
    private readonly FakePasswordGenerator _passwordGenerator = new();
    private readonly FakeGuidGenerator _guidGenerator = new();
    private readonly PasswordService _service;

    public PasswordServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PasswordDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new PasswordDbContext(options);
        _db.Database.EnsureCreated();

        _service = new PasswordService(_db, _passwordGenerator, _guidGenerator);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Create_ReturnsGuidLinkedToStoredPassword()
    {
        _passwordGenerator.Next = "Senha#Forte1234567";
        _guidGenerator.Next = Guid.NewGuid();

        var guid = await _service.CreateAsync();

        var entry = await _db.Passwords.SingleAsync();
        Assert.Equal(guid, entry.Guid);
        Assert.Equal("Senha#Forte1234567", entry.Password);
    }

    [Fact]
    public async Task Create_EachCreationGeneratesNewGuidAndPassword()
    {
        var real = new PasswordService(_db, new StrongPasswordGenerator(), new SystemGuidGenerator());

        var guids = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            guids.Add(await real.CreateAsync());
        }

        var entries = await _db.Passwords.ToListAsync();
        Assert.Equal(5, entries.Count);
        Assert.Equal(5, guids.Distinct().Count());
        Assert.Equal(5, entries.Select(e => e.Password).Distinct().Count());
    }

    [Fact]
    public void Create_SignatureReturnsOnlyGuid()
    {
        var returnType = typeof(PasswordService).GetMethod(nameof(PasswordService.CreateAsync))!.ReturnType;

        Assert.Equal(typeof(Task<Guid>), returnType);
    }

    [Fact]
    public async Task Create_WithGuidCollision_ThrowsAndCreatesNothing()
    {
        var existing = Guid.NewGuid();
        _db.Passwords.Add(new PasswordEntry { Guid = existing, Password = "Existente#1aaaaaaaa" });
        await _db.SaveChangesAsync();

        _passwordGenerator.Next = "Nova#Senha1234567a";
        _guidGenerator.Next = existing;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync());

        Assert.Equal(1, await _db.Passwords.CountAsync());
    }

    [Fact]
    public async Task Create_WithPasswordCollision_ThrowsAndCreatesNothing()
    {
        _db.Passwords.Add(new PasswordEntry { Guid = Guid.NewGuid(), Password = "Existente#1aaaaaaaa" });
        await _db.SaveChangesAsync();

        _passwordGenerator.Next = "Existente#1aaaaaaaa";
        _guidGenerator.Next = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync());

        Assert.Equal(1, await _db.Passwords.CountAsync());
    }

    [Fact]
    public async Task Create_WhenPersistenceFails_ThrowsAndCreatesNothing()
    {
        // Simula indisponibilidade do banco na gravação: o INSERT é recusado pelo SQLite
        _db.Database.ExecuteSqlRaw(
            "CREATE TRIGGER fail_insert BEFORE INSERT ON Passwords BEGIN SELECT RAISE(ABORT, 'falha simulada'); END;");

        _passwordGenerator.Next = "Nova#Senha1234567a";
        _guidGenerator.Next = Guid.NewGuid();

        await Assert.ThrowsAnyAsync<Exception>(() => _service.CreateAsync());

        Assert.Equal(0, await _db.Passwords.CountAsync());
    }

    [Fact]
    public async Task Get_ExistingGuid_ReturnsSamePasswordOnRepeatedQueries()
    {
        var guid = Guid.NewGuid();
        _db.Passwords.Add(new PasswordEntry { Guid = guid, Password = "Senha#Forte1234567" });
        await _db.SaveChangesAsync();

        var first = await _service.GetPasswordAsync(guid.ToString("D"));
        var second = await _service.GetPasswordAsync(guid.ToString("D"));

        Assert.Equal("Senha#Forte1234567", first);
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task Get_WellFormedUnknownGuid_ThrowsGuidNotFound()
    {
        await Assert.ThrowsAsync<GuidNotFoundException>(
            () => _service.GetPasswordAsync(Guid.NewGuid().ToString("D")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nao-e-um-guid")]
    [InlineData("3f2504e04f8911d39a0c0305e82c3301")]
    [InlineData("{3f2504e0-4f89-11d3-9a0c-0305e82c3301}")]
    [InlineData("(3f2504e0-4f89-11d3-9a0c-0305e82c3301)")]
    public async Task Get_MissingEmptyOrMalformedHeader_ThrowsValidationException(string? header)
    {
        await Assert.ThrowsAsync<PasswordValidationException>(() => _service.GetPasswordAsync(header));
    }

    private sealed class FakePasswordGenerator : IPasswordGenerator
    {
        public string Next { get; set; } = "Senha#Padrao12345678";

        public string Generate() => Next;
    }

    private sealed class FakeGuidGenerator : IGuidGenerator
    {
        public Guid Next { get; set; } = Guid.NewGuid();

        public Guid NewGuid() => Next;
    }
}
