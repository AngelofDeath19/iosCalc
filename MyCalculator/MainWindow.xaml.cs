using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace MyCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isShiftPressed = false; // Track if Shift is pressed

        double firstNumber = 0;
        string selectedOperation = "";
        bool isOperationClicked = false; // Check if an operation button is clicked
        bool isInErrorState = false; // Check if the calculator is in an error state
        private const string InitialDisplay = "0"; // Initial display value
        private const string ErrorDisplay = "E"; // Error display value
        private const int MaxLength = 12; // Maximum display length
        public MainWindow()
        {
            InitializeComponent();
            // Default value of display
            ResultDisplay.Text = InitialDisplay;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                isShiftPressed = true; // Set the flag when Shift is pressed
                return;
            }

            bool processed = false;

            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                if (isShiftPressed)
                {
                    if (e.Key == Key.D5) { processed = HandlePercentKey(); }
                    else if (e.Key == Key.D8) { processed = HandleOperationKey("x"); }
                }
                else
                {
                    processed = HandleDigitKey((e.Key - Key.D0).ToString());
                }
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                processed = HandleDigitKey((e.Key - Key.NumPad0).ToString());
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Add:
                    case Key.OemPlus:
                        processed = HandleOperationKey("+");
                        break;
                    case Key.Subtract:
                    case Key.OemMinus:
                        processed = HandleOperationKey("-");
                        break;
                    case Key.Multiply:
                        processed = HandleOperationKey("x");
                        break;
                    case Key.Divide:
                    case Key.OemQuestion:
                        processed = HandleOperationKey("÷");
                        break;
                    case Key.Decimal:
                    case Key.OemComma:
                    case Key.OemPeriod:
                        processed = HandleDecimalKey();
                        break;
                    case Key.Back:
                        processed = HandleBackspaceKey();
                        break;
                    case Key.Escape:
                        processed = HandleClearKey();
                        break;
                    case Key.Enter:
                        processed = HandleEqualsKey();
                        break;
                    case Key.F9:
                        processed = HandlePlusMinusKey();
                        break;
                }
            }
            e.Handled = processed; // Mark the event as handled to prevent further processing
        }

        private bool HandleDigitKey(string digit)
        {
            Button targetButton = FindButtonByContent(digit);
            if (targetButton != null)
            {
                NumberButton_Click(targetButton, new RoutedEventArgs());
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HandlePercentKey()
        {
            Button targetButton = FindButtonByContent("%");
            if (targetButton != null)
            {
                PercentButton_Click(targetButton, new RoutedEventArgs());
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HandleOperationKey(string operation)
        {
            Button targetButton = FindButtonByContent(operation);
            if (targetButton != null)
            {
                OperationButton_Click(targetButton, new RoutedEventArgs());
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HandleDecimalKey()
        {
            Button targetButton = FindButtonByContent(",");
            if (targetButton != null)
            {
                DecimalButton_Click(targetButton, new RoutedEventArgs());
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HandleBackspaceKey()
        {
            if (ResultDisplay.Text.Length > 1)
            {
                ResultDisplay.Text = ResultDisplay.Text.Substring(0, ResultDisplay.Text.Length - 1);
            }
            else
            {
                ResultDisplay.Text = InitialDisplay;
            }
            return true;
        }

        private bool HandleClearKey()
        {
            ClearButton_Click(ClearButton, new RoutedEventArgs());
            return true;
        }
        
        private bool HandleEqualsKey()
        {
            EqualsButton_Click(EqualsButton, new RoutedEventArgs());
            return true;
        }

        private bool HandlePlusMinusKey()
        {
            PlusMinusButton_Click(PlusMinusButton, new RoutedEventArgs());
            return true;
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                isShiftPressed = false; // Reset the flag when Shift is released
            }
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInErrorState) return;

            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                // Get text (number) from the button
                string digit = clickedButton.Content.ToString();
                // Update display
                string currentDisplay = ResultDisplay.Text;

                if (currentDisplay == InitialDisplay || isOperationClicked)
                {
                    if (digit != ".")
                    {
                        currentDisplay = "";
                        isOperationClicked = false;
                    } 
                }



                int currentLength = currentDisplay.Length;
                if (currentDisplay.StartsWith("-"))
                {
                    currentLength--;
                }
                if (currentLength >= MaxLength)
                {
                    return;
                }

                ResultDisplay.Text = currentDisplay + digit;
            }

            if (ResultDisplay.Text != InitialDisplay) ClearButton.Content = "C";
            isOperationClicked = false; // Reset operation clicked state
        }
        private void OperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInErrorState) return;
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                //Saving the first number
                if (double.TryParse(ResultDisplay.Text, out firstNumber))
                {
                    //Saving the operation
                    selectedOperation = clickedButton.Content.ToString();

                    // Flag to check if an operation button is clicked
                    isOperationClicked = true;

                    // Clear the display
                }
                else
                {
                    ResultDisplay.Text = ErrorDisplay;
                    isInErrorState = true; // Set error state
                }
            }

            ClearButton.Content ="C"; // Change clear button to "C" after first number is entered
            isOperationClicked = true; // Set operation clicked state
        }
        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInErrorState) return;

            //Logic
            double secondNumber;

            //Get the second number
            if (double.TryParse(ResultDisplay.Text, out secondNumber))
            {
                // Perform the operation
                if (!string.IsNullOrEmpty(selectedOperation))
                {
                    double result = PerformCalculation(firstNumber, secondNumber, selectedOperation);

                    if (double.IsNaN(result))
                    {
                        return;
                    }
                    // Display the result
                    ResultDisplay.Text = result.ToString();
                    firstNumber = result;
                    selectedOperation = "";
                    isOperationClicked = false;
                }
            }
            else
            {
                ResultDisplay.Text = ErrorDisplay;
                isInErrorState = true;
            }

            if (!isInErrorState) ClearButton.Content = "C"; // Change clear button to "C" after calculation
        }
        private void DecimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInErrorState) return;

            const int MaxLength = 15;

            // Check if the display already contains a decimal point
            if (!ResultDisplay.Text.Contains(",") && ResultDisplay.Text.Length < MaxLength)
            {
                if (isOperationClicked || ResultDisplay.Text == InitialDisplay)
                {

                    if (MaxLength >= 2)
                    {
                        ResultDisplay.Text = "0,";
                        isOperationClicked = false;
                    }
                }
                else
                {
                    ResultDisplay.Text += ",";
                }
            }

            if (ResultDisplay.Text != InitialDisplay && ResultDisplay.Text != "0,") ClearButton.Content = "C";
        }
        private void PlusMinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInErrorState) return;

            double currentValue;
           
            if (double.TryParse(ResultDisplay.Text, out currentValue))
            {
                if (double.TryParse(ResultDisplay.Text, out currentValue))
                {
                    //Change the sign of the number
                    if (currentValue != 0)
                    {
                        currentValue *= -1;
                        ResultDisplay.Text = currentValue.ToString();
                    }
                }
            }
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInErrorState)
            {
                ResultDisplay.Text = InitialDisplay;
                firstNumber = 0;
                selectedOperation = "";
                isOperationClicked = false;
                isInErrorState = false; // Reset error state
                ClearButton.Content = "AC";
                return;
            }

            if (ResultDisplay.Text != InitialDisplay)
            {
                ResultDisplay.Text = InitialDisplay;

                ClearButton.Content = "AC";
            }
            else
            {
                firstNumber = 0;
                selectedOperation = "";
                isOperationClicked = false; // Reset operation clicked state

                ClearButton.Content = "AC";
            }

            ResultDisplay.Text = InitialDisplay;
            //selectedOperation = "";
            //isOperationClicked = false;
            isInErrorState = false; // Reset error state
        }

        private double PerformCalculation(double num1, double num2, string operation)
        {
            double result = 0;
            switch (operation)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
                case "x":
                    result = num1 * num2;
                    break;
                case "÷":
                    if (num2 == 0)
                    {
                        ResultDisplay.Text = ErrorDisplay;
                        isInErrorState = true;
                        return double.NaN;
                    }
                    result = num1 / num2;
                    break;
            }
            return result;
        }

        private void PercentButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInErrorState) return;

            if (!string.IsNullOrEmpty(selectedOperation) && !isOperationClicked)
            {
                double secondNumber;
                if (double.TryParse(ResultDisplay.Text, out secondNumber))
                {
                    double percentageValue = 0;

                    switch (selectedOperation)
                    {
                        case "+":
                        case "-":
                            percentageValue = firstNumber * secondNumber / 100;
                            break;
                        case "x":
                        case "/":
                        case "÷":
                            if (selectedOperation == "%" && secondNumber == 0)
                            {
                                ResultDisplay.Text = ErrorDisplay;
                                isInErrorState = true;
                                return;
                            }
                            percentageValue = secondNumber / 100;
                            break;
                        default:
                            break;
                    }

                    double result = PerformCalculation(firstNumber, percentageValue, selectedOperation);

                    if (double.IsNaN(result))
                    {
                        return;
                    }

                    ResultDisplay.Text = result.ToString();

                    firstNumber = result;

                    isOperationClicked = false;
                }
            }
            else
            {
                 ResultDisplay.Text = ErrorDisplay;
                 isInErrorState = true;
            }

            if (ResultDisplay.Text != InitialDisplay) ClearButton.Content = "C"; // Change clear button to "C" after calculation
        }

        private Button FindButtonByContent(string content)
        {
            foreach (var child in ButtonGrid.Children)
            {
                if (child is Button button && button.Content.ToString() == content)
                {
                    return button;
                }
            }
            return null;
        }
    }
}