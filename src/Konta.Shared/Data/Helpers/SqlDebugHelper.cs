using System.Collections;
using System.Text;
using Dapper;

namespace Konta.Shared.Data.Helpers;

/// <summary>
/// Aide au débogage des requêtes SQL en reconstruisant une version exécutable et lisible.
/// Utilisation statique pour les besoins de traçage exceptionnels.
/// </summary>
public static class SqlDebugHelper
{
    /// <summary>
    /// Génère une chaîne SQL où les paramètres sont remplacés par leurs valeurs réelles.
    /// </summary>
    /// <param name="sql">La requête SQL paramétrée.</param>
    /// <param name="parameters">L'objet contenant les paramètres.</param>
    /// <returns>La requête SQL avec les valeurs substituées.</returns>
    public static string GetReadableSql(string sql, object? parameters)
    {
        if (string.IsNullOrWhiteSpace(sql) || parameters == null) return sql;

        var debugQuery = new StringBuilder(sql);
        var paramValues = ExtractParamValues(parameters);

        var sortedKeys = paramValues.Keys.OrderByDescending(k => k.Length);

        foreach (var key in sortedKeys)
        {
            var placeholder = key.StartsWith("@") ? key : "@" + key;
            var formattedVal = FormatValue(paramValues[key]);
            debugQuery.Replace(placeholder, formattedVal);
        }

        return debugQuery.ToString();
    }

    private static Dictionary<string, object?> ExtractParamValues(object parameters)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (parameters is DynamicParameters dynamicParams)
        {
            foreach (var name in dynamicParams.ParameterNames)
                dict[name] = dynamicParams.Get<object>(name);
        }
        else
        {
            foreach (var prop in parameters.GetType().GetProperties())
                dict[prop.Name] = prop.GetValue(parameters);
        }
        return dict;
    }

    private static string FormatValue(object? val)
    {
        if (val == null || val == DBNull.Value) return "NULL";
        
        return val switch
        {
            string s => $"'{s.Replace("'", "''")}'",
            DateTime d => $"'{d:yyyy-MM-dd HH:mm:ss}'",
            Guid g => $"'{g}'",
            bool b => b ? "TRUE" : "FALSE",
            IEnumerable e and not string => "(" + string.Join(", ", e.Cast<object>().Select(FormatValue)) + ")",
            _ => val.ToString() ?? "NULL"
        };
    }
}
