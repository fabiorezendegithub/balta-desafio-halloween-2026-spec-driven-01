namespace PasswordGenerator.Application;

// Erro de validação do valor do header X-Guid (ausente, vazio ou malformado): HTTP 400
public class PasswordValidationException(string message) : Exception(message);

// GUID bem formado que não existe no sistema: HTTP 404
public class GuidNotFoundException()
    : Exception("O GUID informado não existe.");
