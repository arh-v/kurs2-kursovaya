using BuildingStatePredictionApp.Models.Extensions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using static BuildingStatePredictionApp.Models.HeightsRowsCollectionChangedEventArgs;

namespace BuildingStatePredictionApp.Models;

public class HeightsTableManager : BindableBase, IReadOnlyHeightsTableManager
{
    private DataTable _heights = new();
    private int _rowsLimit = 100;
    private HeightsTableInfoHelper _info;

    public IHeightsTableInfo InfoAbout => _info;

    public IListSource BindableData => _heights;

    public event EventHandler<HeightsRowsCollectionChangedEventArgs> CollectionChanged;

    public HeightsTableManager()
    {
        _info = new(_heights);
    }

    public void LoadTable(IDataReader reader)
    {
        _heights.Load(reader);

        if (_info.RowsCount > _rowsLimit) RemoveExtraLines();

        _info.UpdateColumnsInfo();
    }

    private void RemoveExtraLines()
    {
        while (_info.RowsCount > _rowsLimit)
        {
            RemoveExtraLine();
        }
    }

    public void AddLine(params object[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0 || InfoAbout.ColumnsCount == 0) return;

        values = values.Length <= _info.ColumnsCount ? values : values[.._info.ColumnsCount];
        _heights.Rows.Add(values);

        if (_info.RowsCount > _rowsLimit) RemoveExtraLines();

        var newRow = values.Zip(_info.ColumnsNames, (v, n) => (Value: v, Name: n))
            .Select((t, i) => new DataCell(t.Name, t.Value, _info.RowsCount - 1, i))
            .ToArray();
        var epochName = newRow[0].Value.ToString();
        _info.AddEpochInfo(epochName, int.Parse(epochName));
        CollectionChanged?.Invoke(this, new(newRow, HeightsCollectionChangedAction.Add));
    }

    public void AddLine(params DataCell[] row)
    {
        var values = row.Where(c => InfoAbout.ColumnsNames
                .Contains(c.ColumnName))
            .Select(c => c.Value)
            .ToArray();
        AddLine(values);
    }

    public void RemoveLine()
    {
        if (InfoAbout.RowsCount == 0) return;

        var removedEpochName = _heights.Rows[_info.RowsCount - 1][0].ToString();
        _heights.Rows.RemoveAt(_info.RowsCount - 1);
        _info.RemoveEpochInfo(removedEpochName, int.Parse(removedEpochName));
        CollectionChanged?.Invoke(this, new(null, HeightsCollectionChangedAction.Remove));
    }

    public void RemoveExtraLine()
    {
        if (InfoAbout.RowsCount == 0) return;

        var removedEpochName = _heights.Rows[0][0].ToString();
        _heights.Rows.RemoveAt(0);
        _info.RemoveEpochInfo(removedEpochName, int.Parse(removedEpochName));
        CollectionChanged?.Invoke(this, new(null, HeightsCollectionChangedAction.RemoveExtra));
    }

    public void ResetTable()
    {
        _heights.Clear();
        _heights.Columns.Clear();
        _info.UpdateColumnsInfo();
        CollectionChanged?.Invoke(this, new(null, HeightsCollectionChangedAction.Clear));
    }

    public void AddColumns(string[] names)
    {
        foreach (var column in names.Select(n => new DataColumn(n)))
        {
            _heights.Columns.Add(column);
        }

        _info.UpdateColumnsInfo();
    }

    public DataCell[] GetCells() => _heights.SelectCells(c => c).ToArray();

    public void CopyValuesTo(HeightsTableManager manager)
    {
        if (manager.InfoAbout.ColumnsCount == 0)
        {
            manager.AddColumns(InfoAbout.ColumnsNames.ToArray());
        }

        var rows = GetCells()
            .OrderBy(c => (c.Row, c.Column))
            .GroupBy(c => c.Row);

        foreach (var values in rows)
        {
            manager.AddLine(values.ToArray());
        }
    }
}

public interface IReadOnlyHeightsTableManager
{
    public IHeightsTableInfo InfoAbout { get; }

    public IListSource BindableData { get; }

    public event EventHandler<HeightsRowsCollectionChangedEventArgs> CollectionChanged;

    public DataCell[] GetCells();

    public void CopyValuesTo(HeightsTableManager manager);
}

public class HeightsRowsCollectionChangedEventArgs(DataCell[] newRow, HeightsCollectionChangedAction action)
    : EventArgs
{
    public enum HeightsCollectionChangedAction
    {
        Add,
        Remove,
        RemoveExtra,
        Clear
    }

    public HeightsCollectionChangedAction Action { get; } = action;

    public IReadOnlyList<DataCell> NewRow { get; } = newRow;
}

public class DataCell(string columnName, object value, int row, int column)
{
    public string ColumnName { get; } = columnName;

    public object Value { get; } = value;

    public int Row { get; } = row;

    public int Column { get; } = column;

    public double ToDouble() => Value?.ToString().TryParseInvariant(out var n) ?? false ? n : double.NaN;
}