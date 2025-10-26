using BuildingStatePredictionApp.Models.Extensions;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BuildingStatePredictionApp.Models;

public class DataLoadingHelper
{
    public static string[] LoadColumnsNames(SqliteConnection connection, string tableName)
    {
        ArgumentNullException.ThrowIfNull(connection, nameof(connection));
        ArgumentException.ThrowIfNullOrEmpty(tableName, nameof(tableName));

        if (connection.State == System.Data.ConnectionState.Closed) return [];

        using var command = new SqliteCommand($"select * from [{tableName}]", connection);
        using var reader = command.ExecuteReader();
        return reader.GetColumnSchema().Select(c => c.ColumnName).ToArray();
    }

    public static IEnumerable<string> LoadTableNames(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (connection.State == System.Data.ConnectionState.Closed) yield break;

        var SQLQuery = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
        using var command = new SqliteCommand(SQLQuery, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            yield return reader[0].ToString();
        }
    }

    public static SqliteConnection ConnectToDatabase(string file)
    {
        if (!File.Exists(file)) throw new FileNotFoundException(file, nameof(file));

        var connection = new SqliteConnection("Data Source=" + file);
        connection.Open();
        return connection;
    }

    public static double GetAccuracyFromTxt(string file, string text)
    {
        if (!File.Exists(file)) throw new FileNotFoundException(file, nameof(file));

        using var reader = new StreamReader(file, Encoding.Default);
        var splittedLineWithValue = reader.ReadLines()
            .Where(l => l.StartsWith(text))
            .Select(l => l.Split(':', StringSplitOptions.TrimEntries))
            .FirstOrDefault(l => l.Length == 2);

        if (splittedLineWithValue == null)
            throw new ArgumentException($"Значение для строки {text} в файле {file} не найдено", nameof(text));

        var filtered = splittedLineWithValue[1].Where(c => char.IsDigit(c) || c == '.' || c == ',');
        var value = new string([.. filtered]);
        return double.Parse(value);
    }

    public static string MakeQueryToGetDoubles(IEnumerable<string> columnsNames, string tableName)
    {
        var namesWithReplace = columnsNames.Select(c => $"replace(\"{c}\",',','.') as \"{c}\"");
        return $"SELECT {string.Join(",", namesWithReplace)} FROM [{tableName}]";
    }
}
