using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace DiarioRoxo.WebAPI.Utils;

/// <summary>
/// Cria o banco DiarioRoxoDB executando Database/DiarioRoxoDB.sql quando ele ainda não existe.
/// Facilita rodar o projeto no VS Code sem precisar abrir o SSMS.
/// Desligue com "Banco:CriarAutomaticamente": false no appsettings.json.
/// </summary>
public static class InicializadorBanco
{
    private const string NomeBancoScript = "DiarioRoxoDB";

    public static async Task CriarSeNecessario(IConfiguration configuracao, ILogger logger)
    {
        if (!configuracao.GetValue("Banco:CriarAutomaticamente", false)) return;

        var conexaoTexto = configuracao.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Configure a ConnectionString DefaultConnection.");

        var construtor = new SqlConnectionStringBuilder(conexaoTexto);
        if (!string.Equals(construtor.InitialCatalog, NomeBancoScript, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Criação automática ignorada: a conexão aponta para {Banco}, mas o script cria {Script}.",
                construtor.InitialCatalog, NomeBancoScript);
            return;
        }

        construtor.InitialCatalog = "master";
        await using var conexao = new SqlConnection(construtor.ConnectionString);
        await conexao.OpenAsync();

        await using (var verificar = new SqlCommand("SELECT DB_ID(@nome)", conexao))
        {
            verificar.Parameters.AddWithValue("@nome", NomeBancoScript);
            var resultado = await verificar.ExecuteScalarAsync();
            if (resultado is not null && resultado is not DBNull)
            {
                logger.LogInformation("Banco {Banco} já existe. Nenhum script executado.", NomeBancoScript);
                return;
            }
        }

        var caminho = Path.Combine(AppContext.BaseDirectory, "Database", "DiarioRoxoDB.sql");
        if (!File.Exists(caminho))
            throw new FileNotFoundException("Script do banco não encontrado.", caminho);

        var script = await File.ReadAllTextAsync(caminho);
        var lotes = Regex.Split(script, @"^\s*GO\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        logger.LogInformation("Criando o banco {Banco} a partir do script...", NomeBancoScript);
        try
        {
            foreach (var lote in lotes)
            {
                if (string.IsNullOrWhiteSpace(lote)) continue;
                await using var comando = new SqlCommand(lote, conexao) { CommandTimeout = 180 };
                await comando.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // Desfaz a criação parcial para que a próxima execução tente de novo do zero.
            try
            {
                await using var desfazer = new SqlCommand(
                    $"USE master; IF DB_ID('{NomeBancoScript}') IS NOT NULL BEGIN " +
                    $"ALTER DATABASE [{NomeBancoScript}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                    $"DROP DATABASE [{NomeBancoScript}]; END", conexao);
                await desfazer.ExecuteNonQueryAsync();
            }
            catch (Exception erroDesfazer)
            {
                logger.LogWarning(erroDesfazer, "Não foi possível remover o banco criado parcialmente.");
            }
            throw;
        }

        logger.LogInformation("Banco {Banco} criado e populado com sucesso.", NomeBancoScript);
    }
}
