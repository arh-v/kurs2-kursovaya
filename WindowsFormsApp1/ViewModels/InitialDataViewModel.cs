using BuildingStatePredictionApp.Models;
using BuildingStatePredictionApp.Models.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;

namespace BuildingStatePredictionApp.ViewModels;

internal class InitialDataViewModel : BindableBase
{
    private InitialDataManager _model;
    private int _selectedTableIndex = -1;

    public BindingList<string> TablesNames { get; }

    public int SelectedTableIndex
    {
        get => _selectedTableIndex;
        set
        {
            if (SetProperty(ref _selectedTableIndex, value))
            {
                SelectedTableIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public double Accuracy
    {
        get => _model.Accuracy;
        set => _model.Accuracy = value;
    }

    public double SmoothingFactor
    {
        get => _model.SmoothingFactor;
        set => _model.SmoothingFactor = value;
    }

    public Image BuildingScheme => _model.BuildingScheme;

    public IListSource Heights => _model.Heights;

    public double[] MaxInitialValues => _model.MaxInitialValues;

    public double[] MinInitialValues => _model.MinInitialValues;

    public ICommand LoadDataBaseCommand { get; }

    public ICommand ApplyCommand { get; }

    public ICommand AddLineCommand { get; }

    public ICommand RemoveLineCommand { get; }

    public event EventHandler<EventArgs> OnDataBaseConnect;
    public event EventHandler<ApplyEventArgs> OnApply;
    public event EventHandler<EventArgs> OnAddLine;
    public event EventHandler<EventArgs> OnRemoveLine;
    public event EventHandler<EventArgs> SelectedTableIndexChanged;
    public event EventHandler<EventArgs> DataBaseConnected;

    public InitialDataViewModel(InitialDataManager model)
    {
        _model = model;
        LoadDataBaseCommand = new DelegateCommand(() => OnDataBaseConnect?.Invoke(this, EventArgs.Empty));
        ApplyCommand = new DelegateCommand(() => OnApply?.Invoke(this, new(_model.Initials.GetHeightsRowsValues())));
        AddLineCommand = new DelegateCommand(() => OnAddLine?.Invoke(this, EventArgs.Empty));
        RemoveLineCommand = new DelegateCommand(() => OnRemoveLine?.Invoke(this, EventArgs.Empty));
        _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
        TablesNames = [];
        TablesNames.Watch(_model.TablesNames, v => v);
        OnDataBaseConnect += ConnectDatabase;
        OnAddLine += (s, e) =>
        {
            if (_selectedTableIndex == -1) return;

            _model.AddLine(TablesNames[_selectedTableIndex]);
        };
        OnRemoveLine += (s, e) =>
        {
            if (_selectedTableIndex == -1) return;

            _model.RemoveLine(TablesNames[_selectedTableIndex]);
        };
        SelectedTableIndexChanged += (s, e) =>
        {
            if (_selectedTableIndex == -1) return;

            _model.LoadData(TablesNames[_selectedTableIndex]);
        };
    }

    private void ConnectDatabase(object sender, EventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Filter = "Текстовые файлы (*.sqlite)|*.sqlite|Все файлы (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK) return;

        _model.ConnectToDatabase(openFileDialog.FileName);
        SelectedTableIndex = 0;
        DataBaseConnected?.Invoke(this, EventArgs.Empty);
    }

    public class ApplyEventArgs(IEnumerable<IEnumerable<double>> heights) : EventArgs
    {
        public IEnumerable<IEnumerable<double>> Heights { get; } = heights;
    }
}