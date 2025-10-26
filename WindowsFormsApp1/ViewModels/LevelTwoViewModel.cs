using BuildingStatePredictionApp.Models;
using BuildingStatePredictionApp.Models.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Input;

namespace BuildingStatePredictionApp.ViewModels;

internal class LevelTwoViewModel : BindableBase
{
    private int _selectedBlockIndex = -1;
    private InitialDataManager _initials;
    private BlocksManager _blocks;
    private HashSet<int> _selectedPoints = [];

    public BindingList<int> Points => new(_initials.Initials.Heights.InfoAbout.Points.Select(p => p.Number).ToList());

    public BindingList<int> SelectedPoints { get; }

    public IListSource BlockData
    {
        get
        {
            if (SelectedBlockName == null) return null;
            return _blocks.GetBlock(SelectedBlockName).Heights.BindableData;
        }
    }

    public BindingList<BlockEpoch> Calculations
    {
        get
        {
            if (SelectedBlockName == null) return null;

            var calculations = _blocks.GetBlock(SelectedBlockName).Calculations;
            var list = new BindingList<BlockEpoch>([.. calculations]);
            list.Watch(calculations, v => v);
            return list;
        }
    }

    public BindingList<string> BlocksNames { get; }

    private string SelectedBlockName => SelectedBlockIndex == -1 ? null : BlocksNames[SelectedBlockIndex];

    public int SelectedBlockIndex
    {
        get => _selectedBlockIndex;
        set
        {
            if (!SetProperty(ref _selectedBlockIndex, value)) return;

            RaisePropertyChanged(nameof(Calculations));
            RaisePropertyChanged(nameof(BlockData));
            RaisePropertyChanged(nameof(AmPlot));
            RaisePropertyChanged(nameof(MtPlot));
        }
    }

    public ChartViewModel AmPlot
    {
        get
        {
            if (SelectedBlockName == null) return null;

            return new(_blocks.GetBlock(SelectedBlockName), ChartManagerType.Am);
        }
    }

    public ChartViewModel MtPlot
    {
        get
        {
            if (SelectedBlockName == null) return null;

            return new(_blocks.GetBlock(SelectedBlockName), ChartManagerType.Mt);
        }
    }

    public Image BuildingScheme => _initials.BuildingScheme;

    public ICommand AddBlockCommand { get; }

    public event EventHandler<EventArgs> OnBlockAdd;

    public LevelTwoViewModel(InitialDataManager initials, BlocksManager blocks)
    {
        _initials = initials;
        _blocks = blocks;
        BlocksNames = [];
        AddBlockCommand = new DelegateCommand(() =>
        {
            _blocks.Add(_selectedPoints);
            SelectedBlockIndex = _blocks.Names.Count - 1;
        });
        var observable = new ObservableCollection<int>();
        SelectedPoints = new(observable);
        observable.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) _selectedPoints.Clear();

            if (e.NewItems?.Count == 1) _selectedPoints.Add((int)e.NewItems[0]);

            if (e.OldItems?.Count == 1) _selectedPoints.Remove((int)e.OldItems[0]);
        };
        _blocks.BlocksCollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                BlocksNames.Clear();
                SelectedBlockIndex = -1;
            }

            if (e.NewItems?.Count == 1) BlocksNames.Add((e.NewItems[0] as Block).Name);

            if (e.OldItems?.Count == 1) BlocksNames.Remove((e.OldItems[0] as Block).Name);
        };
        _initials.TableDataLoaded += (s, e) => RaisePropertyChanged(nameof(Points));
    }
}