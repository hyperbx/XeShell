using System.Text;
using XeSharp.Helpers;

namespace XeShell
{
    public class Prompt
    {
        private const string _defaultPrompt = "> ";
        private static string _prompt;

        private static readonly List<string> _history = new(50);
        private static int _historyIndex = -1;

        private static StringBuilder _input;
        private static int _inputIndex = 0;

        public static int Column
        {
            get => Console.GetCursorPosition().Left;

            set
            {
                if (value >= Console.BufferWidth)
                {
                    // Get remaining columns from wrapped text.
                    var remainder = value % Console.BufferWidth;

                    value = remainder <= 0
                        ? Console.BufferWidth - 1
                        : remainder;
                }

                Console.SetCursorPosition(value, Row);
            }
        }

        public static int Row
        {
            get => Console.GetCursorPosition().Top;

            set
            {
                if (value >= Console.BufferHeight)
                {
                    var diff = value - Console.BufferHeight;

                    try
                    {
                        // Resize console buffer height to account for wrapped text.
                        Console.MoveBufferArea(Column, Row, Console.BufferWidth, Console.BufferHeight, Column, Row - diff);
                        return;
                    }
                    catch
                    {
                        // Just in case Console.MoveBufferArea has a heart attack.
                        for (int i = 0; i < diff; i++)
                            Console.WriteLine();

                        value = Console.BufferHeight - 1;
                    }
                }

                Console.SetCursorPosition(Console.GetCursorPosition().Left, value);
            }
        }

        public static T Show<T>(string in_prompt = "")
        {
            if (string.IsNullOrEmpty(in_prompt))
                in_prompt = _defaultPrompt;

            if (!in_prompt.EndsWith(' '))
                in_prompt += " ";

            Console.Write(_prompt = in_prompt);

            return MemoryHelper.ChangeType<T>(ReadInput());
        }

        public static string Show(string in_prompt = "")
        {
            return Show<string>(in_prompt);
        }

        private static string ReadInput()
        {
            _input = new StringBuilder();

            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Backspace:
                    {
                        if (_input.Length <= 0)
                            break;

                        SeekCursor(-1);
                        SeekInput(-1);

                        Column++;

                        _input.Remove(_inputIndex, 1);

                        Console.Write("\b \b");

                        break;
                    }

                    case ConsoleKey.Delete:
                    {
                        if (_input.Length <= _inputIndex)
                            break;

                        var oldColumn = Column;

                        ClearInput();

                        _input.Remove(_inputIndex, 1);

                        Console.Write(_input.ToString());

                        Column = oldColumn;

                        break;
                    }

                    case ConsoleKey.UpArrow:
                    case ConsoleKey.DownArrow:
                        SeekHistory(keyInfo.Key == ConsoleKey.DownArrow);
                        break;

                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                    {
                        var isLeft = keyInfo.Key == ConsoleKey.LeftArrow;

                        // Keep cursor within the boundaries of the input string.
                        if ((isLeft && _inputIndex <= 0) || (!isLeft && _inputIndex == _input.Length))
                            break;

                        _inputIndex = isLeft
                            ? _inputIndex - 1
                            : _inputIndex + 1;

                        SeekCursor(isLeft ? -1 : 1);

                        break;
                    }

                    default:
                    {
                        if (char.IsControl(keyInfo.KeyChar))
                            break;

                        _input.Insert(_inputIndex, keyInfo.KeyChar);

                        SeekInput();

                        // Rewrite prompt if we're in the middle of the input.
                        // FIXME: this causes flickering. ¯\_(ツ)_/¯
                        if (_input.Length > _inputIndex)
                        {
                            ClearInput();

                            Console.Write(_input.ToString());

                            Column = _prompt.Length + _inputIndex;

                            continue;
                        }

                        Console.Write(keyInfo.KeyChar);

                        break;
                    }
                }
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
            Column = _prompt.Length + (_input.Length >= Console.BufferWidth
                ? _input.Length % Console.BufferWidth + 1
                : _input.Length);

            for (int i = 0; i < _input.Length; i++)
            {
                /* Back up to previous row once we've
                   reached the left edge of the console. */
                if (Column <= 0)
                    SeekCursor(-1);

                Console.Write("\b \b");
            }
        }

        private static void SeekCursor(int in_amount = 1)
        {
            if (in_amount == 0)
                return;

            for (int i = 0; i < Math.Abs(in_amount); i++)
            {
                var isLeft = in_amount < 0;
                var (column, row) = Console.GetCursorPosition();

                if (isLeft)
                {
                    column--;

                    if (column < 0)
                    {
                        column = Console.BufferWidth - 1;
                        row -= 1;
                    }
                }
                else
                {
                    column++;

                    if (column >= Console.BufferWidth)
                    {
                        column = 0;
                        row += 1;
                    }
                }

                Console.SetCursorPosition(column, row);
            }
        }

        private static void SeekInput(int in_amount = 1)
        {
            for (int i = 0; i < Math.Abs(in_amount); i++)
            {
                _inputIndex = in_amount < 0
                    ? _inputIndex - 1
                    : _inputIndex + 1;
            }
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

            Column = _prompt.Length + _input.Length;
        }
    }
}
