namespace VetSync.Domain.Entities;

/// <summary>Perfis de acesso suportados pela API.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Usuario = "Usuario";

    public static readonly string[] Todos = { Admin, Usuario };
}
