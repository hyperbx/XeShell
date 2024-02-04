using System.Text;
using XeSharp.Helpers;

namespace XeShell
{
    public class Prompt
    {
        private static readonly string _defaultPrompt = "> ";
        private static string _prompt = _defaultPrompt;

        private static readonly List<string> _history = new(50);
        private static int _historyIndex = -1;

        private static StringBuilder _input;
        private static int _inputIndex = 0;

        public static int Seek
        {
            get => Console.GetCursorPosition().Left;
            set => Console.SetCursorPosition(value, Console.GetCursorPosition().Top);
        }

        public static T Show<T>(string in_prompt = "")
        {
            if (string.IsNullOrEmpty(in_prompt))
                in_prompt = _defaultPrompt;

            if (!in_prompt.EndsWith(' '))
                in_prompt += " ";

            Console.Write(_prompt = in_prompt);

            return MemoryHelper.ChangeType<T>(GetInput());
        }

        public static string Show(string in_prompt = "")
        {
            return Show<string>(in_prompt);
        }

        private static string GetInput()
        {
            _input = new StringBuilder();

            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Backspace && _input.Length > 0)
                {
                    SeekPosition(-1);

                    _input.Remove(_inputIndex, 1);

                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    _input.Insert(_inputIndex, keyInfo.KeyChar);

                    SeekPosition(1);

                    // Rewrite prompt if we're seeked in the middle of the input.
                    if (_input.Length > _inputIndex)
                    {
                        ClearInput();
                        Console.Write(_input.ToString());
                        Seek = _prompt.Length + _inputIndex;
                        continue;
                    }

                    Console.Write(keyInfo.KeyChar);
                }

                if (keyInfo.Key is ConsoleKey.UpArrow or ConsoleKey.DownArrow)
                    SeekHistory(keyInfo.Key == ConsoleKey.DownArrow);

                if (keyInfo.Key is ConsoleKey.LeftArrow or ConsoleKey.RightArrow)
                    SeekPosition(keyInfo.Key == ConsoleKey.RightArrow ? 1 : -1, true);
            }
            while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();

            var result = _input.ToString();

            if (!result.IsNullOrEmptyOrWhiteSpace())
            {
                _history.Add(result);
                _historyIndex = _history.Count;
            }

            _inputIndex = 0;

            return result;
        }

        private static void ClearInput()
        {
            Seek = _prompt.Length + _input.Length;

            for (int i = 0; i < _input.Length; i++)
                Console.Write("\b \b");
        }

        private static void SeekPosition(int in_relativePosition, bool in_isSeekConsole = false)
        {
            if (in_relativePosition > 0)
            {
                _inputIndex = Math.Min(_input.Length, _inputIndex + in_relativePosition);
            }
            else
            {
                _inputIndex = Math.Max(0, _inputIndex - Math.Abs(in_relativePosition));
            }

            if (!in_isSeekConsole)
                return;

            // Keep cursor within the boundaries of post-prompt length and pre-input length.
            var newPos = Math.Max(_prompt.Length,
                Math.Min(_prompt.Length + _input.Length, Seek + in_relativePosition));

            Seek = newPos;
        }

        private static void SeekHistory(bool in_isDown = false)
        {
            if (_history.Count <= 0)
                return;

            var currentInput = _input.ToString();

            ClearInput();

            _input.Clear();

            _historyIndex = in_isDown
                ? Math.Min(_history.Count, _historyIndex + 1)
                : Math.Max(0, _historyIndex - 1);

            _input.Append(_historyIndex < _history.Count ? _history[_historyIndex] : currentInput);
            _inputIndex = _input.Length;

            Console.Write(_input.ToString());

            Seek = _prompt.Length + _inputIndex;
        }
    }
}
