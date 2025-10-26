using BuildingStatePredictionApp.Models;
using BuildingStatePredictionApp.Models.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Input;

namespace BuildingStatePredictionApp.ViewModels;

public class LevelThreeViewModel : BindableBase
{
    private int _selectedBlockIndex = -1;
    private InitialDataManager _initials;
    private PointsLinksManager _links;
    private BlocksManager _blocks;
    private BindingList<int> _points = [];
    private HashSet<int> _selectedPoints = [];
    private Block _subBlock;
    private ChartViewModel _am;
    private ChartViewModel _mt;

    private Block SubBlock
    {
        get => _subBlock;
        set
        {
            if (!SetProperty(ref _subBlock, value)) return;

            RaisePropertyChanged(nameof(SubBlockHeigths));
            RaisePropertyChanged(nameof(Calculations));
        }
    }

    private string SelectedBlockName => SelectedBlockIndex == -1 ? null : BlocksNames[SelectedBlockIndex];

    private PointsLinksManager Links
    {
        get => _links;
        set
        {
            if (!SetProperty(ref _links, value)) return;

            RaisePropertyChanged(nameof(LinksLengths));
            RaisePropertyChanged(nameof(LinksDeltas));
        }
    }

    public BindingList<int> Points => _points;

    public BindingList<int> SelectedPoints { get; }

    public IListSource LinksLengths => _links.Lengths;

    public IListSource LinksDeltas => _links.Deltas;

    public IListSource SubBlockHeigths => _subBlock?.Heights.BindableData;

    public BindingList<BlockEpoch> Calculations
    {
        get
        {
            if (_subBlock == null) return null;

            var calculations = _subBlock.Calculations;
            var list = new BindingList<BlockEpoch>([.. calculations]);
            list.Watch(calculations, e => e);
            return list;
        }
    }

    public ChartViewModel AmPlot
    {
        get => _am;
        private set => SetProperty(ref _am, value);
    }

    public ChartViewModel MtPlot
    {
        get => _mt;
        private set => SetProperty(ref _mt, value);
    }

    public BindingList<string> BlocksNames { get; }

    public int SelectedBlockIndex
    {
        get => _selectedBlockIndex;
        set => SetProperty(ref _selectedBlockIndex, value);
    }

    public Image BuildingScheme => _initials.BuildingScheme;

    public ICommand ShowBlockLinks { get; }

    public ICommand CalculateSubBlock { get; }

    public LevelThreeViewModel(InitialDataManager initials, BlocksManager blocks)
    {
        _links = new(blocks);
        _blocks = blocks;
        _initials = initials;
        BlocksNames = [];
        var observable = new ObservableCollection<int>();
        SelectedPoints = new(observable);
        observable.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) _selectedPoints.Clear();

            if (e.NewItems?.Count == 1) _selectedPoints.Add((int)e.NewItems[0]);

            if (e.OldItems?.Count == 1) _selectedPoints.Remove((int)e.OldItems[0]);
        };
        ShowBlockLinks = new DelegateCommand(() =>
        {
            if (SelectedBlockName == null) return;

            _links.Choose(SelectedBlockName);
            _points = new(_blocks.GetBlock(SelectedBlockName).Heights.InfoAbout.Points.Select(p => p.Number).ToList());
            RaisePropertyChanged(nameof(Points));
        });
        CalculateSubBlock = new DelegateCommand(() =>
        {
            SubBlock = new(_initials.Initials, _selectedPoints, "subBlock");
            AmPlot = new(_subBlock, ChartManagerType.Am);
            MtPlot = new(_subBlock, ChartManagerType.Mt);
        });
        _blocks.BlocksCollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                BlocksNames.Clear();
                SelectedBlockIndex = -1;
                RaisePropertyChanged(nameof(Points));
            }

            if (e.NewItems?.Count == 1)
            {
                BlocksNames.Add((e.NewItems[0] as Block).Name);
                SelectedBlockIndex = _blocks.Names.Count - 1;
            }

            if (e.OldItems?.Count == 1) BlocksNames.Remove((e.OldItems[0] as Block).Name);
        };
        _initials.TableDataLoaded += (s, e) =>
        {
            _links = new(_blocks);
            SubBlock = null;
        };
    }
}