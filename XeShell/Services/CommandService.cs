using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;
using XeShell.Commands;
using XeShell.Exceptions;

namespace XeShell.Services
{
    public class CommandService(XeConsole in_console)
    {
        protected XeConsole _console = in_console;

        private static List<string> _exitCommands =
        [
            "bye",
            "magicboot cold",
            "shutdown"
        ];

        public EPromptResponse PromptCommand(string in_prompt, bool in_isOverrideBaseCommands = true)
        {
            var prompt   = new PromptService();
            var autofill = new AutofillService(_console, prompt);

            var input = prompt.Show(in_prompt);

            if (input.IsNullOrEmptyOrWhiteSpace())
                return EPromptResponse.Error;

            try
            {
                Console.CancelKeyPress += Console_CancelKeyPress;

                SendCommand(input, in_isOverrideBaseCommands);

                if (_exitCommands.Contains(input.ToLower()))
                    return EPromptResponse.ExitCallback;

                return EPromptResponse.Error;
            }
            catch (UnknownCommandException out_ex)
            {
                XeLogger.Error(out_ex.Message);
            }
#if !DEBUG
            catch (Exception out_ex)
            {
                XeLogger.Error($"An internal error occurred.\n{out_ex}");
            }
#endif
            finally
            {
                Console.CancelKeyPress -= Console_CancelKeyPress;
            }

            return EPromptResponse.Error;
        }

        public void SendCommand(string in_command, bool in_isOverrideBaseCommands = true)
        {
            if (in_isOverrideBaseCommands && CommandProcessor.ExecuteArguments(in_command, _console))
                return;

            var response = _console.Client.SendCommand(in_command, false)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "The server response returned null.");

            if (response.Results?.Length > 0)
            {
                foreach (var result in response.Results)
                    XeLogger.Log(result);
            }
            else
            {
                bool isMessage = !string.IsNullOrEmpty(response.Message);

                if (response.Status.IsFailed())
                {
                    if (response.Status.ToHResult() == XeSharp.Net.EXeStatusCode.XBDM_INVALIDCMD)
                        throw new UnknownCommandException(in_command.Split(' ')[0]);

                    XeLogger.Error(isMessage ? response.Message : response.Status.ToString());
                }
                else if (isMessage)
                {
                    XeLogger.Log(response.Message);
                }
            }
        }

        private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _console.Client.Cancel();
        }
    }
}
