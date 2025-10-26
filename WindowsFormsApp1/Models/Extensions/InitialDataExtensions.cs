using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BuildingStatePredictionApp.Models.Extensions;

public static class InitialDataExtensions
{
    public static double[] AggregateColumns(this IReadonlyInitialData initials,
        Func<double, double, double> func, double[] oldValues, bool tableChanged)
    {
        if (oldValues == null)
        {
            return initials.Heights.GetCells()
                .GroupBy(c => c.Column, c => c.ToDouble())
                .Skip(1)
                .Select(g => g.Aggregate(func))
                .ToArray();
        }

        if (tableChanged)
        {
            return initials.Heights.GetCells()
                .GroupBy(c => c.Row, c => c.ToDouble())
                .Last()
                .Skip(1)
                .Zip(oldValues, func)
                .ToArray();
        }

        return oldValues;
    }

    public static IEnumerable<IEnumerable<double>> GetHeightsRowsValues(this IReadonlyInitialData initials)
    {
        ArgumentNullException.ThrowIfNull(initials);

        if (initials.Heights.InfoAbout.RowsCount <= 0) return [];

        return initials.GetHeightsRowsValues(Enumerable.Range(1, initials.Heights.InfoAbout.ColumnsCount - 1).ToHashSet());
    }

    public static IEnumerable<IEnumerable<double>> GetHeightsRowsValues(this IReadonlyInitialData initials,
        ICollection<int> points)
    {
        if (points.Count <= 0) throw new ArgumentException("Для добавления блока нужно выбрать точки!", nameof(points));

        if (points.Count > initials.Heights.InfoAbout.ColumnsCount)
        {
            throw new ArgumentException("Число точек превышает число колонок таблицы.", nameof(points));
        }

        if (initials.Heights.InfoAbout.RowsCount <= 0) return [];

        return initials.Heights.GetCells()
            .Where(c => points.Contains(c.Column))
            .GroupBy(c => c.Row)
            .OrderBy(g => g.Key)
            .Select(g => g
                .OrderBy(c => c.Column)
                .Select(c => c.ToDouble()));
    }
}

