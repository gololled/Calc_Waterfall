using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CalculatorApp
{
    public partial class MainWindow : Window
    {
        private bool isDarkTheme = false;
        private string currentOperation = "";
        private List<string> history = new List<string>();
        private bool isResultDisplayed = false;

        public MainWindow()
        {
            InitializeComponent();
            SetTheme();
        }

        private void SetTheme()
        {
            var backgroundBrush = isDarkTheme ? new SolidColorBrush(Color.FromRgb(46, 46, 46)) : Brushes.White;
            var buttonBrush = isDarkTheme ? new SolidColorBrush(Color.FromRgb(59, 59, 59)) : Brushes.LightGray;
            var foregroundBrush = isDarkTheme ? Brushes.White : Brushes.Black;

            Background = backgroundBrush;
            Display.Background = backgroundBrush;
            Display.Foreground = foregroundBrush;
            HistoryList.Background = backgroundBrush;
            HistoryList.Foreground = foregroundBrush;
            HistBox.Background = backgroundBrush;
            HistBox.Foreground = foregroundBrush;

            foreach (var child in LogicalTreeHelper.GetChildren(this))
            {
                if (child is Button button)
                {
                    button.Background = buttonBrush;
                    button.Foreground = foregroundBrush;
                }
            }
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            isDarkTheme = !isDarkTheme;
            SetTheme();
        }

        private void Digit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (isResultDisplayed)
            {
                Display.Clear();
                isResultDisplayed = false;
            }
            Display.Text += button.Content.ToString();
        }

        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                Display.Text += $" {button.Content} ";
                isResultDisplayed = false;
            }
        }

        private void ToggleSign_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Display.Text, out double number))
            {
                number = -number;
                Display.Text = number.ToString("0.#####");
            }
        }

        private void Sin_Click(object sender, RoutedEventArgs e) => CalculateUnaryOperation(Math.Sin, "sin");
        private void Cos_Click(object sender, RoutedEventArgs e) => CalculateUnaryOperation(Math.Cos, "cos");
        private void Sqrt_Click(object sender, RoutedEventArgs e) => CalculateUnaryOperation(Math.Sqrt, "sqrt");

        private void Factorial_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(Display.Text, out int number) && number >= 0)
            {
                long result = Factorial(number);
                Display.Text = result.ToString();
                AddToHistory($"{number}! = {result}");
            }
            else
            {
                Display.Text = "Ошибка";
            }
        }

        private long Factorial(int n)
        {
            return n <= 1 ? 1 : n * Factorial(n - 1);
        }

        private void Bracket_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                if (isResultDisplayed)
                {
                    Display.Clear();
                    isResultDisplayed = false;
                }
                Display.Text += button.Content.ToString();
            }
        }

        private void Power_Click(object sender, RoutedEventArgs e)
        {
            Display.Text += "^";
        }

        private void CalculateUnaryOperation(Func<double, double> operation, string operationSymbol)
        {
            if (double.TryParse(Display.Text, out double result))
            {
                double originalValue = result;
                result = operation(result);
                Display.Text = result.ToString("0.#####");
                AddToHistory($"{operationSymbol}({originalValue}) = {result}");
                isResultDisplayed = true;
            }
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var expression = Display.Text
                    .Replace("×", "*")
                    .Replace("÷", "/");

                while (expression.Contains("^"))
                {
                    int powerIndex = expression.IndexOf("^");
                    int leftIndex = powerIndex - 1;
                    int rightIndex = powerIndex + 1;

                    while (leftIndex >= 0 && (char.IsDigit(expression[leftIndex]) || expression[leftIndex] == '.'))
                    {
                        leftIndex--;
                    }

                    while (rightIndex < expression.Length && (char.IsDigit(expression[rightIndex]) || expression[rightIndex] == '.'))
                    {
                        rightIndex++;
                    }

                    double baseNum = double.Parse(expression.Substring(leftIndex + 1, powerIndex - leftIndex - 1));
                    double exponent = double.Parse(expression.Substring(powerIndex + 1, rightIndex - powerIndex - 1));
                    double powerResult = Math.Pow(baseNum, exponent);

                    expression = expression.Substring(0, leftIndex + 1) + powerResult.ToString("0.#####") + expression.Substring(rightIndex);
                }

                var result = EvaluateExpression(expression);
                Display.Text = result.ToString("0.#####");
                AddToHistory($"{expression} = {result}");
                currentOperation = "";
                isResultDisplayed = true;
            }
            catch (Exception)
            {
                Display.Text = "Ошибка";
            }
        }

        private double EvaluateExpression(string expression)
        {
            var dataTable = new DataTable();
            return Convert.ToDouble(dataTable.Compute(expression, string.Empty));
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Display.Clear();
            currentOperation = "";
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (Display.Text.Length > 0)
            {
                Display.Text = Display.Text.Substring(0, Display.Text.Length - 1);
            }
        }

        private void AddToHistory(string operation)
        {
            if (HistoryList.Items.Count >= 14)
            {
                HistoryList.Items.Clear();
            }

            history.Add(operation);
            HistoryList.Items.Add(operation);
        }

    }
}
