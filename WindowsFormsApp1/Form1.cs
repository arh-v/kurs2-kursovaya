using BuildingStatePredictionApp.ViewModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace BuildingStatePredictionApp;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        var mainViewModel = new MainViewModel();
        DataContext = mainViewModel;

        // Initial Data
        ConnectDbButton.DataBindings.Add(new("Command", DataContext, "InitialDataVm.LoadDataBaseCommand", true));
        TablesComboBox.DataBindings.Add(new("DataSource", DataContext, "InitialDataVm.TablesNames", true));
        TablesComboBox.DataBindings.Add(new("SelectedIndex", DataContext, "InitialDataVm.SelectedTableIndex", true));
        AccuracyTextBox.DataBindings.Add(new("Text", DataContext, "InitialDataVm.Accuracy", true, DataSourceUpdateMode.OnPropertyChanged));
        SmoothingFactorTextBox.DataBindings.Add(new("Text", DataContext, "InitialDataVm.SmoothingFactor", true, DataSourceUpdateMode.OnPropertyChanged));
        ApplyButton.DataBindings.Add(new("Command", DataContext, "InitialDataVm.ApplyCommand", true));
        AddLineButton.DataBindings.Add(new("Command", DataContext, "InitialDataVm.AddLineCommand", true));
        RemoveLineButton.DataBindings.Add(new("Command", DataContext, "InitialDataVm.RemoveLineCommand", true));
        InitialDataGridView.DataBindings.Add(new("DataSource", DataContext, "InitialDataVm.Heights", true));
        InitialPictureBox.DataBindings.Add(new("Image", DataContext, "InitialDataVm.BuildingScheme", true));

        // Level 1

        LevelOneBlockCalculationsDataGridView.DataBindings.Add(new("DataSource", DataContext, "LevelOneVm.Calculations", true));
        LevelOneBlockCalculationsDataGridView.CellFormatting += FormatSmallNumbers;
        LevelOneBlockCalculationsDataGridView.CellFormatting += FormatStateCellsColor;
        Setup(LevelOneAmChart, DataContext, "LevelOneVm.AmPlot.Series");
        Setup(LevelOneMtChart, DataContext, "LevelOneVm.MtPlot.Series");

        // Level 2

        LevelTwoAddBlockButton.DataBindings.Add("Command", DataContext, "LevelTwoVm.AddBlockCommand", true);
        LevelTwoPictureBox.DataBindings.Add(new("Image", DataContext, "LevelTwoVm.BuildingScheme", true));
        Setup(LevelTwoAmChart, DataContext, "LevelTwoVm.AmPlot.Series");
        Setup(LevelTwoMtChart, DataContext, "LevelTwoVm.MtPlot.Series");
        LevelTwoPointsListBox.DataBindings.Add(new("DataSource", DataContext, "LevelTwoVm.Points"));
        LevelTwoPointsListBox.SelectedIndexChanged += (s, e) =>
        {
            mainViewModel.LevelTwoVm.SelectedPoints.Clear();

            foreach (var point in LevelTwoPointsListBox.SelectedItems)
            {
                mainViewModel.LevelTwoVm.SelectedPoints.Add((int)point);
            }
        };
        LevelTwoBlocksComboBox.DataBindings.Add(new("DataSource", DataContext, "LevelTwoVm.BlocksNames"));
        LevelTwoBlocksComboBox.DataBindings.Add(new("SelectedIndex", DataContext, "LevelTwoVm.SelectedBlockIndex"));
        LevelTwoBlockDataGridView.DataBindings.Add(new("DataSource", DataContext, "LevelTwoVm.BlockData"));
        LevelTwoBlockCalculationsDataGridView.DataBindings.Add(new("DataSource", DataContext, "LevelTwoVm.Calculations"));
        LevelTwoBlockCalculationsDataGridView.CellFormatting += FormatSmallNumbers;
        LevelTwoBlockCalculationsDataGridView.CellFormatting += FormatStateCellsColor;

        // Level 3

        LevelThreeBlocksComboBox.DataBindings.Add(new("DataSource", DataContext, "LevelThreeVm.BlocksNames", true));
        LevelThreeBlocksComboBox.DataBindings.Add(new("SelectedIndex", DataContext, "LevelThreeVm.SelectedBlockIndex"));
        LevelThreePictureBox.DataBindings.Add(new("Image", DataContext, "LevelThreeVm.BuildingScheme", true));
        LevelThreeApplyBlockButton.DataBindings.Add("Command", DataContext, "LevelThreeVm.ShowBlockLinks", true);
        LevelThreeBlockPointsListBox.DataBindings.Add(new("DataSource", DataContext, "LevelThreeVm.Points"));
        LevelThreeBlockPointsListBox.SelectedIndexChanged += (s, e) =>
        {
            mainViewModel.LevelThreeVm.SelectedPoints.Clear();

            foreach (var point in LevelThreeBlockPointsListBox.SelectedItems)
            {
                mainViewModel.LevelThreeVm.SelectedPoints.Add((int)point);
            }
        };
        LevelThreeApplySubBlockButton.DataBindings.Add(new("Command", DataContext, "LevelThreeVm.CalculateSubBlock", true));
        LevelThreeLinksDataGridView.DataBindings.Add(new("DataSource", DataContext, "LevelThreeVm.LinksLengths", true));
        LevelThreeLinksDataGridView.CellFormatting += FormatSmallNumbers;
        LevelThreeLinksStateDataGridView.DataBindings.Add(new("DataSource", DataContext, "LevelThreeVm.LinksDeltas", true));
        LevelThreeLinksStateDataGridView.CellFormatting += FormatSmallNumbers;
        LevelThreeLinksStateDataGridView.CellFormatting += FormatStateCellsColor;
        LevelThreeSubBlockDataGridView.DataBindings.Add(new("DataSource", DataContext, "LevelThreeVm.SubBlockHeigths"));
        LevelThreeSubBlockCalculationsDataGridView.DataBindings.Add(new("DataSource", DataContext, "LevelThreeVm.Calculations"));
        LevelThreeSubBlockCalculationsDataGridView.CellFormatting += FormatSmallNumbers;
        LevelThreeSubBlockCalculationsDataGridView.CellFormatting += FormatStateCellsColor;
        Setup(LevelThreeAmChart, DataContext, "LevelThreeVm.AmPlot.Series");
        Setup(LevelThreeMtChart, DataContext, "LevelThreeVm.MtPlot.Series");

        // Level 4

        LevelFourPictureBox.DataBindings.Add(new("Image", DataContext, "LevelFourVm.BuildingScheme", true));
        LevelFourPointsListBox.DataBindings.Add(new("DataSource", DataContext, "LevelFourVm.Points"));
        LevelFourPointsListBox.SelectedIndexChanged += (s, e) =>
        {
            mainViewModel.LevelFourVm.SelectedPoints.Clear();

            foreach (var point in LevelFourPointsListBox.SelectedItems)
            {
                mainViewModel.LevelFourVm.SelectedPoints.Add((int)point);
            }
        };
        LevelFourApplyButton.DataBindings.Add(new("Command", DataContext, "LevelFourVm.ApplyCommand", true));
        Setup(LevelFourHtChart, DataContext, "LevelFourVm.HtPlot.Series");
        LevelFourDataGridView.DataBindings.Add(new("DataSource", DataContext, "LevelFourVm.Heights"));
    }

    private static void Setup(Chart c, object dataSource, string dataMember)
    {
        var splitted = dataMember.Split('.');
        var vm = dataSource.GetType().GetProperty(splitted[0]).GetValue(dataSource);
        c.ChartAreas[0].AxisX.TitleFont = c.ChartAreas[0].AxisY.TitleFont = new Font("Microsoft Sans Serif", 14f);
        c.ChartAreas[0].AxisY.IsStartedFromZero = false;

        if (vm is not BindableBase bindableVm) return;

        AddSeries(c, vm, dataSource, dataMember, splitted[1]);

        bindableVm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != splitted[1]) return;

            c.Series.Clear();
            AddSeries(c, vm, dataSource, dataMember, e.PropertyName);
            c.ChartAreas[0].RecalculateAxesScale();
        };
    }

    private static void AddSeries(Chart c, object vm, object dataSource, string dataMember, string managerName)
    {
        var chartProperty = vm.GetType().GetProperty(managerName).GetValue(vm);

        if (chartProperty == null) return;

        if (chartProperty is not ChartViewModel cvm) return;

        var (ordinate, absciss) = cvm.GetCoordNames();
        c.ChartAreas[0].AxisX.Title = absciss;
        c.ChartAreas[0].AxisY.Title = ordinate;
        c.ChartAreas[0].AxisX.LabelStyle.Format = absciss == "t" ? "{0}" : "N5";
        c.ChartAreas[0].AxisY.LabelStyle.Format = ordinate == "a" ? "E5" : "N5";

        foreach (var name in cvm.GetSeriesNames())
        {
            c.Series.Add(name).Setup(dataSource, dataMember);
        }

        cvm.SeriesNamesChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add) c.Series.Add(e.NewSerie).Setup(dataSource, dataMember);

            if (e.Action == NotifyCollectionChangedAction.Reset) c.Series.Clear();
        };
    }

    private static void FormatSmallNumbers(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.Value is not double num) return;

        if (num >= 0.0001 || Math.Abs(num) < 1e-16) return;

        e.CellStyle.Format = "E4";
    }

    private static void FormatStateCellsColor(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView table) return;

        if (table.Columns[e.ColumnIndex].HeaderText == "Состояние")
        {
            e.CellStyle.BackColor = e.Value.ToString() == "Аварийное" ? Color.Red :
                e.Value.ToString() == "Нормальное" ? Color.Green : Color.Yellow;
        }

        if (table.Rows.Count != e.RowIndex + 1 ||
            table.Rows[e.RowIndex].Cells[0].Value.ToString() != "Состояние" ||
            e.Value.ToString() == "Состояние") return;

        e.CellStyle.BackColor = e.Value.ToString() == "Не жесткая" ? Color.Red : Color.Green;
    }

    private void chartCheckBox_Checked(object sender, EventArgs e)
    {
        var cb = sender as CheckBox;

        if (cb.Enabled == false) return;

        var control = cb.Parent.Parent;

        foreach (var ch in control.Controls.OfType<Chart>())
        {
            ch.Series[cb.Text].Enabled = cb.Checked;
        }
    }
    private void Form1_Resize(object sender, EventArgs e)
    {
        tabControl1.Height = this.Height - statusStrip1.Height - 37;
    }

    private void ComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        (sender as Control).Enabled = false;
        (sender as Control).Enabled = true;
    }
}

public static class SeriesExtensions
{
    public static void Setup(this Series s, object dataSource, string dataMember)
    {
        s.XValueMember = "item1";
        s.YValueMembers = "item2";
        s.ChartType = SeriesChartType.Spline;
        s.BorderWidth = 2;
        s.MarkerStyle = MarkerStyle.Circle;
        s.Bind(dataSource, dataMember);
    }

    public static void Bind(this Series s, object dataSource, string dataMember)
    {
        var membersNames = dataMember.Split('.');
        s.BindRecursive(dataSource, membersNames);
    }

    public static bool BindRecursive(this Series series, object dataSource, string[] membersNames, int current = 0)
    {
        if (dataSource == null) return false;

        if (current >= membersNames.Length)
        {
            if (dataSource is not IReadOnlyDictionary<string, BindingList<Tuple<double, double>>> collection)
            {
                return false;
            }

            if (!collection.ContainsKey(series.Name)) return false;

            series.Points.DataBind(collection[series.Name], "item1", "item2", "");
            return true;
        }

        var property = dataSource.GetType().GetProperty(membersNames[current]);

        if (dataSource is BindableBase bb)
        {
            PropertyChangedEventHandler d = null;
            d = (s, e) =>
            {
                if (e.PropertyName != property.Name) return;

                var newDataSource = property.GetValue(s);
                if (!series.BindRecursive(newDataSource, membersNames, current + 1))
                {
                    bb.PropertyChanged -= d;
                }
            };
            bb.PropertyChanged += d;
        }

        var newDataSource = property.GetValue(dataSource);
        
        return series.BindRecursive(newDataSource, membersNames, current + 1); ;
    }
}