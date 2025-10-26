using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BuildingStatePredictionApp.Models;

public class PointsLinksManager : BindableBase
{
    private IReadonlyBlocksManager _blocks;
    private Dictionary<string, PointsLinks> _links = [];
    private PointsLinks _current;

    private PointsLinks Current
    {
        get => _current;
        set
        {
            if (!SetProperty(ref _current, value)) return;

            RaisePropertyChanged(nameof(Lengths));
            RaisePropertyChanged(nameof(Deltas));
        }
    }

    public IListSource Lengths => _current?.LinksLengths;

    public IListSource Deltas => _current?.LinksDeltas;

    public PointsLinksManager(IReadonlyBlocksManager blocks)
    {
        _blocks = blocks;
        _blocks.BlocksCollectionChanged += Update;
        
        foreach (var b in blocks.Blocks)
        {
            _links[b.Name] = new(b);
        }
    }

    private void Update(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Current = null;
            _links = [];
        }

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems[0] is not Block b) return;

            _links[b.Name] = new(b);
        }
    }

    public void Choose(Block block)
    {
        Choose(block.Name);
    }

    public void Choose(string blockName)
    {
        Current = _links[blockName];
    }
}
