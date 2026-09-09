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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Globalization;


namespace JustACalculator
{
    /// <summary>
    /// This Calculator was made without packages like DynamicEspresso.Core.
    /// It works via mouse and keyboard.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _displayText = string.Empty;

        public string DisplayText
        {
            get => _displayText;
            set
            {
                if (_displayText != value)
                {
                    _displayText = value;
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        private void ClearEntryButton_Click(object sender, RoutedEventArgs e)
        {
            // Entferne letztes Zeichen (Entry)
            if (!string.IsNullOrEmpty(DisplayText))
            {
                DisplayText = DisplayText.Substring(0, DisplayText.Length - 1);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // damit Binding in XAML funktioniert
            // Tastatureingaben abfangen
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.PreviewTextInput += MainWindow_PreviewTextInput;
            this.Loaded += (s, e) => Keyboard.Focus(this);
        }


        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                // Generischer Handler für Test/Equals: einfach Content verarbeiten
                HandleInput(b.Content?.ToString());
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Multiplication: handle Numpad '*' (Multiply) and letter 'x' or Shift+8 (common on many layouts)
            if (e.Key == Key.Multiply || e.Key == Key.X || (e.Key == Key.D8 && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)))
            {
                HandleInput("x");
                e.Handled = true;
                return;
            }

            // Division: handle Numpad '/' (Divide) and common main-key scancodes (varies by layout)
            // Also handle Shift+7 which on some layouts produces '/'
            if (e.Key == Key.D7 && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                HandleInput("/");
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Divide || e.Key == Key.Oem2 || e.Key == Key.OemQuestion || e.Key == Key.Oem7 || e.Key == Key.OemBackslash)
            {
                HandleInput("/");
                e.Handled = true;
                return;
            }

            // Ziffern (Top-Row)
            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                var digit = (int)e.Key - (int)Key.D0;
                HandleInput(digit.ToString());
                e.Handled = true;
                return;
            }

            // Ziffern (NumPad)
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                var digit = (int)e.Key - (int)Key.NumPad0;
                HandleInput(digit.ToString());
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Add:
                    HandleInput("+");
                    break;
                case Key.OemPlus:
                    // Haupttasten-Plus (meist Shift+Ü oder gleich je nach Layout)
                    HandleInput("+");
                    break;
                case Key.Subtract:
                    HandleInput("-");
                    break;
                case Key.OemMinus:
                    // Haupttasten-Minus (bindet '-' auf der normalen Tastatur)
                    HandleInput("-");
                    break;
                case Key.Multiply:
                    HandleInput("x");
                    break;
                case Key.Divide:
                    HandleInput("/");
                    break;
                case Key.OemComma:
                case Key.OemPeriod:
                    // akzeptiere Punkt und Komma als Dezimaltrenner
                    HandleInput(",");
                    break;
                case Key.Enter:
                    EqualsButton_Click(this, new RoutedEventArgs());
                    break;
                case Key.Back:
                    ClearEntryButton_Click(this, new RoutedEventArgs());
                    break;
                case Key.Escape:
                    Cancel_Click(this, new RoutedEventArgs());
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void MainWindow_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
                return;

            var ch = e.Text[0];

            if (char.IsDigit(ch))
            {
                HandleInput(ch.ToString());
                e.Handled = true;
                return;
            }

            switch (ch)
            {
                case '+': HandleInput("+"); e.Handled = true; break;
                case '-': HandleInput("-"); e.Handled = true; break;
                case 'x':
                case 'X':
                case '*': HandleInput("x"); e.Handled = true; break;
                case '/': HandleInput("/"); e.Handled = true; break;
                case ',':
                case '.': HandleInput(","); e.Handled = true; break;
                default: break;
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            HandleInput(PlusButton.Content?.ToString());
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            HandleInput(MinusButton.Content?.ToString());
        }

        private void MultiplyButton_Click(object sender, RoutedEventArgs e)
        {
            HandleInput(MultiplyButton.Content?.ToString());
        }

        private void DivideButton_Click(object sender, RoutedEventArgs e)
        {
            HandleInput(DivideButton.Content?.ToString());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Löschen-Button (C)
            DisplayText = string.Empty;
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            // Berechne Ergebnis der aktuellen Eingabe und zeige es im Display an.
            if (string.IsNullOrWhiteSpace(DisplayText))
                return;

            // Entferne eventuell letztes Operator-Zeichen
            var ops = new[] { "+", "-", "x", "/" };
            if (ops.Any(op => DisplayText.EndsWith(op)))
            {
                // entferne letztes Zeichen
                DisplayText = DisplayText.Substring(0, DisplayText.Length - 1);
                if (string.IsNullOrWhiteSpace(DisplayText))
                    return;
            }

            // DataTable Compute erwartet '*' für Multiplikation
            // und als Dezimaltrennzeichen den Punkt. Ersetze daher lokales Komma durch Punkt
            var expr = DisplayText.Replace("x", "*").Replace(",", ".");

            try
            {
                var dt = new DataTable();
                var value = dt.Compute(expr, string.Empty);

                // Ergebnis in invariant format holen, dann an aktuelle Kultur anpassen
                string result;
                if (value is double || value is decimal || value is float)
                {
                    result = Convert.ToDouble(value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    result = value?.ToString() ?? string.Empty;
                }

                // Wenn aktuelle Kultur Komma als Dezimaltrennzeichen verwendet, Punkt ersetzen
                var currentSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                if (currentSep == ",")
                    result = result.Replace(".", ",");

                DisplayText = result;
            }
            catch
            {
                DisplayText = "Error";
            }
        }

        private void HandleInput(string? token)
        {
            // Null-Guard: wenn token null oder leer ist, nichts tun
            if (string.IsNullOrEmpty(token))
                return;

            // Operatoren dürfen nicht doppelt hintereinander stehen und nicht als erstes Zeichen
            var operators = new[] { "+", "-", "x", "/" };

            if (operators.Contains(token))
            {
                if (string.IsNullOrEmpty(DisplayText))
                    return; // kein Operator als erstes Zeichen

                // Wenn rechts (neuestes Zeichen) bereits ein Operator ist, nicht erneut erlauben
                foreach (var op in operators)
                {
                    if (DisplayText.EndsWith(op))
                        return;
                }
            }

            // Anfügen am Ende: Eingabe wächst von rechts nach links im UI (FlowDirection=RightToLeft)
            DisplayText = DisplayText + token;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
