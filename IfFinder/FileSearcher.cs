using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace IfFinder
{
    public class FileSearcher
    {   
        /// <summary>
        /// Method that searches a line in the specified file.
        /// </summary>
        /// <param name="line">A string containing a line from the file.</param>
        /// <param name="lineNo">The current linenumber og the line being passed.</param>
        /// <returns>The number of matches found in the line.</returns>
        public delegate int SearchLineDelegate(string line, int lineNo);

        public SearchLineDelegate? SearchLineMethod { get; set; } 
        public event Action<string>? LogEvent;
        public event Action<string>? InfoEvent;
        public event Action<string>? ErrorEvent;

        public string FilePath { get; set; } = string.Empty;
        private Task _searchTask = Task.CompletedTask;        

        /// <summary>
        /// Start a line by line file search.
        /// The search method and file path must be assigned to the owning object.
        /// Only allows one search to be started at a time.
        /// </summary>        
        public async Task SearchFileLinesExclusive()
        {
            if (_searchTask.IsCompleted)
            {
                _searchTask = SearchFileLines();
                await _searchTask;
            }
            else
            {
                InvokeInfoEvent("A search is already active.");
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Start a line by line file search.
        /// The search method and file path must be assigned to the owning object.
        /// </summary>        
        public async Task SearchFileLines()
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                    throw new Exception("File path is missing.");

                // Using a stream reader to avoid loading the whole file into memory at once
                // as would be the case with File.ReadAllLines()
                using (StreamReader streamReader = new StreamReader(FilePath))
                {
                    InvokeLogEvent("Search started...");
                    // Make a local copy of the search method to make sure the method is not changed during a search
                    SearchLineDelegate? searchLineMethod = SearchLineMethod;

                    List<string> lines = new List<string>();
                    string? line = await streamReader.ReadLineAsync();
                    int lineNo = 0;
                    int? matchesFound = 0;
                    while (line != null)
                    {
                        lineNo++;
                        matchesFound += searchLineMethod?.Invoke(line, lineNo);
                        // Adding an artificial delay to make it visual in the GUI
                        await Task.Delay(15);
                        line = streamReader.ReadLine();
                    }
                    
                    if(matchesFound == 0)
                    {
                        InvokeLogEvent($"Search done. No matches found.");
                    }
                    else if (matchesFound == 1)
                    {
                        InvokeLogEvent($"Search done. {matchesFound} match found.");
                    }
                    else
                    {
                        InvokeLogEvent($"Search done. {matchesFound} matches found.");
                    }                    
                }
            }

            catch (Exception ex)
            {
                InvokeErrorEventOrThrow(ex.Message, ex);
            }
        }

        private void InvokeErrorEventOrThrow(string errorMessage, Exception? ex = null)
        {
            var exType = ex?.GetType();
            
            // If the error event delegate has been assigned, use this to forward the error message
            if (ErrorEvent != null)
            {
                ErrorEvent(errorMessage);                
            }
            else // Otherwise, rethrow
            {                
                throw new Exception("Error FileSearcher class: " + errorMessage, ex);
            }            
        }

        private void InvokeLogEvent(string message) { LogEvent?.Invoke(message); }
        private void InvokeInfoEvent(string message) { InfoEvent?.Invoke(message); }        

    }
}
