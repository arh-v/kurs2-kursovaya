using BuildingStatePredictionApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Input;
using static BuildingStatePredictionApp.Models.HeightsRowsCollectionChangedEventArgs;

namespace BuildingStatePredictionApp.ViewModels;

internal class LevelFourViewModel : BindableBase
{
    private ChartViewModel _ht;
    private InitialDataManager _initials;
    private HeightsTableManager _heights;

    public IListSource Heights => _heights.BindableData;

    public ChartViewModel HtPlot
    {
        get => _ht;
        set => SetProperty(ref _ht, value);
    }

    public BindingList<int> Points => new(_initials.Initials.Heights.InfoAbout.Points.Select(p => p.Number).ToList());

    public BindingList<int> SelectedPoints { get; } = [];

    public Image BuildingScheme => _initials.BuildingScheme;

    public ICommand ApplyCommand { get; }

    public LevelFourViewModel(InitialDataManager initials)
    {
        _initials = initials;
        _heights = new();
        HtPlot = new(_heights);
        _initials.Initials.Heights.CollectionChanged += (s, e) =>
        {
            if (e.Action == HeightsCollectionChangedAction.Add) _heights.AddLine([.. e.NewRow]);

            if (e.Action == HeightsCollectionChangedAction.Remove) _heights.RemoveLine();

            if (e.Action == HeightsCollectionChangedAction.RemoveExtra) _heights.RemoveExtraLine();

            if (e.Action == HeightsCollectionChangedAction.Clear) _heights.ResetTable();
        };
        _initials.TableDataLoaded += (s, e) => RaisePropertyChanged(nameof(Points));
        ApplyCommand = new DelegateCommand(() =>
        {
            _heights.ResetTable();
            _heights.AddColumns(SelectedPoints.Select(p => p.ToString()).Prepend("Эпоха").ToArray());
            _initials.Initials.Heights.CopyValuesTo(_heights);
        });
    }
}