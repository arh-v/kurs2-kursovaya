using BuildingStatePredictionApp.Models.Extensions;
using Microsoft.Data.Sqlite;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

namespace BuildingStatePredictionApp.Models;

public class InitialDataManager : BindableBase
{
    private double[] _maxInitialValues;
    private double[] _minInitialValues;
    private int _lastTableLength;
    private InitialData _initials = new();
    private ObservableCollection<string> _tablesNames = [];
    private string _databasePath;

    public double Accuracy
    {
        get => _initials.Accuracy;
        set => _initials.Accuracy = value;
    }

    public double SmoothingFactor
    {
        get => _initials.SmoothingFactor;
        set => _initials.SmoothingFactor = value;
    }

    public double[] MaxInitialValues
    {
        get
        {
            var tableChanged = _lastTableLength != _initials.Heights.InfoAbout.RowsCount;
            MaxInitialValues = _initials.AggregateColumns(Math.Max, _maxInitialValues, tableChanged);
            _lastTableLength = _initials.Heights.InfoAbout.RowsCount;
            return _maxInitialValues;
        }
        private set => _maxInitialValues = value;
    }

    public double[] MinInitialValues
    {
        get
        {
            var tableChanged = _lastTableLength != _initials.Heights.InfoAbout.RowsCount;
            MinInitialValues = _initials.AggregateColumns(Math.Min, _minInitialValues, tableChanged);
            _lastTableLength = _initials.Heights.InfoAbout.RowsCount;
            return _minInitialValues;
        }
        private set => _minInitialValues = value;
    }

    public Image BuildingScheme
    {
        get => _initials.BuildingScheme;
        set => _initials.BuildingScheme = value;
    }

    public IListSource Heights => _initials.Heights.BindableData;

    public IReadonlyInitialData Initials => _initials;

    public ReadOnlyObservableCollection<string> TablesNames { get; }

    public event EventHandler<EventArgs> TableDataLoaded;

    public InitialDataManager()
    {
        TablesNames = new(_tablesNames);
    }

    public void ConnectToDatabase(string databasePath)
    {
        ArgumentNullException.ThrowIfNull(databasePath, nameof(databasePath));
        ArgumentException.ThrowIfNullOrEmpty(databasePath, nameof(databasePath));

        if (!File.Exists(databasePath)) throw new FileNotFoundException(databasePath);

        _tablesNames.Clear();
        using var connection = DataLoadingHelper.ConnectToDatabase(databasePath);
        _databasePath = connection.DataSource;
        var folder = Directory.GetParent(_databasePath).FullName;
        var textFile = folder + "\\Описание объекта.txt";
        var imageFile = folder + "\\Схема объекта.png";

        foreach (var name in DataLoadingHelper.LoadTableNames(connection))
        {
            _tablesNames.Add(name);
        }

        Accuracy = DataLoadingHelper.GetAccuracyFromTxt(textFile, "Точность измерений:");
        BuildingScheme = Image.FromFile(imageFile);
    }

    public void LoadData(string tableName)
    {
        ArgumentException.ThrowIfNullOrEmpty(tableName, nameof(tableName));
        _initials.Heights.ResetTable();
        RaisePropertyChanged(nameof(Heights));

        try
        {
            using var connection = DataLoadingHelper.ConnectToDatabase(_databasePath);
            var columnsNames = DataLoadingHelper.LoadColumnsNames(connection, tableName);
            var query = DataLoadingHelper.MakeQueryToGetDoubles(columnsNames, tableName);
            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();
            _initials.Heights.LoadTable(reader);
            TableDataLoaded?.Invoke(this, EventArgs.Empty);
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException(DatabasePathNotFound(), _databasePath, ex);
        }
    }

    public void AddLine(string tableName)
    {
        ArgumentException.ThrowIfNullOrEmpty(tableName, nameof(tableName));

        if (_initials.Heights.InfoAbout.RowsCount < 2) return;

        var rnd = new Random();
        var deviations = MinInitialValues.Zip(MaxInitialValues, (i, j) => Math.Abs(i - j))
            .Select(d => d * rnd.Next(-10000, 10000) / 20000.0);
        var mediums = MinInitialValues.Zip(MaxInitialValues, (i, j) => (i + j) / 2.0);
        var rowValues = deviations.Zip(mediums, (i, j) => (object)Math.Round(i + j, 4));
        var rowValuesWithEpochNumber = rowValues.Prepend(_initials.Heights.InfoAbout.RowsCount.ToString()).ToArray();
        var values = string.Join(',', rowValuesWithEpochNumber.Select(v => $"'{v}'"));
        var columns = string.Join(',', _initials.Heights.InfoAbout.ColumnsNames.Select(c => $"[{c}]"));
        var commandText = $"INSERT INTO [{tableName}] ({columns}) values({values})";

        try
        {
            using var connection = DataLoadingHelper.ConnectToDatabase(_databasePath);
            using var command = new SqliteCommand(commandText, connection);
            command.ExecuteNonQuery();
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException(DatabasePathNotFound(), _databasePath, ex);
        }

        _initials.Heights.AddLine(rowValuesWithEpochNumber);
    }

    public void RemoveLine(string tableName)
    {
        if (_initials.Heights.InfoAbout.RowsCount <= 2) return;

        var epoch = _initials.Heights.GetCells().Where(c => c.Column == 0).OrderBy(c => c.Row).Last().Value;
        var query = $"Delete from [{tableName}] where [Эпоха] = {epoch}";

        try
        {
            using var connection = DataLoadingHelper.ConnectToDatabase(_databasePath);
            using var Command = new SqliteCommand(query, connection);
            Command.ExecuteNonQuery();
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException(DatabasePathNotFound(), _databasePath, ex);
        }

        _initials.Heights.RemoveLine();
    }

    private string DatabasePathNotFound() =>
        $"Путь к базе данных не найден, необходимо вызвать {nameof(ConnectToDatabase)}.";
}