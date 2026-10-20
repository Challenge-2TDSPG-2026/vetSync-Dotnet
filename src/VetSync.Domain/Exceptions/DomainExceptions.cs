namespace VetSync.Domain.Exceptions;

/// <summary>Exceção base de todas as falhas de regra de negócio da aplicação.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

/// <summary>Uma regra de negócio/validação foi violada (HTTP 400).</summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>O recurso solicitado não existe (HTTP 404).</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }

    public static NotFoundException For(string recurso, object id)
        => new($"{recurso} com ID {id} não encontrado(a).");
}

/// <summary>A operação conflita com o estado atual de um recurso, ex.: e-mail duplicado (HTTP 409).</summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Credenciais inválidas ou usuário sem permissão de autenticar (HTTP 401).</summary>
public class AuthenticationFailedException : DomainException
{
    public AuthenticationFailedException(string message) : base(message) { }
}
