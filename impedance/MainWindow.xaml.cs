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
                double z0 = ParseDoubleInput(txtWaveImpedance.Text, "Волновой импеданс (Zв)");
                double k = ParseDoubleInput(txtK.Text, "Коэффициент К");
                double waveLength = ParseDoubleInput(txtWaveLength.Text, "Длина волны (λ)");
                double zmin = ParseDoubleInput(txtZmin.Text, "ΔZmin");

                // Проверка ввода
                if (waveLength <= 0)
                    throw new ArgumentException("Длина волны должна быть больше 0");

                // Подсчет и отображение результатов
                string result = CalculateLoadImpedance(z0, k, waveLength, zmin);
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

        private string CalculateLoadImpedance(double z0, double k, double waveLength, double zmin)
        {
            string result = "=== Расчёт импеданса нагрузки ===\n\n";

            // Отображение введенных параметров
            result += "Введенные параметры:\n";
            result += $"• Волновой импеданс (Zв): {z0} Ом\n";
            result += $"• Коэффициент К: {k}\n";
            result += $"• Длина волны (λ): {waveLength} м\n";
            result += $"• ΔZmin: {zmin} м\n\n";

            // Расчёт волнового числа β = 2π/λ
            double beta = (2 * Math.PI) / waveLength;
            result += "Промежуточные подсчеты:\n";
            result += $"• Волновое число (β = 2π/λ): {beta:F6} рад/м\n";

            // Расчёт β × ΔZmin
            double betaZmin = beta * zmin;
            result += $"• β × ΔZmin: {betaZmin:F6} рад\n";

            // Расчёт tan(β × Zmin)
            double tgBetaZmin = Math.Tan(betaZmin);
            result += $"• tg(β × ΔZmin): {tgBetaZmin:F6}\n\n";

            // Расчёт с использованием комплексных чисел для точного определения импеданса
            // Формула: Zн = Zв × (i + K×tg(β×ΔZmin)) / (iK + tg(β×ΔZmin))

            Complex numerator = new Complex(k * tgBetaZmin, 1);
            Complex denominator = new Complex(tgBetaZmin, k);

            result += "Расчет комплексных чисел:\n";
            result += $"• Числитель: (i + K×tg(β×ΔZmin)) = {FormatComplex(numerator)}\n";
            result += $"• Знаменатель: (iK + tg(β×ΔZmin)) = {FormatComplex(denominator)}\n";

            // Расчёт деления комплексных чисел
            Complex ratio = numerator / denominator;
            result += $"• Результат деления: {FormatComplex(ratio)}\n\n";

            // Расчёт 
            Complex zLoad = z0 * ratio;

            result += "Финальный результат:\n"; 
            result += $"• Импеданс нагрузки (Zн): {FormatComplex(zLoad)} Ом\n";


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
                return c.Imaginary >= 0 ? $"j{c.Imaginary:F6}" : $"-j{Math.Abs(c.Imaginary):F6}";
            }
            else
            {
                string imagPart = c.Imaginary >= 0 ? $" + j{c.Imaginary:F6}" : $" - j{Math.Abs(c.Imaginary):F6}";
                return $"{c.Real:F6}{imagPart}";
            }
        }
    }
}