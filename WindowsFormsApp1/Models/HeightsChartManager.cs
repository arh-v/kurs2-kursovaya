using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingStatePredictionApp.Models;

public class HeightsChartManager : ChartManager
{
    private IReadOnlyHeightsTableManager _heights;

    public HeightsChartManager(IReadOnlyHeightsTableManager heights) : base(ChartManagerType.Ht)
    {
        _heights = heights;
        var heightsByRows = _heights.GetCells()
            .OrderBy(c => (c.Row, c.Column))
            .GroupBy(c => c.Row, (i, g) => g.Skip(1)
                .Select(c => GetValues(g.First(), c)));
        using var heightsEnum = heightsByRows.GetEnumerator();
        ExecuteOnEachSeries((name) =>
        {
            heightsEnum.MoveNext();
            _series[name] = new(heightsEnum.Current.ToList());
        });
        heights.CollectionChanged += Update;
    }

    private void Update(object sender, HeightsRowsCollectionChangedEventArgs e)
    {
        Update(e.Action, e.NewRow);
    }

    protected override void AddPoint(IReadOnlyList<DataCell> newRow)
    {
        ExecuteOnEachSeries((name, index) =>
        {
            if (!_series.ContainsKey(name))
            {
                _series[name] = [];
                _seriesNamesChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, name));
            }

            _series[name].Add(GetValues(newRow[0], newRow[index + 1]));
        });
    }

    protected override void RemovePoint()
    {
        ExecuteOnEachSeries((name) =>
        {
            _series[name].RemoveAt(_series[name].Count - 1);
        });
    }

    protected override void RemoveExtraPoint()
    {
        ExecuteOnEachSeries((name) =>
        {
            _series[name].RemoveAt(0);
        });
    }

    protected override void ClearPoints()
    {
        _series.Clear();
        _seriesNamesChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset, null));
    }

    private Tuple<double, double> GetValues(DataCell first, DataCell second)
    {
        return new(first.ToDouble(), second.ToDouble());
    }

    public override string[] GetSeriesNames()
    {
        var columns = _heights.GetCells()
            .Where(c => c.Row == 0)
            .OrderBy(c => c.Column)
            .Skip(1).Select(c => c.ColumnName);
        return columns.Select(n => $"{_ordinate}{n}({_absciss})").ToArray();
    }
}