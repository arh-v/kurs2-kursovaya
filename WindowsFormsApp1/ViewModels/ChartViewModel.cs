using BuildingStatePredictionApp.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BuildingStatePredictionApp.ViewModels;

public class ChartViewModel : BindableBase
{
    private ChartManager _model;

    public IReadOnlyDictionary<string, BindingList<Tuple<double, double>>> Series => _model.Series;

    public ChartViewModel(Block b, ChartManagerType t)
    {
        _model = new BlockChartManager(b, (BlockChartManagerType)t);
        ModelChangedSet();
    }

    public ChartViewModel(IReadOnlyHeightsTableManager heights)
    {
        _model = new HeightsChartManager(heights);
        ModelChangedSet();
    }

    private void ModelChangedSet()
    {
        _model.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_model.Series))
            {
                RaisePropertyChanged(nameof(Series));
            }
        };
    }

    public (string o, string a) GetCoordNames() => _model.GetCoordNames();

    public string[] GetSeriesNames() => _model.GetSeriesNames();

    public event EventHandler<SeriesNamesChangedEventArgs> SeriesNamesChanged
    {
        add
        {
            _model.SeriesNamesChanged += value;
        }
        remove
        {
            _model.SeriesNamesChanged -= value;
        }
    }
}