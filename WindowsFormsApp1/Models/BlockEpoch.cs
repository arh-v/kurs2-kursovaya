using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace BuildingStatePredictionApp.Models;

public class BlockEpoch : BindableBase
{
    private double _mSmoothed;
    private double _aSmoothed;
    private double _mSmoothedPlus;
    private double _aSmoothedPlus;
    private double _mSmoothedMinus;
    private double _aSmoothedMinus;

    [Browsable(false)]
    public int PointsCount { get; }

    [Browsable(false)]
    public IReadOnlyList<double> Heights { get; }

    [Browsable(false)]
    public int Number { get; private set; }

    [Browsable(false)]
    public bool IsPrediction { get; private set; }

    [DisplayName("Эпоха")]
    public string Name { get; private set; }

    public double M { get; private set; }

    [DisplayName("a")]
    public double A { get; private set; }

    [DisplayName("Mсгл")]
    public double MSmoothed
    {
        get => _mSmoothed;
        set => SetProperty(ref _mSmoothed, value);
    }

    [DisplayName("aсгл")]
    public double ASmoothed
    {
        get => _aSmoothed;
        set => SetProperty(ref _aSmoothed, value);
    }

    [DisplayName("M+")]
    public double MPlus { get; private set; }

    [DisplayName("a+")]
    public double APlus { get; private set; }

    [DisplayName("Mсгл+")]
    public double MSmoothedPlus
    {
        get => _mSmoothedPlus;
        set => SetProperty(ref _mSmoothedPlus, value);
    }

    [DisplayName("aсгл+")]
    public double ASmoothedPlus
    {
        get => _aSmoothedPlus;
        set => SetProperty(ref _aSmoothedPlus, value);
    }

    [DisplayName("M-")]
    public double MMinus { get; private set; }

    [DisplayName("a-")]
    public double AMinus { get; private set; }

    [DisplayName("Mсгл-")]
    public double MSmoothedMinus
    {
        get => _mSmoothedMinus;
        set => SetProperty(ref _mSmoothedMinus, value);
    }

    [DisplayName("aсгл-")]
    public double ASmoothedMinus
    {
        get => _aSmoothedMinus;
        set => SetProperty(ref _aSmoothedMinus, value);
    }

    [DisplayName("ε'")]
    public double Epsilon { get; private set; }

    [DisplayName("R")]
    public double R { get; private set; }

    [DisplayName("|Mi - M0|")]
    public double MDeviation { get; private set; }

    [DisplayName("Состояние")]
    public string State =>
        Math.Abs(R - MDeviation) < 1e-16 ? "Предаварийное" : R > MDeviation ? "Нормальное" : "Аварийное";

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="BlockEpoch"/>, представляющий собой первую эпоху.
    /// </summary>
    /// <param name="heights">Высоты точек на первой эпохе</param>
    /// <param name="accuracy">Точность</param>
    /// <param name="smoothingFactor">Коэффициент экспоненциального сглаживания</param>
    public BlockEpoch(IEnumerable<double> heights, double accuracy, double smoothingFactor)
    {
        Heights = heights.ToList();
        PointsCount = Heights.Count;
        Name = "0";
        Number = 0;
        Init(accuracy, smoothingFactor);
    }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="BlockEpoch"/>, представляющий собой N-ную эпоху.
    /// </summary>
    /// <param name="firstEpoch">Первая эпоха</param>
    /// <param name="heights">Высоты точек на текущей эпохе</param>
    /// <param name="accuracy">Точность</param>
    /// <param name="smoothingFactor">Коэффициент экспоненциального сглаживания</param>
    /// <param name="number">Номер эпохи, начиная с 1.</param>
    public BlockEpoch(BlockEpoch firstEpoch, IEnumerable<double> heights, double accuracy,
        double smoothingFactor, int number)
    {
        if (number < 1) throw new ArgumentOutOfRangeException(nameof(number), "Номер должен быть не меньше 1.");

        Heights = heights.ToList();
        PointsCount = Heights.Count;
        Name = number.ToString();
        Number = number;
        Init(firstEpoch, accuracy, smoothingFactor);
    }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="BlockEpoch"/>, представляющий собой прогноз по предыдущим эпохам.
    /// <para>
    /// Временная сложность - O(n^2).
    /// </para>
    /// </summary>
    /// <param name="all">Последовательность предыдущих эпох</param>
    /// <param name="accuracy">Точность</param>
    /// <param name="smoothingFactor">Коэффициент экспоненциального сглаживания</param>
    public BlockEpoch(IEnumerable<BlockEpoch> all, double accuracy, double smoothingFactor)
    {
        //var transposedHeights = all.SelectMany(b => b.Heights
        //        .Select((h, i) => (i, h)))
        //    .GroupBy(t => t.i, t => t.h);
        //Heights = transposedHeights.Select(h => BlockCalculations.CalculatePredictionValues(h, smoothingFactor)[^1]);
        var MPredictionValues = BlockMathHelper.CalculatePredictionValues(all.Select(b => b.M), smoothingFactor);
        var APredictionValues = BlockMathHelper.CalculatePredictionValues(all.Select(b => b.A), smoothingFactor);
        var MPlusPredictionValues = BlockMathHelper.CalculatePredictionValues(all.Select(b => b.MPlus),smoothingFactor);
        var APlusPredictionValues = BlockMathHelper.CalculatePredictionValues(all.Select(b => b.APlus), smoothingFactor);
        var MMinusPredictionValues = BlockMathHelper.CalculatePredictionValues(all.Select(b => b.MMinus), smoothingFactor);
        var AMinusPredictionValues = BlockMathHelper.CalculatePredictionValues(all.Select(b => b.AMinus), smoothingFactor);
        
        foreach (var (block, i) in all.Append(this).Select((b, i) => (b, i)))
        {
            block.MSmoothed = MPredictionValues[i];
            block.ASmoothed = APredictionValues[i];
            block.MSmoothedPlus = MPlusPredictionValues[i];
            block.ASmoothedPlus = APlusPredictionValues[i];
            block.MSmoothedMinus = MMinusPredictionValues[i];
            block.ASmoothedMinus = AMinusPredictionValues[i];
        }

        IsPrediction = true;
        PointsCount = all.Max(b => b.PointsCount);
        Name = "Прогноз";
        var firstEpoch = all.First();
        Number = all.Last().Number + 1;
        M = MPredictionValues[^1];
        MPlus = MPlusPredictionValues[^1];
        MMinus = MMinusPredictionValues[^1];
        A = APredictionValues[^1];
        APlus = APlusPredictionValues[^1];
        AMinus = AMinusPredictionValues[^1];
        Epsilon = MPlus - MMinus;
        R = Epsilon / 2;
        MDeviation = Math.Abs(firstEpoch.M - M);
        //Init(firstEpoch, accuracy, smoothingFactor);
    }

    private void Init(double accuracy, double smoothingFactor) => Init(this, accuracy, smoothingFactor);

    private void Init(BlockEpoch firstEpoch, double accuracy, double smoothingFactor)
    {
        CalculateMs(accuracy);
        CalculateAngles(firstEpoch, accuracy);
        Epsilon = MPlus - MMinus;
        R = Epsilon / 2;
        MDeviation = firstEpoch == null ? 0 : Math.Abs(firstEpoch.M - M);
    }

    private void CalculateAngles(BlockEpoch firstEpoch, double accuracy)
    {
        if (firstEpoch == null || firstEpoch == this)
        {
            A = 0;
            APlus = 0;
            AMinus = 0;
            return;
        }

        A = BlockMathHelper.CalculateAngle(firstEpoch.Heights, Heights, firstEpoch.M, M);
        APlus = BlockMathHelper.CalculateAngle(firstEpoch.Heights.Select(i => i + accuracy),
            Heights.Select(i => i + accuracy), firstEpoch.MPlus, MPlus);
        AMinus = BlockMathHelper.CalculateAngle(firstEpoch.Heights.Select(i => i - accuracy),
            Heights.Select(i => i - accuracy), firstEpoch.MMinus, MMinus);
    }

    private void CalculateMs(double accuracy)
    {
        M = BlockMathHelper.CalculateM(Heights);
        MPlus = BlockMathHelper.CalculateM(Heights.Select(i => i + accuracy));
        MMinus = BlockMathHelper.CalculateM(Heights.Select(i => i - accuracy));
    }
}