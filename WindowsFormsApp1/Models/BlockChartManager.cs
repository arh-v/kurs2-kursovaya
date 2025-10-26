using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingStatePredictionApp.Models;

public class BlockChartManager : ChartManager
{
    private Block _block;

    public BlockChartManager(Block b, BlockChartManagerType t) : base((ChartManagerType)t)
    {
        _block = b;
        ExecuteOnEachSeries((name) =>
        {
            _series[name] = new(_block.Calculations.Select(e => GetValues(e, name)).ToList());
        });
        _block.DataChanged += Update;
    }

    private void Update(object sender, BlockDataChangedEventArgs e)
    {
        base.Update(e.Action, null);
    }

    protected override void AddPoint(IReadOnlyList<DataCell> newRow)
    {
        ExecuteOnEachSeries((name) =>
        {
            _series[name].RemoveAt(_series[name].Count - 1);
            var newEpochValues = GetValues(_block.Calculations[^2], name);
            var predictionValues = GetValues(_block.Calculations[^1], name);
            _series[name].Add(newEpochValues);
            _series[name].Add(predictionValues);
        });
    }

    protected override void RemovePoint()
    {
        ExecuteOnEachSeries((name) =>
        {
            _series[name].RemoveAt(_series[name].Count - 1);
            _series[name].RemoveAt(_series[name].Count - 1);
            var predictionValues = GetValues(_block.Calculations[^1], name);
            _series[name].Add(predictionValues);
        });
    }

    protected override void RemoveExtraPoint()
    {
        ExecuteOnEachSeries((name) =>
        {
            _series[name].Clear();
            var values = _block.Calculations.Select(e => GetValues(e, name));

            foreach (var value in values)
            {
                _series[name].Add(value);
            }
        });
    }

    protected override void ClearPoints()
    {
        ExecuteOnEachSeries((name) =>
        {
            _series[name].Clear();
        });
        _seriesNamesChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset, null));
    }

    public override string[] GetSeriesNames()
    {
        var signs = new string[] { "+", "-", "" };
        var predictSign = new string[] { " Прогноз", "" };
        return signs.SelectMany(s => predictSign.Select(p => $"{_ordinate}({_absciss}){s}{p}")).ToArray();
    }

    private Tuple<double, double> GetValues(BlockEpoch e, string n) => n switch
    {
        "M(t)" => new(e.Number, e.M),
        "M(t)+" => new(e.Number, e.MPlus),
        "M(t)-" => new(e.Number, e.MMinus),
        "M(t) Прогноз" => new(e.Number, e.MSmoothed),
        "M(t)+ Прогноз" => new(e.Number, e.MSmoothedPlus),
        "M(t)- Прогноз" => new(e.Number, e.MSmoothedMinus),
        "a(M)" => new(e.M, e.A),
        "a(M)+" => new(e.MPlus, e.APlus),
        "a(M)-" => new(e.MMinus, e.AMinus),
        "a(M) Прогноз" => new(e.MSmoothed, e.ASmoothed),
        "a(M)+ Прогноз" => new(e.MSmoothedPlus, e.ASmoothedPlus),
        "a(M)- Прогноз" => new(e.MSmoothedMinus, e.ASmoothedMinus),
        _ => throw new NotImplementedException()
    };
}

public enum BlockChartManagerType
{
    Mt,
    Am
};