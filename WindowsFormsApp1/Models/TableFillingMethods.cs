using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BuildingStatePredictionApp.Models
{
//    abstract class TableFillingMethods
//    {
//        private static IReadOnlyList<string> SeriesPostfixes;

//        static TableFillingMethods()
//        {
//            SeriesPostfixes = new List<string>() { "", "+", "-", " Прогноз", "+ Прогноз", "- Прогноз" };
//        }
//        //----------------------------------- Таблицы --------------------------------------

//        /// <summary>
//        /// Метод заполнения таблицы расчетными значениями M и a
//        /// </summary>
//        /// <param name="table"></param>
//        /// <param name="block"></param>
//        public static void Fill_dop_table(DataGridView table, Block block)
//        {
//            FillTable(table, block.CalculatedValues, Block.AdditionalColumnNames);
//            table.Columns.Add("Состояние", "Состояние");
//            table[0, block.M.Count - 1].Value = "Прогноз";

//            for (var row = 0; row < block.M.Count; row++)
//            {
//                table[block.CalculatedValues.Count + 1, row].Value = block.Sost[row];
//                table[block.CalculatedValues.Count + 1, row].Style.BackColor = block.Sost[row] == "Нормальное" ? Color.Green :
//                    block.Sost[row] == "Аварийное" ? Color.Red : Color.Yellow;
//            }
//        }

//        /// <summary>
//        /// Метод заполнения таблицы для H
//        /// </summary>
//        /// <param name="table"></param>
//        /// <param name="block"></param>
//        public static void FillTable(DataGridView table, Block block)
//        {
//            var bigH = new List<List<double>>();

//            for (var point = 0; point < block.Heights.GetLongLength(0); point++)
//            {
//                var h = new List<double>();

//                for (var row = 0; row < block.Heights.GetLongLength(1); row++)
//                {
//                    h.Add(block.Heights[point, row]);
//                }
                
//                bigH.Add(h);
//            }

//            FillTable(table, bigH, block.col_names);
//        }

//        public static void FillTable(DataGridView table, IReadOnlyList<List<double>> H, List<string> colnames)
//        {
//            table.Columns.Clear();
//            table.Rows.Clear();

//            for (var col = 0; col < colnames.Count; col++)
//            {
//                table.Columns.Add(colnames[col], colnames[col]);
//                table.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
//                table.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;

//                for (var row = 0; row < H[0].Count; row++)
//                {

//                    if (col == 0)
//                    {
//                        table.Rows.Add();
//                        table[col, row].Value = row;
//                        continue;
//                    }

//                    table[col, row].Value = Math.Round(H[col-1][row], 4);
//                }
//            }
//        }

//        //----------------------------------- Графики --------------------------------------

//        public static void SetStyle(Chart chart)
//        {
//            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
//            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
//            var MaxPoints = chart.Series[0].Points.Count;
//            chart.ApplyPaletteColors();

//            foreach (var series in chart.Series)
//            {
//                series.ChartType = SeriesChartType.Spline;
//                series.Enabled = IsChartSeriesEnabled(series, chart);
//                series.BorderWidth = 2;
//                series.MarkerStyle = MarkerStyle.Circle;

//                for (var i = 0; i < series.Points.Count; i++)
//                {
//                    if (series.Name.Contains("Прогноз"))
//                    {
//                        series.Points[1].Label = MaxPoints.ToString();
//                    }
//                    else
//                    {
//                        series.Points[i].Label = i.ToString();
//                    }
                        
//                    series.Points[i].LabelForeColor = series.Color;
//                }
//            }
//        }

//        public static bool IsChartSeriesEnabled(Series s, Chart ch)
//        {
//            Control control = ch.Parent;

//            foreach (GroupBox gb in control.Controls.OfType<GroupBox>())
//                foreach (CheckBox cb in gb.Controls.OfType<CheckBox>())
//                    if (s.Name == cb.Text)
//                        return cb.Checked;

//            return true;
//        }

//        /// <summary>
//        /// Метод построения графика M(t)
//        /// </summary>
//        public static void ChartMt(Chart chart, Block block)
//        {
//            chart.Series.Clear();
//            chart.ChartAreas[0].AxisX.Title = "t";
//            chart.ChartAreas[0].AxisY.Title = "M";
//            chart.ChartAreas[0].AxisX.TitleFont = new Font("Microsoft Sans Serif", 14f);
//            chart.ChartAreas[0].AxisY.TitleFont = new Font("Microsoft Sans Serif", 14f);

//            foreach (string postfix in SeriesPostfixes)
//            {
//                chart.Series.Add(new Series("M(t)" + postfix));
//            }

//            chart.ChartAreas[0].AxisY.LabelStyle.Format = "{0.0000}";

//            for (int i = 0; i < block.M.Count - 1; i++)
//            {
//                chart.Series["M(t)"].Points.AddXY(i, block.M[i]);
//                chart.Series["M(t)+"].Points.AddXY(i, block.M_plus[i]);
//                chart.Series["M(t)-"].Points.AddXY(i, block.M_minus[i]);
//            }

//            //Прогнозы
//            for (int i = block.M.Count - 2; i < block.M.Count; i++)
//            {
//                chart.Series["M(t) Прогноз"].Points.AddXY(i, block.M[i]);
//                chart.Series["M(t)+ Прогноз"].Points.AddXY(i, block.M_plus[i]);
//                chart.Series["M(t)- Прогноз"].Points.AddXY(i, block.M_minus[i]);
//            }

//            SetStyle(chart);
//            chart.ChartAreas[0].AxisY.Maximum = block.M_plus.Max() * 2 - block.M_minus.Min() / block.M.Count;
//            chart.ChartAreas[0].AxisY.Minimum = block.M_minus.Min() * 2 - block.M_plus.Max() / block.M.Count;
//            chart.ChartAreas[0].AxisX.Minimum = 0;
//            chart.ChartAreas[0].AxisX.Maximum = block.M.Count;
//        }

//        /// <summary>
//        /// Метод построения графика a(M)
//        /// </summary>
//        public static void BuildSmallAFromBigM(Chart chart, Block block)
//        {
//            chart.Series.Clear();

//            foreach (string postfix in SeriesPostfixes)
//            {
//                chart.Series.Add(new Series("a(M)" + postfix));
//            }

//            chart.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.0000,}";
//            chart.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.0000,}";
//            chart.ChartAreas[0].AxisX.Title = "M";
//            chart.ChartAreas[0].AxisY.Title = "a (в секундах)";
//            chart.ChartAreas[0].AxisX.TitleFont = new Font("Microsoft Sans Serif", 14f);
//            chart.ChartAreas[0].AxisY.TitleFont = new Font("Microsoft Sans Serif", 14f);

//            for (int i = 0; i < block.M.Count - 1; i++)
//            {
//                chart.Series["a(M)"].Points.AddXY(block.M[i], block.a[i]);
//                chart.Series["a(M)+"].Points.AddXY(block.M_plus[i], block.a_plus[i]);
//                chart.Series["a(M)-"].Points.AddXY(block.M_minus[i], block.a_minus[i]);
//            }

//            for (int i = block.M.Count - 2; i < block.M.Count; i++)
//            {
//                chart.Series["a(M) Прогноз"].Points.AddXY(block.M[i], block.a[i]);
//                chart.Series["a(M)+ Прогноз"].Points.AddXY(block.M_plus[i], block.a_plus[i]);
//                chart.Series["a(M)- Прогноз"].Points.AddXY(block.M_minus[i], block.a_minus[i]);
//            }

//            SetStyle(chart);
//        }

//        public static List<List<double>> Level4ChartFill(Chart chart, double[,] bigH, List<string> colnames)
//        {
//            chart.Series.Clear();
//            chart.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.0000,}";
//            chart.ChartAreas[0].AxisX.Title = "t";
//            chart.ChartAreas[0].AxisY.Title = "H";
//            chart.ChartAreas[0].AxisX.TitleFont = new Font("Microsoft Sans Serif", 14f);
//            chart.ChartAreas[0].AxisY.TitleFont = new Font("Microsoft Sans Serif", 14f);
//            var predictions = new List<List<double>>();
//            double h_max = 0;
//            double h_min = 0;

//            for (int point = 0; point < colnames.Count - 1; point++)
//            {
//                chart.Series.Add(new Series($"H{colnames[point + 1]}(t)"));
//                chart.Series.Add(new Series($"H{colnames[point + 1]}(t) Прогноз"));
//                var pr = new List<double>();

//                for (int row = 0; row < bigH.GetLongLength(1); row++)
//                {
//                    chart.Series[$"H{colnames[point + 1]}(t)"].Points.AddXY(row, bigH[point, row]);
//                    pr.Add(bigH[point, row]);
//                }

//                Block.GetPredictions(pr);
//                predictions.Add(pr);
//                chart.Series[$"H{colnames[point + 1]}(t) Прогноз"].Points.AddXY(bigH.GetLongLength(1) - 1, pr[pr.Count - 2]);
//                chart.Series[$"H{colnames[point + 1]}(t) Прогноз"].Points.AddXY(bigH.GetLongLength(1), pr[pr.Count - 1]);

//                if (h_max < pr.Max() || point == 0)
//                {
//                    h_max = pr.Max();
//                }
                
//                if (h_min > pr.Min() || point == 0)
//                {
//                    h_min = pr.Min();
//                }
//            }

//            SetStyle(chart);
//            chart.ChartAreas[0].AxisY.Minimum = h_min - (h_max - h_min == 0 ? 1 : h_max - h_min);
//            chart.ChartAreas[0].AxisY.Maximum = h_max + (h_max - h_min == 0 ? 1 : h_max - h_min);
//            return predictions;
//        }
//    }
}
