using BuildingStatePredictionApp.Models;
using Prism.Mvvm;

namespace BuildingStatePredictionApp.ViewModels;

internal class MainViewModel : BindableBase
{
    private InitialDataManager _initials;

    public InitialDataViewModel InitialDataVm { get; }

    public LevelOneViewModel LevelOneVm { get; } = new();

    public LevelTwoViewModel LevelTwoVm { get; }

    public LevelThreeViewModel LevelThreeVm { get; }

    public LevelFourViewModel LevelFourVm { get; }

    public MainViewModel()
    {
        _initials = new();
        InitialDataVm = new(_initials);
        var blocks = new BlocksManager(_initials.Initials);
        LevelTwoVm = new(_initials, blocks);
        LevelThreeVm = new(_initials, blocks);
        LevelFourVm = new(_initials);
        InitialDataVm.OnApply += (s, e) => LevelOneVm.Calculate(_initials.Initials);
    }
}