using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using static BuildingStatePredictionApp.Models.HeightsRowsCollectionChangedEventArgs;

namespace BuildingStatePredictionApp.Models;

public class Block : BindableBase
{
    private ObservableCollection<BlockEpoch> _calculations = [];
    private HashSet<int> _points;
    private HeightsTableManager _heights = new();

    public IReadOnlyHeightsTableManager Heights => _heights;

    public string Name { get; }

    public int PointsCount => _points.Count;

    public IReadonlyInitialData Initials { get; }

    public ReadOnlyObservableCollection<BlockEpoch> Calculations { get; }

    public event EventHandler<BlockDataChangedEventArgs> DataChanged;

    public Block(IReadonlyInitialData initials, ICollection<int> points, string name)
    {
        if (points.Count <= 0) throw new ArgumentException("Для добавления блока нужно выбрать точки!", nameof(points));

        if (points.Count > initials.Heights.InfoAbout.ColumnsCount)
        {
            throw new ArgumentException("Число точек превышает число колонок таблицы.", nameof(points));
        }

        _points = [.. points];
        Name = name;
        Calculations = new(_calculations);
        Initials = initials;
        FillData();
        Initials.Heights.CollectionChanged += UpdateData;
    }

    private void UpdateData(object sender, HeightsRowsCollectionChangedEventArgs e)
    {
        if (e.Action == HeightsCollectionChangedAction.Add) AddNewEpoch(e.NewRow.ToArray());

        if (e.Action == HeightsCollectionChangedAction.Remove) RemoveEpoch();

        if (e.Action == HeightsCollectionChangedAction.RemoveExtra) RemoveExtraEpoch();

        if (e.Action == HeightsCollectionChangedAction.Clear) _calculations.Clear();

        RaisePropertyChanged(nameof(Calculations));
        DataChanged?.Invoke(this, new(e));
    }

    private void RemoveExtraEpoch()
    {
        _heights.RemoveExtraLine();
        Recalculate();
    }

    private void AddNewEpoch(DataCell[] row)
    {
        _heights.AddLine(row);
        var heights = row.Skip(1)
            .Where(c => _heights.InfoAbout.ColumnsNames.Contains(c.ColumnName))
            .Select(r => r.ToDouble());
        var epochNumber = Convert.ToInt32(row.First().Value);
        AddNewEpochCalculation(heights, epochNumber);
    }

    private void Recalculate()
    {
        _calculations.Clear();
        FillCalculations();
    }

    private void RemoveEpoch()
    {
        _heights.RemoveLine();
        _calculations.RemoveAt(Calculations.Count - 1);
        _calculations.RemoveAt(Calculations.Count - 1);
        CalculatePredictionEpoch();
    }

    private void FillData()
    {
        var columnsNames = Initials.Heights.GetCells()
            .Where(c => c.Row == 0 && (_points.Contains(c.Column) || c.Column == 0))
            .OrderBy(c => c.Column)
            .Select(c => c.ColumnName);
        _heights.AddColumns(columnsNames.ToArray());
        Initials.Heights.CopyValuesTo(_heights);
        FillCalculations();
    }

    private void FillCalculations()
    {
        var heights = _heights.GetCells()
            .Where(c => c.Column != 0)
            .GroupBy(c => c.Row, c => c.ToDouble())
            .OrderBy(c => c.Key);

        foreach (var values in heights)
        {
            AddNewEpochCalculation(values, values.Key);
        }
    }

    private void AddNewEpochCalculation(IEnumerable<double> heights, int rowNumber)
    {
        if (_calculations.Count == 0)
        {
            CalculateFirstEpoch(heights);
            return;
        }

        if (_calculations[^1].IsPrediction)
        {
            _calculations.RemoveAt(_calculations.Count - 1);
        }

        if (Initials.Heights.InfoAbout.RowsCount > _calculations.Count)
        {
            CalculateCommonEpoch(heights, rowNumber);
        }

        if (Initials.Heights.InfoAbout.RowsCount == _calculations.Count)
        {
            CalculatePredictionEpoch();
        }
    }

    private void CalculateFirstEpoch(IEnumerable<double> heights)
    {
        var first = new BlockEpoch(heights, Initials.Accuracy, Initials.SmoothingFactor);
        _calculations.Add(first);
    }

    private void CalculateCommonEpoch(IEnumerable<double> heights, int number)
    {
        var current = new BlockEpoch(_calculations[0], heights, Initials.Accuracy, Initials.SmoothingFactor, number);
        _calculations.Add(current);
    }

    private void CalculatePredictionEpoch()
    {
        var prediction = new BlockEpoch(_calculations, Initials.Accuracy, Initials.SmoothingFactor);
        _calculations.Add(prediction);
    }
}

public class BlockDataChangedEventArgs(HeightsRowsCollectionChangedEventArgs e) : EventArgs
{
    public HeightsCollectionChangedAction Action { get; } = e.Action;
}