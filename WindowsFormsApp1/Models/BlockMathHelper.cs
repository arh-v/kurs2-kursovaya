using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingStatePredictionApp.Models;

public static class BlockMathHelper
{
    /// <summary>
    /// Метод для расчета длины вектора с заданными значениями.
    /// <para>
    /// Временная сложность выполнения метода - O(n).
    /// </para>
    /// </summary>
    /// <param name="values">Значения вектора.</param>
    /// <returns>Число <see cref="double"/>, представляющее длину вектора высот точек.</returns>
    public static double CalculateM(IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return Math.Sqrt(values.Sum(i => i * i));
    }

    /// <summary>
    /// Метод для расчета угла между векторами по значениям элементов и длинам.
    /// <para>
    /// Временная сложность выполнения метода - O(Min(n, m)).
    /// </para>
    /// </summary>
    /// <param name="firstEpochHeights">Высоты точек на первой эпохе.</param>
    /// <param name="currentEpochHeights">Высоты точек на текущей эпохе.</param>
    /// <param name="firstEpochM">Длина вектора высот первой эпохи.</param>
    /// <param name="currentEpochM">Длина вектора высот текущей эпохи.</param>
    /// <returns>Число <see cref="double"/>, представляющее значение угла отклонения.</returns>
    public static double CalculateAngle(IEnumerable<double> firstEpochHeights, IEnumerable<double> currentEpochHeights,
        double firstEpochM, double currentEpochM)
    {
        ArgumentNullException.ThrowIfNull(firstEpochHeights);
        ArgumentNullException.ThrowIfNull(currentEpochHeights);

        if (firstEpochHeights == currentEpochHeights)
        {
            return 0;
        }

        using var firstEnumerator = firstEpochHeights.GetEnumerator();
        using var secondEnumerator = currentEpochHeights.GetEnumerator();
        var sum = 0.0;

        while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext())
        {
            sum += firstEnumerator.Current * secondEnumerator.Current;
        }

        if (firstEnumerator.MoveNext() || secondEnumerator.MoveNext()) return double.NaN;

        return Math.Acos(sum / (firstEpochM * currentEpochM));
    }

    /// <summary>
    /// Метод для расчета члена сглаженной последовательности значений по текущему значению оригинальной
    /// последовательности, коэффициенту сглаживания и предыдущему сглаженному значению.
    /// <para>
    /// Временная сложность выполнения метода - O(1).
    /// </para>
    /// </summary>
    /// <param name="factor">Коэффициент экспоненциального сглаживания.</param>
    /// <param name="original">Сглаживаемое значение.</param>
    /// <param name="previousMember">Предыдущий член сглаженной последовательности.</param>
    /// <returns>Число <see cref="double"/>, представляющее значение члена сглаженной последовательности.</returns>
    public static double CalculateSmoothedSequenceMember(double factor, double original, double previousMember)
    {
        if (factor < 0 || factor > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Коэффициент должен быть в диапазоне [0; 1].");
        }

        return factor * original + (1 - factor) * previousMember;
    }

    /// <summary>
    /// Метод для расчета прогноза по заданным значениям и коэффициенту слаживания.
    /// <para>
    /// Временная сложность выполнения метода - O(n).
    /// </para>
    /// </summary>
    /// <param name="values">Множество значений.</param>
    /// <param name="smoothingFactor">Коэффициент экспоненциального сглаживания.</param>
    /// <returns>Лист с расчетными значениями для прогноза.</returns>
    public static List<double> CalculatePredictionValues(IEnumerable<double> values, double smoothingFactor)
    {
        ArgumentNullException.ThrowIfNull(values);

        var averageM = values.Average();
        var first = CalculateSmoothedSequenceMember(smoothingFactor, values.First(), averageM);
        var smoothed = new List<double>() { first };

        foreach (var (value, previousIndex) in values.Skip(1).Select((v, i) => (v, i)))
        {
            smoothed.Add(CalculateSmoothedSequenceMember(smoothingFactor, value, smoothed[previousIndex]));
        }

        smoothed.Add(CalculateSmoothedSequenceMember(smoothingFactor, smoothed.Average(), smoothed[^1]));
        return smoothed;
    }
}

