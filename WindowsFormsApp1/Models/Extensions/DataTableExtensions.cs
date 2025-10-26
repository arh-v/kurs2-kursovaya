using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace BuildingStatePredictionApp.Models.Extensions;

public static class DataTableExtensions
{
    public static IEnumerable<TResult> SelectColumns<TResult>(this DataTable table, Func<IEnumerable<object>, int, TResult> selector)
    {
        return table.AsEnumerable()
                .SelectMany(r => r.ItemArray
                    .Select((v, i) => (v, i)))
                .GroupBy(t => t.i, t => t.v)
                .Select((g, i) => selector(g, i));
    }

    public static IEnumerable<TResult> SelectColumns<TResult>(this DataTable table, Func<IEnumerable<object>, TResult> selector)
    {
        return table.SelectColumns((c, i) => selector(c));
    }

    public static IEnumerable<TResult> SelectRows<TResult>(this DataTable table, Func<IEnumerable<object>, TResult> selector)
    {
        return table.SelectRows((r, i) => selector(r));
    }

    public static IEnumerable<TResult> SelectRows<TResult>(this DataTable table, Func<IEnumerable<object>, int, TResult> selector)
    {
        return table.AsEnumerable()
                .Select((r, i) => selector(r.ItemArray, i));
    }

    public static IEnumerable<TResult> SelectColumnsNames<TResult>(this DataTable table, Func<string, int, TResult> selector)
    {
        var index = 0;

        foreach (var column in table.Columns)
        {
            yield return selector(column.ToString(), index);
            index++;
        }
    }

    public static IEnumerable<TResult> SelectColumnsNames<TResult>(this DataTable table, Func<string, TResult> selector)
    {
        return table.SelectColumnsNames((c, i) => selector(c));
    }

    

    public static IEnumerable<TResult> SelectCells<TResult>(this DataTable table,
        Func<DataCell, int, TResult> selector)
    {
        return table.SelectCells(c => c).Select(selector);
    }

    public static IEnumerable<TResult> SelectCells<TResult>(this DataTable table,
        Func<DataCell, TResult> selector)
    {
        return table.AsEnumerable()
            .SelectMany((l, r) => l.ItemArray
                .Select((v, c) => new DataCell(table.Columns[c].ToString(), v, r, c)))
            .Select(c => selector(c));
    }
}