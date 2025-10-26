using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using static BuildingStatePredictionApp.Models.HeightsRowsCollectionChangedEventArgs;

namespace BuildingStatePredictionApp.Models;

public class PointsLinks
{
    private Block _block;
    private DataTable _linksLengths = new();
    private DataTable _linksDeltas = new();
    private List<Link> _links;

    public IListSource LinksLengths => _linksLengths;

    public IListSource LinksDeltas => _linksDeltas;

    public PointsLinks(Block block)
    {
        _block = block;
        block.Heights.CollectionChanged += Update;
        MakeLinks();

        foreach (var column in _links.Select(l => l.ToString()).Prepend("Эпоха"))
        {
            _linksLengths.Columns.Add(column, typeof(object));
            _linksDeltas.Columns.Add(column, typeof(object));
        }

        FillTables();
    }

    private void MakeLinks()
    {
        var cells = _block.Heights.GetCells();
        var columnsNames = cells
            .Where(c => c.Row == 0)
            .OrderBy(c => c.Column)
            .Select(c => c.ColumnName);
        var pointsNames = columnsNames.Skip(1);
        _links = pointsNames.SelectMany((n1, i) => pointsNames
                .Skip(i + 1).Select(n2 => new Link(_block.Initials, cells, n1, n2)))
            .ToList();
    }

    private void FillTables()
    {
        var epochsNames = _block.Heights.GetCells()
            .Where(c => c.Column == 0)
            .OrderBy(c => c.Row)
            .Select(c => c.Value);
        var linksByEpochs = _links
            .SelectMany((l, i) => l.Values
                .Select((v, j) => (i, j, v)))
            .GroupBy(t => t.j, t => t.v);
        var rows = epochsNames.Zip(linksByEpochs, (e, l) => (e, l));

        foreach (var row in rows)
        {
            var lengthsRow = row.l.Select(l => (object)l.Length).Prepend(row.e).ToArray();
            var deltasRow = row.l.Select(l => (object)l.Delta).Prepend(row.e).ToArray();
            _linksLengths.Rows.Add(lengthsRow);
            _linksDeltas.Rows.Add(deltasRow);
        }

        _linksDeltas.Rows.Add(GetLinksStates());
    }

    private void Update(object sender, HeightsRowsCollectionChangedEventArgs e)
    {
        if (e.Action == HeightsCollectionChangedAction.Add)
        {
            var newHeightsCells = e.NewRow.Skip(1);
            var points = newHeightsCells.SelectMany((c1, i) => newHeightsCells
                .Skip(i + 1).Select(c2 => (First: c1.ToDouble(), Second: c2.ToDouble())));
            using var linkEnum = _links.GetEnumerator();
            using var pointEnum = points.GetEnumerator();

            while (linkEnum.MoveNext() && pointEnum.MoveNext())
            {
                var link = linkEnum.Current;
                var p = pointEnum.Current;
                link.AddEpoch(p.First, p.Second);
            }

            _linksDeltas.Rows.RemoveAt(_linksDeltas.Rows.Count - 1);
            var lengthsRow = _links.Select(l => (object)l.LastValues.Length).Prepend(e.NewRow[0].Value).ToArray();
            var deltasRow = _links.Select(l => (object)l.LastValues.Delta).Prepend(e.NewRow[0].Value).ToArray();
            _linksLengths.Rows.Add(lengthsRow);
            _linksDeltas.Rows.Add(deltasRow);
            _linksDeltas.Rows.Add(GetLinksStates());
        }

        if (e.Action == HeightsCollectionChangedAction.Remove)
        {
            _links.ForEach(l => l.RemoveEpoch());
            RemoveRowAt(_linksLengths.Rows.Count - 1);
        }

        if (e.Action == HeightsCollectionChangedAction.RemoveExtra)
        {
            _links.ForEach(l => l.RemoveExtraEpoch());
            RemoveRowAt(0);
        }
    }

    private void RemoveRowAt(int index)
    {
        _linksLengths.Rows.RemoveAt(index);
        _linksDeltas.Rows.RemoveAt(index);
        _linksDeltas.Rows.RemoveAt(_linksDeltas.Rows.Count - 1);
        _linksDeltas.Rows.Add(GetLinksStates());
    }

    private object[] GetLinksStates() => _links.Select(l => (object)l.State).Prepend("Состояние").ToArray();

    private class Link
    {
        private LinkedList<double> _lenghts;
        private LinkedList<double> _deltas;
        private int _badDeltasCount;
        private IReadonlyInitialData _initials;

        public string FirstPoint { get; }

        public string SecondPoint { get; }

        public IReadOnlyCollection<double> Lengths => _lenghts;

        public IReadOnlyCollection<double> Deltas => _deltas;

        public IEnumerable<(double Length, double Delta)> Values => _lenghts.Zip(_deltas, (a, b) => (a, b));

        public (double Length, double Delta) LastValues => (_lenghts.Last.Value, _deltas.Last.Value);

        public string State => _badDeltasCount > 0 ? "Не жесткая" : "Жесткая";

        public Link(IReadonlyInitialData initials, DataCell[] heights, string firstPoint, string secondPoint)
        {
            _initials = initials;
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
            var choosenValues = heights.Where(c => c.ColumnName == firstPoint || c.ColumnName == secondPoint)
                .OrderBy(c => (c.Column, c.Row))
                .GroupBy(c => c.ColumnName, c => c.ToDouble(), (k, g) => g.ToArray())
                .ToArray();
            var lengths = choosenValues[0].Zip(choosenValues[1], (i, j) => Math.Abs(i - j)).ToArray();
            _lenghts = new(lengths);
            _deltas = new(_lenghts.Where((v, i) => i + 1 < _lenghts.Count)
                .Select((v, i) =>
                {
                    var delta = Math.Abs(v - lengths[i + 1]);

                    if (IsBad(delta)) _badDeltasCount++;

                    return delta;
                }).Prepend(0));
        }

        public void AddEpoch(double firstValue, double secondValue)
        {
            _lenghts.AddLast(Math.Abs(firstValue - secondValue));
            var delta = Math.Abs(_lenghts.Last.Value - _lenghts.Last.Previous.Value);
            _deltas.AddLast(delta);
            
            if (IsBad(delta)) _badDeltasCount++;
        }

        public void RemoveEpoch()
        {
            _lenghts.RemoveLast();
            var delta = _deltas.Last.Value;
            _deltas.RemoveLast();

            if (IsBad(delta)) _badDeltasCount--;
        }

        public void RemoveExtraEpoch()
        {
            _lenghts.RemoveFirst();
            var delta = _deltas.First.Value;
            _deltas.RemoveFirst();

            if (IsBad(delta)) _badDeltasCount--;
        }

        private bool IsBad(double delta) => delta <= _initials.Accuracy;

        public override string ToString() => $"{FirstPoint}-{SecondPoint}";
    }
}
