using Spectre.Console;

namespace XeShell.Helpers
{
    public class ConsoleHelper
    {
        private static readonly ProgressColumn[] _defaultProgressColumns =
        [
            new SpinnerColumn(),
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new RemainingTimeColumn()
        ];

        public static T StatusCommon<T>(string in_status, Func<StatusContext, T> in_action)
        {
            return AnsiConsole.Status().Spinner(Spinner.Known.Line).Start(in_status, in_action);
        }

        public static void StatusCommon(string in_status, Action<StatusContext> in_action)
        {
            AnsiConsole.Status().Spinner(Spinner.Known.Line).Start(in_status, in_action);
        }

        public static T ProgressCommon<T>(Func<ProgressContext, T> in_action)
        {
            return AnsiConsole.Progress().Columns(_defaultProgressColumns).Start(in_action);
        }

        public static void ProgressCommon(Action<ProgressContext> in_action)
        {
            AnsiConsole.Progress().Columns(_defaultProgressColumns).Start(in_action);
        }
    }
}
