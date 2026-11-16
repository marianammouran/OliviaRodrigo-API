using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Utils;

/// <summary>Identifica erros do SQL Server para devolver o status HTTP adequado.</summary>
public static class ErrosBanco
{
    /// <summary>2601/2627: violação de UNIQUE ou PRIMARY KEY.</summary>
    public static bool Duplicado(DbUpdateException erro) =>
        erro.GetBaseException() is SqlException { Number: 2601 or 2627 };

    /// <summary>547: violação de FOREIGN KEY ou CHECK.</summary>
    public static bool Restricao(DbUpdateException erro) =>
        erro.GetBaseException() is SqlException { Number: 547 };
}
