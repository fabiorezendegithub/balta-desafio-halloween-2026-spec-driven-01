using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Data;
using PasswordGenerator.Domain;

namespace PasswordGenerator.Application;

public class PasswordService(
    PasswordDbContext db,
    IPasswordGenerator passwordGenerator,
    IGuidGenerator guidGenerator)
{
    public async Task<Guid> CreateAsync(CancellationToken cancellationToken = default)
    {
        var password = passwordGenerator.Generate();
        var guid = guidGenerator.NewGuid();

        var collision = await db.Passwords.AnyAsync(
            p => p.Guid == guid || p.Password == password,
            cancellationToken);

        if (collision)
        {
            throw new InvalidOperationException("Colisão de GUID ou senha na criação.");
        }

        // Uma única linha por SaveChanges: a gravação de GUID e senha é atômica (D-05)
        db.Passwords.Add(new PasswordEntry { Guid = guid, Password = password });
        await db.SaveChangesAsync(cancellationToken);

        return guid;
    }

    public async Task<string> GetPasswordAsync(string? guidHeader, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(guidHeader))
        {
            throw new PasswordValidationException("O header X-Guid é obrigatório e não pode ser vazio.");
        }

        if (!Guid.TryParseExact(guidHeader, "D", out var guid))
        {
            throw new PasswordValidationException("O header X-Guid deve conter um GUID no formato padrão com hífens.");
        }

        var password = await db.Passwords
            .AsNoTracking()
            .Where(p => p.Guid == guid)
            .Select(p => p.Password)
            .FirstOrDefaultAsync(cancellationToken);

        return password ?? throw new GuidNotFoundException();
    }
}
