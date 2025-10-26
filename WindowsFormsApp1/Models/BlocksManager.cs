using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BuildingStatePredictionApp.Models;

public class BlocksManager : BindableBase, IReadonlyBlocksManager
{
    private IReadonlyInitialData _initials;
    private ObservableCollection<Block> _blocks = [];
    private BlockNameGenerator _namer = new();
    private Dictionary<string, Block> _blocksWithNames = [];
    private ObservableCollection<string> _names = [];

    public ReadOnlyObservableCollection<Block> Blocks { get; private set; }

    public ReadOnlyObservableCollection<string> Names { get; private set; }

    public event NotifyCollectionChangedEventHandler BlocksCollectionChanged;

    public BlocksManager(IReadonlyInitialData initials)
    {
        _initials = initials;
        Blocks = new(_blocks);
        Names = new(_names);
        _initials.Heights.CollectionChanged += (s, e) =>
        {
            if (e.Action == HeightsRowsCollectionChangedEventArgs.HeightsCollectionChangedAction.Clear)
            {
                Clear();
            }
        };
    }

    public void Add(ICollection<int> points)
    {
        if (points.Count <= 0) throw new ArgumentException("Для добавления блока нужно выбрать точки!", nameof(points));

        if (points.Count > _initials.Heights.InfoAbout.ColumnsCount)
        {
            throw new ArgumentException("Число точек превышает число колонок таблицы.", nameof(points));
        }

        var name = _namer.NextName();
        var block = new Block(_initials, points, name);
        _blocksWithNames.Add(name, block);
        _blocks.Add(block);
        _names.Add(name);
        BlocksCollectionChanged?.Invoke(Blocks, new(NotifyCollectionChangedAction.Add, block));
    }

    public void Clear()
    {
        _namer = new();
        _blocks.Clear();
        _names.Clear();
        _blocksWithNames = [];
        RaisePropertyChanged(nameof(Blocks));
        RaisePropertyChanged(nameof(Names));
        BlocksCollectionChanged?.Invoke(Blocks, new(NotifyCollectionChangedAction.Reset));
    }

    public Block GetBlock(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _blocksWithNames[name];
    }
}

public interface IReadonlyBlocksManager
{
    public ReadOnlyObservableCollection<Block> Blocks { get; }

    public ReadOnlyObservableCollection<string> Names { get; }

    public event NotifyCollectionChangedEventHandler BlocksCollectionChanged;

    public Block GetBlock(string name);
}