using System;
using System.Numerics;
using System.Windows;

namespace impedance
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сбор введенных значений
                double Zwave = ParseDoubleInput(txtWaveImpedance.Text, "Волновой импеданс (Zв)");
                double waveLength = ParseDoubleInput(txtWaveLength.Text, "Длина волны (λв)");
                double Z0min = ParseDoubleInput(txtZ0min.Text, "Z0min");
                double Zmin = ParseDoubleInput(txtZmin.Text, "Zmin");
                double ZImax = ParseDoubleInput(txtZImax.Text, "Z(Iд max)");
                double ZImin = ParseDoubleInput(txtZImin.Text, "Z(Iд min)");

                // Проверка ввода
                if (waveLength <= 0)
                    throw new ArgumentException("Длина волны должна быть больше 0");

                // Подсчет и отображение результатов
                string result = CalculateLoadImpedance(Zwave, waveLength, Z0min, Zmin, ZImax, ZImin);
                txtResult.Text = result;
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Ошибка: {ex.Message}";
            }
        }

        private double ParseDoubleInput(string input, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException($"{parameterName} не может быть пустым.");
            }

            if (!double.TryParse(input, out double value))
            {
                throw new ArgumentException($"{parameterName} Должно быть действительным.");
            }

            return value;
        }

        private string CalculateLoadImpedance(double Zwave, double waveLength, double Z0min, double Zmin, double ZImax, double ZImin)
        {
            string result = "=== Расчёт импеданса нагрузки ===\n\n";
            

            
            // Расчёт волнового числа β = 2π/λ
            double beta = 2 * Math.PI / waveLength;

            // Расчёт КСВ
            double Umax = Math.Abs(Math.Sin(beta * (ZImax - Z0min)));
            double Umin = Math.Abs(Math.Sin(beta * (ZImin - Z0min)));
            double K = Umax / Umin;

            // Расчёт ΔZmin
            double deltaZmin = Z0min - Zmin;

            // Тангенс из формулы
            double tg = Math.Tan(beta * deltaZmin);

            // Расчёт с использованием комплексных чисел для определения импеданса
            Complex numerator = new Complex(K * tg, 1);
            Complex denuminator = new Complex(tg, K);

            // Расчёт импеданса нагрузки(Zн)
            Complex Zload = Zwave * (numerator / denuminator);

            // Расчёт Коэффициента отражения(Гz)
            Complex Гz = (Zload - Zwave) / (Zload + Zwave);

            result += $"• Коэффициент Стоячей Волны (К): {K}\n";
            result += $"• Импеданс нагрузки (Zн): {FormatComplex(Zload)} Ом\n";
            result += $"• Коэффициент отражения(Гz): {FormatComplex(Гz)} \n";

            return result;
        }

        private string FormatComplex(Complex c)
        {
            if (Math.Abs(c.Imaginary) < 1e-10)
            {
                return $"{c.Real:F6}";
            }
            else if (Math.Abs(c.Real) < 1e-10)
            {
                return c.Imaginary >= 0 ? $"{c.Imaginary:F6}j" : $"-{Math.Abs(c.Imaginary):F6}j";
            }
            else
            {
                string imagPart = c.Imaginary >= 0 ? $" + {c.Imaginary:F6}j" : $" - {Math.Abs(c.Imaginary):F6}j";
                return $"{c.Real:F6}{imagPart}";
            }
        }
    }
}