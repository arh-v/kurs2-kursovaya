using BuildingStatePredictionApp.Models;
using System.Numerics;

namespace BuildingStatePredictionAppTests;

[TestFixture]
public class BlockMathHelperTests
{
    private readonly double[] _firstEpochHeights = [ 101.6664, 101.8940, 101.7054, 101.5935,
        101.8039, 101.3931, 101.3413, 101.4250, 101.3869, 101.1702, 101.1459, 101.2865, 101.6776, 101.7572,
        101.8106, 101.1941, 101.0018, 101.0849, 101.0496, 101.0576, 101.0632, 101.0644, 101.0666, 101.0661,
        101.0658, 101.0657 ];

    private readonly double[] _secondEpochHeights = [ 101.6632, 101.8982, 101.7086, 101.5977,
        101.8059, 101.3904, 101.3390, 101.4209, 101.3824, 101.1665, 101.1449, 101.2822, 101.6822, 101.7562,
        101.8096, 101.1968, 101.0042, 101.0857, 101.0456, 101.0551, 101.0621, 101.0637, 101.0665, 101.0660,
        101.0704, 101.0703 ];

    private readonly double[] _standartLengthsValues = [ 516.7362707, 516.7356903, 516.7338312, 516.7307589,
        516.7337282, 516.7367253, 516.7393549, 516.732523, 516.7351178, 516.731873, 516.7340472, 516.7329363,
        516.7354799, 516.741589, 516.733682 ];

    [TestCase(TestName = "Вычисление M по стандартным имитационным данным.")]
    public void TestLengthCalculationWithStandartData()
    {
        TestLengthCalculation(_firstEpochHeights, 516.7363);
    }


    [TestCase(new double[] { }, 0, TestName = "Вычисление M с пустой коллекцией (нуль-вектор).")]
    public void TestLengthCalculation(double[] inputValues, double expected)
    {
        var actual = BlockMathHelper.CalculateM(inputValues);
        Assert.That(Math.Round(actual, 4), Is.EqualTo(expected));
    }

    [TestCase(TestName = "Вычисление M с неинициализированной коллекцией.")]
    public void TestLengthCalculationNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BlockMathHelper.CalculateM(null));
    }

    [TestCase(TestName = "Вычисление угла a по стандартным имитационным данным.")]
    public void TestAngleCalculationWithStandartData()
    {
        var firstEpochM = BlockMathHelper.CalculateM(_firstEpochHeights);
        var secondEpochM = BlockMathHelper.CalculateM(_secondEpochHeights);

        TestAngleCalculation(_firstEpochHeights, _secondEpochHeights, firstEpochM, secondEpochM, 0.000030315);
    }

    [TestCase(new[] { 2.0, 0 }, new[] { 2.0, 0 }, 2, 2, 0, TestName = "Угол между одинаковыми векторами.")]
    [TestCase(new double[] { }, new double[] { }, 0, 0, double.NaN, TestName = "Угол между нуль-векторами.")]
    [TestCase(new[] { 2.0, 0 }, new[] { 2.0, 0, 0 }, 2, 2, double.NaN,
        TestName = "Угол между векторами с разным количеством элементов.")]
    public void TestAngleCalculation(double[] firstEpochHeights, double[] currentEpochHeights,
        double firstEpochM, double currentEpochM, double expected)
    {
        var actual = BlockMathHelper.CalculateAngle(firstEpochHeights, currentEpochHeights, firstEpochM,
            currentEpochM);
        Assert.That(Math.Round(actual, 9), Is.EqualTo(expected));
    }

    [TestCase(TestName = "Вычисление угла a с неинициализированными коллекциями.")]
    public void TestAngleCalculationNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BlockMathHelper.CalculateAngle(null, null, 0, 0));
    }

    [TestCase(0.9, 516.7338, 516.7357, 516.7340, TestName = "Сглаживание с коэффициентом 0,9.")]
    public void TestSmoothedSequenceMemberCalculation(double factor, double original, double previousMember,
        double expected)
    {
        var actual = BlockMathHelper.CalculateSmoothedSequenceMember(factor, original, previousMember);
        Assert.That(Math.Round(actual, 4), Is.EqualTo(expected));
    }

    [TestCase(-0.9, TestName = "Сглаживание с отрицательным коэффициентом.")]
    [TestCase(1.1, TestName = "Сглаживание с коэффициентом выходящим за диапазон [0; 1].")]
    public void TestSmoothedSequenceMemberCalculationException(double factor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BlockMathHelper.CalculateSmoothedSequenceMember(factor, 516.7338, 516.7357));
    }

    [TestCase(0.9, 516.734861, TestName = "Прогноз по стандартным данным.")]
    public void TestPredictionMaking(double factor, double expected)
    {
        var actual = BlockMathHelper.CalculatePredictionValues(_standartLengthsValues, factor)[^1];
        Assert.That(Math.Round(actual, 6), Is.EqualTo(expected));
    }

    [TestCase(-0.9, TestName = "Прогноз с отрицательным коэффициентом.")]
    [TestCase(1.1, TestName = "Прогноз с коэффициентом выходящим за диапазон [0; 1].")]
    public void TestPredictionMakingOutOfRangeException(double factor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BlockMathHelper.CalculatePredictionValues(_standartLengthsValues, factor));
    }

    [TestCase(TestName = "Прогноз с неинициализированной коллекцией.")]
    public void TestPredictionMakingNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            BlockMathHelper.CalculatePredictionValues(null, 0.9));
    }
}
