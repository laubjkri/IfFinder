using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using KlUtils;
using System.Text.RegularExpressions;
using static IfFinder.FileSearcher;

namespace IfFinder{

    public partial class MainWindow : Window
    {
        private string? _filePath;
        private string? _fileName;
        private FileSearcher _fileSearcher;
        

        public MainWindow()
        {
            // Initalize model before GUI so data is available
            _fileSearcher = new FileSearcher();
            _fileSearcher.ErrorEvent += ShowError;
            _fileSearcher.InfoEvent += ShowInfo;
            _fileSearcher.LogEvent += Log;
            _fileSearcher.SearchLineMethod = SearchLineRegEx;

            InitializeComponent();
            MinWidth = Width; 
            MinHeight = Height;
            ClearLog();           

        }

        private void SelectFileButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                _fileSearcher.FilePath = _filePath;
                _fileName = System.IO.Path.GetFileName(_filePath);


                fileLabel.Content = _fileName;
            }
        }

        private async void SearchFileButtonClick(object sender, RoutedEventArgs e)
        {
            SearchLineDelegate? searchMethod = GetSearchMethod();
            if (searchMethod != null)
            {
                _fileSearcher.SearchLineMethod = searchMethod;
                await _fileSearcher.SearchFileLinesExclusive();
            }
            else
            {
                ShowError("Undefined search method.");
            }
        }

        private int SearchLineSimple(string line, int lineNo)
        {
            if (line.Contains("if"))
            {                
                Log($"Found \"if\" at line {lineNo}");
                return 1;
            }
            return 0;
        }

        private int SearchLineRegEx(string line, int lineNo)
        {

            /*             
                Please note that this solution does not validate the complete if statement,
                and cannot handle multiple lines.             
            
                So for example, this would not be found:
                    "if"
                    (one or more lines)
                    "("

                To validate the whole if statement this would need to be checked (and probably more):
                    "if (condition) {expression}"
                     or 
                    "if (condition) (newline)
                        expression on one line"
            
                A full parsing system would be needed for such a check.
            */

            string pattern =
                @"if" +         // Find the letters "if"
                @"\s*" +        // whitespaces may appear before the paranthesis
                @"\(" +         // The opening paranthesis of the condition
                @"[^\)]+" +     // One or more characters that are not ")" (no check of valid condition)
                @"\)"           // The ending paranthesis of the condition
            ;
            int matchesFound = 0;
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(line);

            foreach (Match match in matches)
            {
                Log($"Found match at line {lineNo}: \"{match}\"");
                matchesFound++;
            }

            return matchesFound;
        }

        private void Log(string message)
        {
            logTextBox.AppendText(message + '\n');            
        }

        private void ClearLog()
        {
            logTextBox.Clear();
        }

        private void ClearLogButtonClick(object sender, RoutedEventArgs e)
        {
            ClearLog();
        }

        private SearchLineDelegate? GetSearchMethod()
        {
            ComboBoxItem selectedItem = (ComboBoxItem)searchMethodComboBox.SelectedItem;
            string? tag = selectedItem.Tag.ToString();
            switch (tag)
            {
                case "simple":
                    return SearchLineSimple;                    
                case "regex":
                    return SearchLineRegEx;                    
                default:
                    return null;                    
            }
        }

        private void ShowError(string message)
        {
            KlMessageBox.Show("Error", message, this);
        }

        private void ShowInfo(string message)
        {
            KlMessageBox.Show("Info", message, this);
        }

    }
}
