using System.Diagnostics;
using System.Text;
using XeSharp.Helpers;

namespace XeShell.Services
{
    public class PromptService
    {
        private const string _defaultPrompt = ">";
        private string _prompt { get; set; }

        /// <summary>
        /// The history of commands entered by the user.
        /// </summary>
        public static List<string> History { get; set; } = new(50);

        /// <summary>
        /// The index of the current element seeked to in <see cref="History"/>.
        /// </summary>
        public static int HistoryIndex { get; set; } = -1;

        /// <summary>
        /// The user input displayed to the console.
        /// </summary>
        public StringBuilder Input { get; set; } = new();

        /// <summary>
        /// The user input as typed by the user.
        /// </summary>
        public StringBuilder InputRaw { get; set; } = new();

        /// <summary>
        /// The index of the cursor relative to <see cref="Input"/>.
        /// </summary>
        public int InputIndex { get; set; } = 0;

        /// <summary>
        /// The cancellation token source for interrupting operations in other services.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();

        /// <summary>
        /// The column the cursor is on.
        /// </summary>
        public int Column
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

        /// <summary>
        /// The row the cursor is on.
        /// </summary>
        public int Row
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

        /// <summary>
        /// Shows the prompt.
        /// </summary>
        /// <typeparam name="T">The type to implicilty cast to.</typeparam>
        /// <param name="in_prompt">The prompt to display.</param>
        public T Show<T>(string in_prompt = "")
        {
            if (string.IsNullOrEmpty(in_prompt))
                in_prompt = _defaultPrompt;

            Console.Write(_prompt = in_prompt);

            CancellationTokenSource = new();

            return MemoryHelper.ChangeType<T>(ReadInput());
        }

        /// <summary>
        /// Shows the prompt.
        /// </summary>
        /// <param name="in_prompt">The prompt to display.</param>
        public string Show(string in_prompt = "")
        {
            return Show<string>(in_prompt);
        }

        private string ReadInput()
        {
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Backspace:
                    {
                        if (Input.Length <= 0 || InputIndex <= 0)
                            break;

                        CloneInputToRaw();
                        SeekCursor(-1);
                        SeekInput(-1);

                        Column++;

                        if (Input.Length > InputIndex)
                        {
                            Refresh(RemoveInput);
                            break;
                        }

                        Console.Write("\b \b");

                        break;
                    }

                    case ConsoleKey.Delete:
                    {
                        if (Input.Length <= InputIndex)
                            break;

                        CloneInputToRaw();

                        var oldColumn = Column;
                        Refresh(RemoveInput);
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
                        if (isLeft && InputIndex <= 0 || !isLeft && InputIndex == Input.Length)
                            break;

                        InputIndex = isLeft
                            ? InputIndex - 1
                            : InputIndex + 1;

                        SeekCursor(isLeft ? -1 : 1);

                        break;
                    }

                    default:
                    {
                        if (char.IsControl(keyInfo.KeyChar))
                            break;

                        AppendInput(keyInfo.KeyChar);
                        SeekInput();

                        if (Input.Length > InputIndex)
                        {
                            Refresh();
                            break;
                        }

                        Console.Write(keyInfo.KeyChar);

                        break;
                    }
                }
            }
            while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();

            var result = Input.ToString();

            if (!result.IsNullOrEmptyOrWhiteSpace())
            {
                History.Add(result);
                HistoryIndex = History.Count;
            }

            InputIndex = 0;

            CancellationTokenSource.Cancel();

            return result;
        }

        /// <summary>
        /// Refreshes the prompt.
        /// </summary>
        /// <param name="in_postClearCallback">An action to perform after clearing the prompt before rewriting it.</param>
        public void Refresh(Action in_postClearCallback = null)
        {
            ClearInput();

            if (in_postClearCallback != null)
                in_postClearCallback();

            Console.Write(Input.ToString());

            Column = _prompt.Length + InputIndex;
        }

        /// <summary>
        /// Clears the prompt.
        /// </summary>
        public void ClearInput()
        {
            Column = _prompt.Length + (Input.Length >= Console.BufferWidth
                ? Input.Length % Console.BufferWidth + 1
                : Input.Length);

            for (int i = 0; i < Input.Length; i++)
            {
                /* Back up to previous row once we've
                   reached the left edge of the console. */
                if (Column <= 0)
                    SeekCursor(-1);

                Console.Write("\b \b");
            }
        }

        /// <summary>
        /// Appends a character to the prompt at the current input index.
        /// </summary>
        /// <param name="in_input">The character to append.</param>
        public void AppendInput(char in_input)
        {
            Input.Insert(InputIndex, in_input);
            CloneInputToRaw();

#if DEBUG
            Debug.WriteLine("Input ──── : " + Input.ToString());
            Debug.WriteLine("InputRaw ─ : " + InputRaw.ToString());
#endif
        }

        /// <summary>
        /// Removes a character from the prompt at the current input index.
        /// </summary>
        public void RemoveInput()
        {
            Input.Remove(InputIndex, 1);
            InputRaw.Remove(InputIndex, 1);

#if DEBUG
            Debug.WriteLine("Input ──── : " + Input.ToString());
            Debug.WriteLine("InputRaw ─ : " + InputRaw.ToString());
#endif
        }

        /// <summary>
        /// Clones <see cref="Input"/> to <see cref="InputRaw"/>.
        /// </summary>
        public void CloneInputToRaw()
        {
            InputRaw = new(Input.ToString());
        }

        /// <summary>
        /// Seeks the cursor horizontally.
        /// </summary>
        /// <param name="in_amount">The amount of columns to seek by (negative numbers move backwards).</param>
        public void SeekCursor(int in_amount = 1)
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

        /// <summary>
        /// Seeks the input index.
        /// </summary>
        /// <param name="in_amount">The amount to seek by (negative numbers move backwards).</param>
        public void SeekInput(int in_amount = 1)
        {
            for (int i = 0; i < Math.Abs(in_amount); i++)
            {
                InputIndex = in_amount < 0
                    ? InputIndex - 1
                    : InputIndex + 1;
            }
        }

        /// <summary>
        /// Seeks to the next history element.
        /// </summary>
        /// <param name="in_isDown">Determines whether to seek to the next element down in the list.</param>
        public void SeekHistory(bool in_isDown = false)
        {
            if (History.Count <= 0)
                return;

            var currentInput = Input.ToString();

            ClearInput();

            Input.Clear();

            HistoryIndex = in_isDown
                ? Math.Min(History.Count, HistoryIndex + 1)
                : Math.Max(0, HistoryIndex - 1);

            Input.Append(HistoryIndex < History.Count ? History[HistoryIndex] : currentInput);
            InputIndex = Input.Length;

            Console.Write(Input.ToString());

            Column = _prompt.Length + Input.Length;
        }
    }
}
