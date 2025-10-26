using BuildingStatePredictionApp.Models;
using BuildingStatePredictionApp.Models.Extensions;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace BuildingStatePredictionApp.ViewModels;

internal class LevelOneViewModel : BindableBase
{
    private Block _model;

    public ChartViewModel AmPlot { get; private set; }

    public ChartViewModel MtPlot { get; private set; }

    public BindingList<BlockEpoch> Calculations { get; private set; }

    public void Calculate(IReadonlyInitialData initials)
    {
        var points = Enumerable.Range(1, initials.Heights.InfoAbout.ColumnsCount - 1).ToHashSet();
        _model = new(initials, points, "");
        AmPlot = new(_model, ChartManagerType.Am);
        MtPlot = new(_model, ChartManagerType.Mt);
        RaisePropertyChanged(nameof(AmPlot));
        RaisePropertyChanged(nameof(MtPlot));
        Calculations = new([.. _model.Calculations]);
        Calculations.Watch(_model.Calculations, v => v);
        RaisePropertyChanged(nameof(Calculations));
    }
}