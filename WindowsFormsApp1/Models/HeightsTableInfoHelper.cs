using BuildingStatePredictionApp.Models.Extensions;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static BuildingStatePredictionApp.Models.HeightsTableInfoHelper;

namespace BuildingStatePredictionApp.Models;

public class HeightsTableInfoHelper : BindableBase, IHeightsTableInfo
{
    private DataTable _heights;
    private HashSet<EpochInfo> _epochs = [];
    private HashSet<PointInfo> _points = [];
    private HashSet<string> _columnsNames = [];

    public IReadOnlySet<PointInfo> Points => _points;

    public IReadOnlySet<EpochInfo> Epochs => _epochs;

    public int RowsCount => _heights.Rows.Count;

    public int ColumnsCount => _heights.Columns.Count;

    public IReadOnlySet<string> ColumnsNames => _columnsNames;

    public HeightsTableInfoHelper(DataTable heights)
    {
        _heights = heights;
    }

    public void AddEpochInfo(string Name, int Number)
    {
        _epochs.Add(new EpochInfo(Name, Number));
    }

    public void RemoveEpochInfo(string Name, int Number)
    {
        _epochs.Remove(new EpochInfo(Name, Number));
    }

    public void UpdateColumnsInfo()
    {
        _points.Clear();
        var points = _heights.SelectColumnsNames(c => c)
            .Skip(1)
            .Select(p => new PointInfo(p, int.Parse(p)));

        foreach (var p in points)
        {
            _points.Add(p);
        }
        
        _columnsNames.Clear();

        foreach (var p in _heights.SelectColumnsNames(c => c))
        {
            _columnsNames.Add(p);
        }
    }

    public record class PointInfo(string Name, int Number);

    public record class EpochInfo(string Name, int Number);
}

public interface IHeightsTableInfo
{
    public IReadOnlySet<PointInfo> Points { get; }

    public IReadOnlySet<EpochInfo> Epochs { get; }

    public int RowsCount { get; }

    public int ColumnsCount { get; }

    public IReadOnlySet<string> ColumnsNames { get; }
}