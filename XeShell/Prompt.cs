using System.Text;
using XeSharp.Helpers;

namespace XeShell
{
    public class Prompt
    {
        private static readonly string _defaultPrompt = ">";
        private static readonly List<string> _history = new(50);

        private static int _historyIndex = -1;

        public static T Show<T>(string in_prompt = "")
        {
            if (string.IsNullOrEmpty(in_prompt))
                in_prompt = _defaultPrompt;

            if (!in_prompt.EndsWith(' '))
                in_prompt += " ";

            Console.Write(in_prompt);

            return MemoryHelper.ChangeType<T>(ReadInput());
        }

        public static string Show(string in_prompt = "")
        {
            return Show<string>(in_prompt);
        }

        private static string ReadInput()
        {
            var input = new StringBuilder();

            ConsoleKeyInfo keyInfo;

            // TODO: allow left/right arrow keys to seek input.
            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    input.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);
                }

                if (keyInfo.Key is ConsoleKey.UpArrow or ConsoleKey.DownArrow)
                    WriteInput(ref input, keyInfo.Key == ConsoleKey.DownArrow);
            }
            while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();

            var result = input.ToString();

            if (!result.IsNullOrEmptyOrWhiteSpace())
            {
                _history.Add(result);
                _historyIndex = _history.Count;
            }

            return result;
        }

        private static void WriteInput(ref StringBuilder in_input, bool in_isDown = false)
        {
            if (_history.Count <= 0)
                return;

            var currentInput = in_input.ToString();

            for (int i = 0; i < in_input.Length; i++)
                Console.Write("\b \b");

            in_input.Clear();

            _historyIndex = in_isDown
                ? Math.Min(_history.Count, _historyIndex + 1)
                : Math.Max(0, _historyIndex - 1);

            in_input.Append(_historyIndex < _history.Count ? _history[_historyIndex] : currentInput);

            Console.Write(in_input.ToString());
        }
    }
}
