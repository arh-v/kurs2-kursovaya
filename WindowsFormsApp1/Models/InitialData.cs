using Prism.Mvvm;
using System.Drawing;

namespace BuildingStatePredictionApp.Models;

public class InitialData : BindableBase, IReadonlyInitialData
{
    private double _accuracy;
    private double _smoothingFactor = 0.9;

    private Image _buildingScheme;

    public double Accuracy
    {
        get => _accuracy;
        set => SetProperty(ref _accuracy, value);
    }

    public double SmoothingFactor
    {
        get => _smoothingFactor;
        set => SetProperty(ref _smoothingFactor, value);
    }

    public Image BuildingScheme
    {
        get => _buildingScheme;
        set => SetProperty(ref _buildingScheme, value);
    }

    public HeightsTableManager Heights { get; } = new();

    IReadOnlyHeightsTableManager IReadonlyInitialData.Heights => Heights;
}

public interface IReadonlyInitialData
{
    public double Accuracy { get; }

    public double SmoothingFactor { get; }

    public Image BuildingScheme { get; }

    public IReadOnlyHeightsTableManager Heights { get; }
}