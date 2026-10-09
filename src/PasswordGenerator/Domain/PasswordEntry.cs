namespace PasswordGenerator.Domain;

public class PasswordEntry
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string Password { get; set; } = string.Empty;
}
