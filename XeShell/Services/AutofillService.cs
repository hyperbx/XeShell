using System.Diagnostics;
using XeSharp.Device;
using XeSharp.Helpers;
using XeShell.Interop;

namespace XeShell.Services
{
    public class AutofillService
    {
        protected XeDbgConsole _console;
        protected PromptService _promptService;

        private bool _isInputBuffered = false;
        private string _searchQuery = string.Empty;
        private List<string> _searchResults = [];

        public AutofillService() { }

        /// <summary>
        /// Creates an autofill service for the input console and prompt.
        /// </summary>
        /// <param name="in_console">The console to pull the filesystem from.</param>
        /// <param name="in_promptService">The prompt service to manipulate.</param>
        public AutofillService(XeDbgConsole in_console, PromptService in_promptService)
        {
            _console = in_console;
            _promptService = in_promptService;

            // Start autofill service.
            Task.Run(Run);
        }

        /// <summary>
        /// Starts the autofill service.
        /// </summary>
        public void Run()
        {
            while (!_promptService.CancellationTokenSource.IsCancellationRequested)
            {
                if (Win32.IsKeyDown(EKeyCode.Tab))
                {
                    if (_isInputBuffered)
                        continue;

                    var cli = _promptService.InputRaw.ToString().Split(' ');
#if DEBUG
                    Debug.WriteLine($"Command Line: {string.Join(' ', cli)}");
#endif
                    // Get the last thing being typed (likely a path).
                    var query = cli.Last().ToLower();

                    if (query.IsNullOrEmptyOrWhiteSpace())
                        continue;

                    // Start a new search.
                    if (query != _searchQuery)
                    {
                        _searchQuery = query;
                        _searchResults.Clear();
                    }
#if DEBUG
                    Debug.WriteLine($"Autofill Query: {_searchQuery}");
#endif
                    // Search for this path in the current directory.
                    if (_searchResults.Count <= 0)
                    {
                        var isAbsolutePath = FormatHelper.IsAbsolutePath(query);

                        var node = isAbsolutePath
                            ? _console.FileSystem.GetNodeFromPath(query)
                            : _console.FileSystem.CurrentDirectory;

                        if (isAbsolutePath)
                        {
                            _searchResults = node.Nodes
                                .Where(x => x.ToString().ToLower().StartsWith(query))
                                .Select(x => x.ToString())
                                .ToList();
                        }
                        else
                        {
                            _searchResults = node.Nodes
                                .Where(x => x.Name.ToLower().StartsWith(query))
                                .Select(x => x.Name)
                                .ToList();
                        }

                        if (_searchResults.Count <= 0)
                            goto Exit;
                    }

                    var output = cli.SkipLast(1).ToList();
                    var result = _searchResults[0];

                    // Wrap path with spaces in quotes.
                    if (result.Contains(' '))
                        result = $"\"{result}\"";
#if DEBUG
                    Debug.WriteLine($"Autofill Result: {result}");
#endif
                    output.Add(result);
                    _searchResults.RemoveAt(0);

                    // Recreate command line input.
                    _promptService.ClearInput();
                    _promptService.Input.Clear();
                    _promptService.Input.Append(string.Join(' ', output));
                    _promptService.Refresh(() => _promptService.InputIndex = _promptService.Input.Length);

                Exit:
                    _isInputBuffered = true;
                }
                else
                {
                    _isInputBuffered = false;
                }
            }
        }
    }
}
