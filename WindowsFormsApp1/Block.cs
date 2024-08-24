using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    class Block
    {
        public List<double> M;
        public List<double> M_plus;
        public List<double> M_minus;
        public List<double> Mpr;
        public List<double> Mpr_plus;
        public List<double> Mpr_minus;
        public List<double> a;
        public List<double> a_plus;
        public List<double> a_minus;
        public List<double> apr;
        public List<double> apr_plus;
        public List<double> apr_minus;
        public double[,] Heights;
        double[,] Heights_plus;
        double[,] Heights_minus;
        public List<string> Sost;
        public static double epsilon;
        public static double k;
        public List<string> col_names;
        public List<double> R;
        public List<double> eps_str;
        public List<double> MM;
        public static List<string> dop_col_names = new List<string> { "Эпоха", "M", "a", "Mпр", "aпр", "M+", "a+", "Mпр+", "aпр+", "M-", "a-", "Mпр-", "aпр-", "ε'", "R", "|Mi-M0|"/*, "Состояние" */};

        public List<List<double>> Rasch_zn
        {
            get
            {
                return new List<List<double>> { M, a, Mpr, apr, M_plus, a_plus, Mpr_plus, apr_plus, M_minus, a_minus, Mpr_minus, apr_minus, eps_str, R, MM };
            }
        }

        public Block(double[,] H, List<string> col_n)
        {
            col_names = col_n;

            M = new List<double>();
            M_plus = new List<double>();
            M_minus = new List<double>();
            
            Mpr = new List<double>();
            Mpr_plus = new List<double>();
            Mpr_minus = new List<double>();
            
            a = new List<double>();
            a_plus = new List<double>();
            a_minus = new List<double>();

            apr = new List<double>();
            apr_plus = new List<double>();
            apr_minus = new List<double>();

            Heights = H;
            Heights_plus = new double[Heights.GetLongLength(0), Heights.GetLongLength(1)];
            Heights_minus = new double[Heights.GetLongLength(0), Heights.GetLongLength(1)];

            Calculate_boundary_Heights(Heights, epsilon);

            Calculate_M(Heights, M);
            Calculate_M(Heights_plus, M_plus);
            Calculate_M(Heights_minus, M_minus);

            Calculate_a(Heights, a, M);
            Calculate_a(Heights_plus, a_plus, M_plus);
            Calculate_a(Heights_minus, a_minus, M_minus);

            Prognoz(M, Mpr);
            Prognoz(M_plus, Mpr_plus);
            Prognoz(M_minus, Mpr_minus);

            Prognoz(a, apr);
            Prognoz(a_plus, apr_plus);
            Prognoz(a_minus, apr_minus);

            Check_sost();
        }
        public Block(DataGridView H) : this(H_ToArray(H), get_colnames(H)) { }
        private static double[,] H_ToArray(DataGridView H)
        {
            double[,] Heights = new double[H.Columns.Count - 1, H.Rows.Count];
            for (int col = 0; col < H.Columns.Count - 1; col++)
                for (int row = 0; row < H.Rows.Count; row++)
                {
                    Heights[col, row] = Convert.ToDouble(H[col + 1, row].Value);
                }
            return Heights;
        }
        private static List<string> get_colnames(DataGridView H)
        {
            List<string> colnames = new List<string>();
            foreach (DataGridViewColumn col in H.Columns)
                colnames.Add(col.Name);
            return colnames;
        }

        private void Calculate_boundary_Heights(double[,] H, double E)
        {
            for (int col = 0; col < H.GetLongLength(0); col++)
                for (int row = 0; row < H.GetLongLength(1); row++)
                {
                    Heights_plus[col, row] = H[col, row] + E;
                    Heights_minus[col, row] = H[col, row] - E;
                }
        }

        private void Calculate_M(double[,] Heights, List<double> M)
        {
            double summ;
            for (int row = 0; row < Heights.GetLongLength(1); row++)
            {
                summ = 0;
                for (int col = 0; col < Heights.GetLongLength(0); col++)
                    summ += Math.Pow(Heights[col, row], 2);
                M.Add(Math.Sqrt(summ));
            }
        }

        private void Calculate_a(double[,] Heights, List<double> a, List<double> M)
        {
            double Summ;
            a.Add(0);
            for (int row = 1; row < Heights.GetLongLength(1); row++)
            {
                Summ = 0;
                for (int col = 0; col < Heights.GetLongLength(0); col++)
                    Summ += Heights[col, 0] * Heights[col, row];
                a.Add(Math.Round(Math.Acos(Summ < (M[row] * M[0])? Summ / (M[row] * M[0]) : 1), 7) * 180 / Math.PI * 60 * 60);
            }
        }

        /// <summary>
        /// Прогнозная ф-ция
        /// </summary>
        /// <param name="progn">лист прогнозируемых значений</param>
        public static List<double> Prognoz(List<double> progn)
        {
            //лист расчетных значений
            List<double> r_zn = new List<double>();
            r_zn.Add(k * progn[0] + (1 - k) * progn.Average());

            for (int row = 1; row < progn.Count; row++)
                r_zn.Add(k * progn[row] + (1 - k) * r_zn[row - 1]);

            r_zn.Add(k * progn.Average() + (1 - k) * progn[progn.Count - 1]);
            progn.Add(r_zn[r_zn.Count-1]);
            return r_zn;
        }
        /// <summary>
        /// Прогнозная ф-ция
        /// </summary>
        /// <param name="M">лист прогнозируемых значений</param>
        /// <param name="Mpr">лист прогнозных значений</param>
        private void Prognoz(List<double> M, List<double> Mpr)
        {
            Mpr.AddRange(Prognoz(M));
        }

        private void Check_sost()
        {
            R = new List<double>();
            MM = new List<double>();
            eps_str = new List<double>();
            Sost = new List<string>();
            for (int row = 0; row < M.Count; row++)
            {
                eps_str.Add(Math.Abs(Math.Round(M_plus[row],7) - Math.Round(M_minus[row],7)));
                R.Add(eps_str[row] / 2);
                MM.Add(Math.Abs(Math.Round(M[row],7) - Math.Round(M[0],7)));

                if (R[row] == MM[row])
                    Sost.Add("Предаварийное");
                else if (R[row] > MM[row])
                    Sost.Add("Нормальное");
                else Sost.Add("Аварийное");
            }

        }
    }
}