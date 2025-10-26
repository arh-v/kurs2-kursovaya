using BuildingStatePredictionApp.Models;
using System;

namespace BuildingStatePredictionAppTests;

[TestFixture]
public class BlockEpochTests
{
    private readonly double[] _firstEpochHeights = [ 101.6664, 101.8940, 101.7054, 101.5935,
        101.8039, 101.3931, 101.3413, 101.4250, 101.3869, 101.1702, 101.1459, 101.2865, 101.6776, 101.7572,
        101.8106, 101.1941, 101.0018, 101.0849, 101.0496, 101.0576, 101.0632, 101.0644, 101.0666, 101.0661,
        101.0658, 101.0657 ];

    private readonly double[] _secondEpochHeights = [ 101.6632, 101.8982, 101.7086, 101.5977,
        101.8059, 101.3904, 101.3390, 101.4209, 101.3824, 101.1665, 101.1449, 101.2822, 101.6822, 101.7562,
        101.8096, 101.1968, 101.0042, 101.0857, 101.0456, 101.0551, 101.0621, 101.0637, 101.0665, 101.0660,
        101.0704, 101.0703 ];

    private readonly double[] _firstEpochValues = [
        516.7363, 0,
        516.7388, 0,
        516.7337, 0,
        0.0051, 0.0025, 0];

    private readonly double[] _secondEpochValues = [
        516.73569, 0.00003,
        516.73824, 0.00003,
        516.73314, 0.00003,
        0.00510, 0.00255, 0.00058];

    private readonly double[] _firstEpochWithSmoothedValues = [
        516.73627, 0.00000, 516.73624, 0.00000,
        516.73882, 0.00000, 516.73879, 0.00000,
        516.73372, 0.00000, 516.73369, 0.00000,
        0.00510, 0.00255, 0.00000];

    private readonly double[] _secondEpochWithSmoothedValues = [
        516.73569, 0.00003, 516.73575, 0.00003,
        516.73824, 0.00003, 516.73829, 0.00003,
        516.73314, 0.00003, 516.73320, 0.00003,
        0.00510, 0.00255, 0.00058];

    private readonly double[] _predictedValues = [
        516.73597, 0.00002, 516.73597, 0.00002,
        516.73852, 0.00002, 516.73852, 0.00002,
        516.73342, 0.00002, 516.73342, 0.00002,
        0.00510, 0.00255, 0.00030];

    private double _accuracy = 0.0005;
    private double _smoothingFactor = 0.9;

    [TestCase(TestName = "Расчет первой эпохи.")]
    public void TestFirstEpochCreation()
    {
        var first = new BlockEpoch(_firstEpochHeights, _accuracy, _smoothingFactor);
        CheckValuesWithoutPredictions(first, _firstEpochValues, 4);
    }

    [TestCase(TestName = "Расчет второй эпохи.")]
    public void TestSecondEpochCreation()
    {
        var first = new BlockEpoch(_firstEpochHeights, _accuracy, _smoothingFactor);
        var second = new BlockEpoch(first, _secondEpochHeights, _accuracy, _smoothingFactor, 1);
        CheckValuesWithoutPredictions(second, _secondEpochValues, 5);
    }

    [TestCase(TestName = "Расчет прогнозной эпохи.")]
    public void TestPredictionEpochCreation()
    {
        var first = new BlockEpoch(_firstEpochHeights, _accuracy, _smoothingFactor);
        var second = new BlockEpoch(first, _secondEpochHeights, _accuracy, _smoothingFactor, 1);
        var epochs = new[] { first, second };
        var predicted = new BlockEpoch(epochs, _accuracy, _smoothingFactor);
        CheckValuesWithPredictions(predicted, _predictedValues, 5);
    }

    [TestCase(TestName = "Расчет значений для прогноза")]
    public void TestPredictionValuesCalculation()
    {
        var first = new BlockEpoch(_firstEpochHeights, _accuracy, _smoothingFactor);
        var second = new BlockEpoch(first, _secondEpochHeights, _accuracy, _smoothingFactor, 1);
        var epochs = new[] { first, second };
        var predicted = new BlockEpoch(epochs, _accuracy, _smoothingFactor);
        CheckValuesWithPredictions(first, _firstEpochWithSmoothedValues, 5);
        CheckValuesWithPredictions(second, _secondEpochWithSmoothedValues, 5);
    }

    private void CheckValuesWithoutPredictions(BlockEpoch block, double[] expected, int digits)
    {
        var actual = new double[]
        {
            block.M, block.A,
            block.MPlus, block.APlus,
            block.MMinus, block.AMinus,
            block.Epsilon, block.R, block.MDeviation
        };
        CheckValues(actual, expected, digits);
    }

    private void CheckValuesWithPredictions(BlockEpoch block, double[] expected, int digits)
    {
        var actual = new double[]
        {
            block.M, block.A, block.MSmoothed, block.ASmoothed,
            block.MPlus, block.APlus, block.MSmoothedPlus, block.ASmoothedPlus,
            block.MMinus, block.AMinus, block.MSmoothedMinus, block.ASmoothedMinus,
            block.Epsilon, block.R, block.MDeviation
        };
        CheckValues(actual, expected, digits);
    }

    private void CheckValues(double[] actual, double[] expected, int digits)
    {
        var actualRounded = actual.Select(x => Math.Round(x, digits));

        foreach (var (value, i) in actualRounded.Select((v, i) => (v, i)))
        {
            Assert.That(value, Is.EqualTo(expected[i]));
        }
    }
}