using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using static BuildingStatePredictionApp.Models.HeightsRowsCollectionChangedEventArgs;

namespace BuildingStatePredictionApp.Models;

public abstract class ChartManager : BindableBase, INotifySeriesNamesChanged
{
    private ChartManagerType _chartType;
    protected Dictionary<string, BindingList<Tuple<double, double>>> _series = [];
    protected string _absciss;
    protected string _ordinate;
    protected EventHandler<SeriesNamesChangedEventArgs> _seriesNamesChanged;

    public IReadOnlyDictionary<string, BindingList<Tuple<double, double>>> Series => _series;

    public event EventHandler<SeriesNamesChangedEventArgs> SeriesNamesChanged
    {
        add
        {
            _seriesNamesChanged += value;
        }
        remove
        {
            _seriesNamesChanged -= value;
        }
    }

    public ChartManager(ChartManagerType chartType)
    {
        _chartType = chartType;
        (_ordinate, _absciss) = GetCoordNames();
    }

    protected void Update(HeightsCollectionChangedAction action, IReadOnlyList<DataCell> newRow)
    {
        if (action == HeightsCollectionChangedAction.Add) AddPoint(newRow);

        if (action == HeightsCollectionChangedAction.Remove) RemovePoint();

        if (action == HeightsCollectionChangedAction.RemoveExtra) RemoveExtraPoint();

        if (action == HeightsCollectionChangedAction.Clear) ClearPoints();

        RaisePropertyChanged(nameof(Series));
    }

    protected abstract void AddPoint(IReadOnlyList<DataCell> newRow);

    protected abstract void RemovePoint();

    protected abstract void RemoveExtraPoint();

    protected abstract void ClearPoints();

    protected void ExecuteOnEachSeries(Action<string> actionOnSerieByName)
    {
        ExecuteOnEachSeries((n, i) => actionOnSerieByName(n));
    }

    protected void ExecuteOnEachSeries(Action<string, int> actionOnSerieByName)
    {
        var coordsNames = GetCoordNames();
        var names = GetSeriesNames();

        foreach (var (name, index) in names.Select((n, i) => (n, i)))
        {
            actionOnSerieByName(name, index);
        }
    }

    public abstract string[] GetSeriesNames();

    public (string o, string a) GetCoordNames()
    {
        return _chartType switch
        {
            ChartManagerType.Mt => ("M", "t"),
            ChartManagerType.Am => ("a", "M"),
            ChartManagerType.Ht => ("H", "t"),
            _ => throw new NotImplementedException()
        };
    }
}

public enum ChartManagerType
{
    Mt,
    Am,
    Ht
};

public interface INotifySeriesNamesChanged
{
    public event EventHandler<SeriesNamesChangedEventArgs> SeriesNamesChanged; 
}

public class SeriesNamesChangedEventArgs(NotifyCollectionChangedAction action, string newName) : EventArgs
{
    public NotifyCollectionChangedAction Action { get; } = action;
    public string NewSerie { get; } = newName;
}