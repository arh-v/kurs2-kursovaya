using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingStatePredictionApp.Models.Extensions;

public static class BindingListExtensions
{
    public static void Watch<T1, T2>(this BindingList<T1> watcher, ICollection<T2> notifier, Func<T2, T1> converter)
    {
        if (notifier is INotifyCollectionChanged observable)

        observable.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) watcher.Clear();

            if (e.NewItems?.Count == 1) watcher.Add(converter((T2)e.NewItems[0]));

            if (e.OldItems?.Count == 1) watcher.Remove(converter((T2)e.OldItems[0]));
        };
    }
}
