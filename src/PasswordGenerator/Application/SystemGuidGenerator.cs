namespace PasswordGenerator.Application;

public sealed class SystemGuidGenerator : IGuidGenerator
{
    public Guid NewGuid() => Guid.NewGuid();
}
