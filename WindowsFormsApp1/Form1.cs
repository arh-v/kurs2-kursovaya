using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SQLite;

using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

using static WindowsFormsApp1.fill_functions;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private SQLiteConnection SQLiteConn;
        private DataTable dTable;

        private static List<GroupBox> GroupBoxes_list;
        private static List<SplitContainer> SplitContainer_list;
        private static List<DataGridView> dataGridView_list;

        string path;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SQLiteConn = new SQLiteConnection();
            GroupBoxes_list = new List<GroupBox>() { groupBox1, groupBox2, groupBox3, groupBox4, groupBox5, groupBox6};

            SplitContainer_list = new List<SplitContainer>() { splitContainer1, splitContainer2, splitContainer3, splitContainer4, splitContainer5, splitContainer6, splitContainer7, splitContainer8, splitContainer9, splitContainer10, splitContainer11, splitContainer12, splitContainer13 };
            dataGridView_list = new List<DataGridView>() { dataGridView2, dataGridView3, dataGridView4, dataGridView5, dataGridView_lvl3_1, dataGridView_lvl3_2, dataGridView_lvl3_3, dataGridView_lvl3_4 };
            Disable_Controls();
            foreach(DataGridView dgv in dataGridView_list)
                dgv.AllowUserToAddRows = false;
            ComboBox_setDefault(comboBox1);
            tabControl1.Height = this.Height - statusStrip1.Height - 37;
            pictureBox1.Left = this.Width / 2 - pictureBox1.Width / 2;
        }

        private void data_controls_enabled(bool en)
        {
            textBox1.Enabled = en;
            textBox2.Enabled = en;
            button2.Enabled = en;
            button3.Enabled = en;
            button4.Enabled = en;
        }

        private void Clear_images()
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;
        }

        private void Disable_Controls()
        {
            data_controls_enabled(false);

            foreach (GroupBox gb in GroupBoxes_list)
            {
                gb.Enabled = false;
                foreach (CheckBox cb in gb.Controls)
                    cb.Checked = true;
            }

            foreach (SplitContainer sc in SplitContainer_list)
                SplitContainer_setDefault(sc);

            foreach (DataGridView dgv in dataGridView_list)
            {
                dgv.Columns.Clear();
                dgv.Rows.Clear();
            }

            dataGridView1.DataSource = null;

            chart1.Series.Clear();
            chart2.Series.Clear();
            chart3.Series.Clear();
            chart4.Series.Clear();
            chart5.Series.Clear();
            chart6.Series.Clear();
            chart_lvl4.Series.Clear();

            listBox_lvl3_2.Items.Clear();

            bc = 1039;
            ComboBox_setDefault(comboBox3);
            ComboBox_setDefault(comboBox2);
        }

        private void ComboBox_setDefault(ComboBox cb)
        {
            cb.Enabled = false;
            cb.SelectedIndex = -1;
            cb.Items.Clear();
        }

        private void SplitContainer_setDefault(SplitContainer sc)
        {
            if (sc.Orientation == Orientation.Vertical)
                sc.SplitterDistance = sc.Width / 2;
            else
                sc.SplitterDistance = sc.Height / 2;
        }

        private void splitContainer_DoubleClick(object sender, EventArgs e)
        {
            var sc = sender as SplitContainer;
            SplitContainer_setDefault(sc);
        }

        private bool OpenDBFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "Текстовые файлы (*.sqlite)|*.sqlite|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                SQLiteConn = new SQLiteConnection("Data Source=" + openFileDialog.FileName + ";Version=3;");
                SQLiteConn.Open();
                SQLiteCommand command = new SQLiteCommand(SQLiteConn);

                // Путь к файлу
                path = "";
                List<string> path_list = new List<string>(SQLiteConn.FileName.Split('\\'));
                path_list.RemoveAt(path_list.Count - 1);
                path_list.ForEach(delegate(string s) { path += s + '\\'; });

                return true;
            }
            return false;
        }

        private bool GetTableNames()
        {
            string SQLQuery = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
            // запрос
            SQLiteCommand command = new SQLiteCommand(SQLQuery, SQLiteConn);
            // выполнение запроса и получение списка
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                comboBox1.Items.Add(reader[0].ToString());
            if (comboBox1.Items.Count == 0)
                return false;
            comboBox1.Enabled = true;
            comboBox1.SelectedIndex = 0;

            return true;
        }

        private bool dTable_check(DataTable dt)
        {
            if (dt.Columns.Count < 4 || dt.Columns[0].ColumnName != "Эпоха" || dt.Rows.Count < 2)
                return false;
            for (int row = 0; row < dt.Rows.Count; row++)
                if (dt.Rows[row].Field<string>("Эпоха") != row.ToString())
                    return false;
            try
            {
                for (int col = 1; col < dt.Columns.Count; col++)
                    for (int row = 0; row < dt.Rows.Count; row++)
                        dt.Rows[row][col] = Convert.ToDouble(dt.Rows[row].Field<string>(col).Replace('.',','));
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ф-ция для отображения таблицы данных dTable в компоненте dataGridView1
        /// </summary>
        /// <param name="SQLQuery">Строка SQL-запроса для составления таблицы</param>
        private bool ShowTableSQL(string SQLQuery)
        {
            dTable = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(SQLQuery, SQLiteConn);
            adapter.Fill(dTable);

            //Проверка данных в dTable
            if (!dTable_check(dTable))
            {
                dataGridView1.DataSource = null;
                return false;
            }

            // заполнение dataGridView1
            dataGridView1.DataSource = dTable;

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            return true;
        }

        private void OpenTXT(string f_path, string text, TextBox tb)
        {
            if (File.Exists(f_path))
            {
                string E;
                StreamReader sr = new StreamReader(f_path, Encoding.Default);

                string line;

                // Пока не конец файла
                while ((line = sr.ReadLine()) != null)
                {
                    E = "";
                    if (line.Contains(text) && line.Split(':').Count() == 2)
                    {
                        foreach (char ch in line.Split(':')[1])
                            E = (char.IsDigit(ch) || ch == '.' || ch == ',') ? E + ch : E + "";
                        if (double.TryParse(E.Replace('.', ','), out double result))
                        {
                            tb.Text = result.ToString();
                            break;
                        }
                    }
                }
                //Закрытие файла
                sr.Close();
            }
            else FileNotExists(f_path);
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Disable_Controls();
            if (!comboBox1.Enabled)
                return;
            if (ShowTableSQL(SQL_AllTable()))
                data_controls_enabled(true);
            else
                toolStripStatusLabel1.Text = "Выбранная таблица имеет неверные данные!";
        }

        // подключить БД
        private void button1_Click(object sender, EventArgs e)
        {
            Disable_Controls();
            Clear_images();
            ComboBox_setDefault(comboBox1);
            SQLiteConn.Dispose();
            if (OpenDBFile() && GetTableNames())
            {
                if (File.Exists(path + "Схема объекта.png"))
                {
                    try
                    {
                        pictureBox1.Load(path + "Схема объекта.png");
                        pictureBox2.Load(path + "Схема объекта.png");
                        pictureBox3.Load(path + "Схема объекта.png");
                        pictureBox4.Load(path + "Схема объекта.png");
                    }
                    catch { toolStripStatusLabel1.Text = "Не удалось загрузить схему объекта."; }
                }
                else FileNotExists(path + "Схема объекта.png");

                OpenTXT(path + "Описание объекта.txt", "Точность измерений:", textBox1);
            }
            else toolStripStatusLabel1.Text = "Выбранная БД имеет неверные данные!";
        }

        /// <summary>
        /// Ф-ция составления запроса для вывода всей таблицы из БД
        /// </summary>
        /// <returns>Строка SQL-запроса</returns>
        private string SQL_AllTable()
        {
            string query = "";

            // чтение первой строки
            SQLiteCommand command = new SQLiteCommand(SQLiteConn);
            command.CommandText = "select * from [" + comboBox1.SelectedItem + "]";
            try
            {
                SQLiteDataReader reader = command.ExecuteReader();
                reader.Read();

                //составление запроса
                query += "SELECT ";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    query += SQL_OneColumn(reader.GetName(i));
                    if (i != reader.FieldCount - 1)
                        query += ", ";
                }

                query += "FROM [" + comboBox1.SelectedItem + "]";
            }
            catch { }

            return query;
        }
        private string SQL_OneColumn(string colName)
        {
            return "replace(\"" + colName + "\",',','.') as \"" + colName + "\"";
        }

        /// <summary>
        /// ф-ция для проверки правильности ввода значений в TextBox
        /// </summary>
        /// <returns></returns>
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text.Contains('.'))
            {
                tb.Text = tb.Text.Replace('.', ',');
                tb.SelectionStart = tb.Text.Length;
            }

            if (double.TryParse(tb.Text, out double r_double) && r_double >= 0)
                if (tb == textBox2)
                    checkBox2.Checked = r_double <=1? true:false;
                else checkBox1.Checked = true;
            else if (tb == textBox1)
                checkBox1.Checked = false;
            else if (tb == textBox2)
                checkBox2.Checked = false;
        }

        private void FileNotExists(string filename)
        {
            toolStripStatusLabel1.Text = $"Файл \"{filename}\" не найден.";
        }

        private void chartCheckBox_Checked(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb.Enabled == false)
                return;
            
            Control control = cb.Parent.Parent;
            foreach (Chart ch in control.Controls.OfType<Chart>())
                ch.Series[cb.Text].Enabled = cb.Checked;
        }

        // ---------------------------------------------- Уровень 1 ---------------------------------------------------------

        Block lvl1_block;

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked && checkBox2.Checked && dTable.Rows.Count > 2)
            {
                Block.epsilon = Convert.ToDouble(textBox1.Text);
                Block.k = Convert.ToDouble(textBox2.Text);
                lvl1_block = new Block(dataGridView1);

                Chart_AM(chart1, lvl1_block);
                Chart_Mt(chart2, lvl1_block);

                groupBox1.Enabled = true;
                groupBox2.Enabled = true;

                Fill_dop_table(dataGridView2, lvl1_block);
                
                // Начало 2-го уровня
                listBox2.Items.Clear();
                for (int col = 1; col < lvl1_block.col_names.Count; col++)
                    listBox2.Items.Add(lvl1_block.col_names[col]);

                ComboBox_setDefault(comboBox3);
                ComboBox_setDefault(comboBox2);

                // начало 4-го уровня
                Lvl4_listBox_fill(lvl1_block);
            }
        }

        // ---------------------------------------------- Уровень 2 ---------------------------------------------------------

        List<Block> lvl2_blocks = new List<Block>();
        int bc = 1039;
        private void button5_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked || !checkBox2.Checked || listBox2.SelectedItems.Count < 3)
                return;
            List<string> colnames = new List<string>();
            colnames.Add("Эпоха");
            colnames.AddRange(listBox2.SelectedItems.OfType<string>());

            double[,] h = new double[colnames.Count-1, lvl1_block.Heights.GetLongLength(1)];

            for (int col = 0; col < colnames.Count - 1; col++)
                    for (int row = 0; row < lvl1_block.Heights.GetLongLength(1); row++)
                        h[col, row] = lvl1_block.Heights[Convert.ToInt32(colnames[col + 1]) - 1, row];

            lvl2_blocks.Add(new Block(h, colnames));
            bc += 1;

            comboBox2.Enabled = true;
            comboBox3.Enabled = true;

            comboBox3.Items.Add((char)bc); //2-й ур
            comboBox2.Items.Add((char)bc); //3-й ур

            comboBox3.SelectedIndex = comboBox3.Items.Count - 1;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBox3.Enabled)
                return;
            Fill_table(dataGridView3, lvl2_blocks[comboBox3.SelectedIndex]);
            Fill_dop_table(dataGridView4, lvl2_blocks[comboBox3.SelectedIndex]);

            Chart_AM(chart3, lvl2_blocks[comboBox3.SelectedIndex]);
            Chart_Mt(chart4, lvl2_blocks[comboBox3.SelectedIndex]);

            groupBox3.Enabled = true;
            groupBox4.Enabled = true;
        }

        // ---------------------------------------------- Уровень 3 ---------------------------------------------------------

        private void button7_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked || !checkBox2.Checked || comboBox2.Items.Count == 0 || comboBox2.SelectedIndex == -1)
                return;
            Length_H();

            listBox_lvl3_2.Items.Clear();
            for (int i = 1; i < lvl2_blocks[comboBox2.SelectedIndex].col_names.Count; i++)
                listBox_lvl3_2.Items.Add(lvl2_blocks[comboBox2.SelectedIndex].col_names[i]);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked || !checkBox2.Checked || listBox_lvl3_2.SelectedItems.Count < 3)
                return;
            List<string> colnames = new List<string>();
            colnames.Add("Эпоха");
            colnames.AddRange(listBox_lvl3_2.SelectedItems.OfType<string>());

            double[,] h = new double[colnames.Count - 1, lvl1_block.Heights.GetLongLength(1)];

            for (int col = 0; col < colnames.Count - 1; col++)
                for (int row = 0; row < lvl1_block.Heights.GetLongLength(1); row++)
                    h[col, row] = lvl1_block.Heights[Convert.ToInt32(colnames[col + 1]) - 1, row];
            Block lvl3_block = new Block(h, colnames);

            Fill_table(dataGridView_lvl3_3, lvl3_block);
            Fill_dop_table(dataGridView_lvl3_4, lvl3_block);

            Chart_AM(chart5, lvl3_block);
            Chart_Mt(chart6, lvl3_block);

            groupBox5.Enabled = true;
            groupBox6.Enabled = true;
        }

        private void Length_H()
        {
            List<string> Lengths_names = new List<string>();
            List<List<double>> Lengths = new List<List<double>>();
            List<double> l;
            Lengths_names.Add("Эпоха");
            for (int i = 1; i < lvl2_blocks[comboBox2.SelectedIndex].col_names.Count - 1; i++)
                for (int j = i + 1; j < lvl2_blocks[comboBox2.SelectedIndex].col_names.Count; j++)
                {
                    Lengths_names.Add($"{lvl2_blocks[comboBox2.SelectedIndex].col_names[i]}-{lvl2_blocks[comboBox2.SelectedIndex].col_names[j]}");
                    l = new List<double>();
                    for (int row = 0; row < lvl1_block.M.Count - 1; row++)
                        l.Add(Math.Abs(lvl1_block.Heights[i, row] - lvl1_block.Heights[j, row]));

                    Lengths.Add(l);
                }

            Fill_table(dataGridView_lvl3_1, Lengths, Lengths_names);
            Delta_length_H(Lengths_names, Lengths);
        }

        private void Delta_length_H(List<string> L_names, List<List<double>> L)
        {
            List<List<double>> Dt_Lengths = new List<List<double>>();
            List<string> Lengths_names = L_names;
            List<double> l_col;
            List<string> sost = new List<string>();
            List<string> sost_col;

            for (int col = 0; col < L.Count; col++)
            {
                l_col = new List<double>();
                sost_col = new List<string>();

                for (int row = 0; row < L[0].Count; row++)
                {
                    l_col.Add(Math.Abs(L[col][0] - L[col][row]));
                    sost_col.Add(l_col[row] <= Block.epsilon ? "+" : "-");
                }
                sost.Add(sost_col.Contains("-") ? "не жесткая" : "жесткая");

                Dt_Lengths.Add(l_col);
            }
            Fill_table(dataGridView_lvl3_2, Dt_Lengths, Lengths_names);

            dataGridView_lvl3_2.Rows.Add();
            dataGridView_lvl3_2[0, dataGridView_lvl3_2.Rows.Count - 1].Value = "Состояние";
            for (int col = 1; col < sost.Count + 1; col++)
            {
                dataGridView_lvl3_2[col, dataGridView_lvl3_2.Rows.Count - 1].Value = sost[col-1];
                dataGridView_lvl3_2[col, dataGridView_lvl3_2.Rows.Count - 1].Style.BackColor = sost[col-1] == "жесткая" ? Color.Green : Color.Red;
            }
        }

        // ---------------------------------------------- Уровень 4 ---------------------------------------------------------

        private void Lvl4_listBox_fill(Block block)
        {
            listBox3.Items.Clear();
            for (int i = 1; i < block.col_names.Count; i++)
                listBox3.Items.Add(block.col_names[i]);
        }

        private void Heights_ColumnNames_add(List<string> colnames, double[,] h, ListBox lb)
        {
            colnames.Add("Эпоха");
            colnames.AddRange(lb.SelectedItems.OfType<string>());

            for (int col = 0; col < colnames.Count - 1; col++)
                for (int row = 0; row < lvl1_block.Heights.GetLongLength(1); row++)
                    h[col, row] = lvl1_block.Heights[Convert.ToInt32(colnames[col + 1]) - 1, row];
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked || !checkBox2.Checked || listBox3.SelectedItems.Count < 1)
                return;

            List<string> colnames = new List<string>();
            double[,] h = new double[listBox3.SelectedItems.Count, lvl1_block.Heights.GetLongLength(1)];
            Heights_ColumnNames_add(colnames, h, listBox3);

            Fill_table(dataGridView5, Lvl4_chart_fill(chart_lvl4, h, colnames), colnames);
            dataGridView5[0, dataGridView5.RowCount - 1].Value = "Прогноз";
        }

        //Добавить строку
        private void button3_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            
            double max_razn = 0;
            double razn;

            string commandText = $"INSERT INTO [{comboBox1.SelectedItem}] ([Эпоха]";
            string commandTextValues = $"{dTable.Rows.Count}";
            for (int col = 1; col < dTable.Columns.Count; col++)
            {
                for (int row = 1; row < dTable.Rows.Count; row++)
                {
                    razn = Math.Abs(Convert.ToDouble(dTable.Rows[0][col]) - Convert.ToDouble(dTable.Rows[row][col]));
                    if (razn > max_razn || row == 1)
                        max_razn = razn;
                }

                commandTextValues += $", '{Convert.ToDouble(dTable.Rows[0][col]) + Math.Round(max_razn * rnd.Next(-10000, 10000) / 10000.0, 4)}'";
                commandText += $", [{col}]";

            }

            commandText += $") values({commandTextValues})";

            SQLiteCommand Command = new SQLiteCommand(commandText, SQLiteConn);

            Command.ExecuteNonQuery();
            ShowTableSQL(SQL_AllTable());
        }

        //Удалить строку
        private void button4_Click(object sender, EventArgs e)
        {
            if (dTable.Rows.Count > 2)
            {
                SQLiteCommand Command = new SQLiteCommand($"Delete from [{comboBox1.SelectedItem}] where [Эпоха] = {dTable.Rows.Count - 1}", SQLiteConn);
                Command.ExecuteNonQuery();
                ShowTableSQL(SQL_AllTable());
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            tabControl1.Height = this.Height - statusStrip1.Height - 37;
        }
    }
}
