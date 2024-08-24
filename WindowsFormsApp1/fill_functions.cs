using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    abstract class fill_functions
    {
        //----------------------------------- Таблицы --------------------------------------

        /// <summary>
        /// Метод заполнения таблицы расчетными значениями M и a
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="block"></param>
        public static void Fill_dop_table(DataGridView dgv, Block block)
        {
            Fill_table(dgv, block.Rasch_zn, Block.dop_col_names);
            dgv.Columns.Add("Состояние", "Состояние");
            dgv[0, block.M.Count - 1].Value = "Прогноз";
            for (int row = 0; row < block.M.Count; row++)
            {
                dgv[block.Rasch_zn.Count + 1, row].Value = block.Sost[row];
                dgv[block.Rasch_zn.Count + 1, row].Style.BackColor = block.Sost[row] == "Нормальное" ? Color.Green : block.Sost[row] == "Аварийное" ? Color.Red : Color.Yellow;
            }
        }

        /// <summary>
        /// Метод заполнения таблицы для H
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="block"></param>
        public static void Fill_table(DataGridView dgv, Block block)
        {
            List<List<double>> H = new List<List<double>>();
            List<double> h;
            for (int point = 0; point < block.Heights.GetLongLength(0); point++)
            {
                h = new List<double>();
                for (int row = 0; row < block.Heights.GetLongLength(1); row++)
                    h.Add(block.Heights[point, row]);
                H.Add(h);
            }

            Fill_table(dgv, H, block.col_names);
        }

        public static void Fill_table(DataGridView dgv, List<List<double>> H, List<string> colnames)
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            for (int col = 0; col < colnames.Count; col++)
            {
                dgv.Columns.Add(colnames[col], colnames[col]);
                dgv.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgv.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;

                for (int row = 0; row < H[0].Count; row++)
                {

                    if (col == 0)
                    {
                        dgv.Rows.Add();
                        dgv[col, row].Value = row;
                    }

                    else
                        dgv[col, row].Value = Math.Round(H[col-1][row], 4);
                }
            }
        }

        //----------------------------------- Графики --------------------------------------

        public static void Chart_set_style(Chart chart)
        {
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            int MaxPoints = chart.Series[0].Points.Count;

            chart.ApplyPaletteColors();
            foreach (Series s in chart.Series)
            {
                chart.Series[s.Name].ChartType = (SeriesChartType)4;
                chart.Series[s.Name].Enabled = ChartSerieEnable(chart.Series[s.Name], chart);
                chart.Series[s.Name].BorderWidth = 2;
                chart.Series[s.Name].MarkerStyle = (MarkerStyle)2;
                for (int i = 0; i < s.Points.Count; i++)
                {
                    if (s.Name.Contains("Прогноз"))
                        s.Points[1].Label = MaxPoints.ToString();
                    else
                        s.Points[i].Label = i.ToString();
                    s.Points[i].LabelForeColor = s.Color;
                }
            }
        }

        public static bool ChartSerieEnable(Series s, Chart ch)
        {
            Control control = ch.Parent;

            foreach (GroupBox gb in control.Controls.OfType<GroupBox>())
                foreach (CheckBox cb in gb.Controls.OfType<CheckBox>())
                    if (s.Name == cb.Text)
                        return cb.Checked;
            return true;
        }

        /// <summary>
        /// Метод построения графика M(t)
        /// </summary>
        public static void Chart_Mt(Chart chart, Block block)
        {
            chart.Series.Clear();

            chart.ChartAreas[0].AxisX.Title = "t";
            chart.ChartAreas[0].AxisY.Title = "M";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Microsoft Sans Serif", 14f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Microsoft Sans Serif", 14f);

            foreach (string s in new List<string> { "", "+", "-", " Прогноз", "+ Прогноз", "- Прогноз" })
                chart.Series.Add(new Series("M(t)" + s));

            chart.ChartAreas[0].AxisY.LabelStyle.Format = "{0.0000}";

            for (int i = 0; i < block.M.Count - 1; i++)
            {
                chart.Series["M(t)"].Points.AddXY(i, block.M[i]);
                chart.Series["M(t)+"].Points.AddXY(i, block.M_plus[i]);
                chart.Series["M(t)-"].Points.AddXY(i, block.M_minus[i]);
            }

            //Прогнозы
            for (int i = block.M.Count - 2; i < block.M.Count; i++)
            {
                chart.Series["M(t) Прогноз"].Points.AddXY(i, block.M[i]);
                chart.Series["M(t)+ Прогноз"].Points.AddXY(i, block.M_plus[i]);
                chart.Series["M(t)- Прогноз"].Points.AddXY(i, block.M_minus[i]);
            }

            Chart_set_style(chart);

            chart.ChartAreas[0].AxisY.Maximum = block.M_plus.Max() + (block.M_plus.Max() - block.M_minus.Min()) / block.M.Count;
            chart.ChartAreas[0].AxisY.Minimum = block.M_minus.Min() - (block.M_plus.Max() - block.M_minus.Min()) / block.M.Count;
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = block.M.Count;
        }

        /// <summary>
        /// Метод построения графика a(M)
        /// </summary>
        public static void Chart_AM(Chart chart, Block block)
        {

            chart.Series.Clear();

            foreach (string s in new List<string> { "", "+", "-", " Прогноз", "+ Прогноз", "- Прогноз" })
                chart.Series.Add(new Series("a(M)" + s));

            chart.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.0000,}";
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.0000,}";
            chart.ChartAreas[0].AxisX.Title = "M";
            chart.ChartAreas[0].AxisY.Title = "a (в секундах)";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Microsoft Sans Serif", 14f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Microsoft Sans Serif", 14f);
            for (int i = 0; i < block.M.Count - 1; i++)
            {
                //M
                chart.Series["a(M)"].Points.AddXY(block.M[i], block.a[i]);
                //M+
                chart.Series["a(M)+"].Points.AddXY(block.M_plus[i], block.a_plus[i]);
                //M-
                chart.Series["a(M)-"].Points.AddXY(block.M_minus[i], block.a_minus[i]);
            }

            //Прогнозы
            for (int i = block.M.Count - 2; i < block.M.Count; i++)
            {
                chart.Series["a(M) Прогноз"].Points.AddXY(block.M[i], block.a[i]);
                chart.Series["a(M)+ Прогноз"].Points.AddXY(block.M_plus[i], block.a_plus[i]);
                chart.Series["a(M)- Прогноз"].Points.AddXY(block.M_minus[i], block.a_minus[i]);
            }

            Chart_set_style(chart);
        }

        public static List<List<double>> Lvl4_chart_fill(Chart chart, double[,] H, List<string> colnames)
        {
            chart.Series.Clear();

            chart.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.0000,}";
            chart.ChartAreas[0].AxisX.Title = "t";
            chart.ChartAreas[0].AxisY.Title = "H";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Microsoft Sans Serif", 14f);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Microsoft Sans Serif", 14f);

            List<List<double>> Progn = new List<List<double>>();
            List<double> pr;
            double h_max = 0;
            double h_min = 0;
            for (int point = 0; point < colnames.Count - 1; point++)
            {
                chart.Series.Add(new Series($"H{colnames[point + 1]}(t)"));
                chart.Series.Add(new Series($"H{colnames[point + 1]}(t) Прогноз"));
                pr = new List<double>();
                for (int row = 0; row < H.GetLongLength(1); row++)
                {
                    chart.Series[$"H{colnames[point + 1]}(t)"].Points.AddXY(row, H[point, row]);
                    pr.Add(H[point, row]);
                }

                Block.Prognoz(pr);
                Progn.Add(pr);
                chart.Series[$"H{colnames[point + 1]}(t) Прогноз"].Points.AddXY(H.GetLongLength(1) - 1, pr[pr.Count - 2]);
                chart.Series[$"H{colnames[point + 1]}(t) Прогноз"].Points.AddXY(H.GetLongLength(1), pr[pr.Count - 1]);

                if (point == 0)
                {
                    h_max = pr.Max();
                    h_min = pr.Min();
                }
                else
                {
                    if (h_max < pr.Max()) h_max = pr.Max();
                    if (h_min > pr.Min()) h_min = pr.Min();
                }

            }


            Chart_set_style(chart);

            chart.ChartAreas[0].AxisY.Minimum = h_min - ((h_max - h_min) == 0 ? 1 : (h_max - h_min));
            chart.ChartAreas[0].AxisY.Maximum = h_max + ((h_max - h_min) == 0 ? 1 : (h_max - h_min));

            return Progn;
        }
    }
}
