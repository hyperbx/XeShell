﻿using System.Reflection;
using XeSharp.Device;
using XeSharp.Helpers;
using XeSharp.Logger;

// https://github.com/thesupersonic16/HedgeModManager/blob/rewrite/HedgeModManager/CLI/CommandLine.cs

namespace XeShell.Commands
{
    public class CommandProcessor
    {
        public static Dictionary<CommandAttribute, Type> Commands = [];

        static CommandProcessor()
        {
            RegisterCommands(Assembly.GetExecutingAssembly());
        }

        public static void RegisterCommands(Assembly in_assembly)
        {
            var types = in_assembly.GetTypes().Where(t => typeof(ICommand).IsAssignableFrom(t));

            if (types == null)
                return;

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<CommandAttribute>();

                if (attr == null)
                    continue;

                Commands[attr] = type;
            }
        }

        public static List<Command> ParseArguments(string[] in_args)
        {
            var commands = new List<Command>();

            for (int i = 0; i < in_args.Length; i++)
            {
                var command = Commands.FirstOrDefault(x => x.Key.Name == in_args[i]);

                if (command.Key == null)
                {
                    command = Commands.FirstOrDefault(x => x.Key.Alias == in_args[i]);

                    if (command.Key == null)
                        return commands;
                }

                if (in_args.Length <= command.Key.Inputs?.Length + i)
                {
                    XeLogger.Error($"Error: too few inputs for command \"{command.Key.Name}\"");
                    break;
                }

                var inputs = new List<object>();

                List<object> ResolveInputs(Type[] in_inputTypes)
                {
                    var result = new List<object>();

                    foreach (var input in in_inputTypes ?? [])
                    {
                        i++;

                        if (in_args.Length <= i)
                            return result;

                        var data = ParseDataFromString(Type.GetTypeCode(input), in_args[i]);

                        if (data != null)
                        {
                            result.Add(data);
                        }
                        else
                        {
                            XeLogger.Error($"Error: unknown type \"{input.Name}\" for command \"{command.Key.Name}\".");
                        }
                    }

                    return result;
                }

                inputs.AddRange(ResolveInputs(command.Key.Inputs));
                inputs.AddRange(ResolveInputs(command.Key.OptionalInputs));

                commands.Add(new Command(in_args[0], command.Key, command.Value, inputs));
            }

            return commands;
        }

        public static List<Command> ParseArguments(string in_args)
        {
            return ParseArguments(StringHelper.ParseArgs(in_args));
        }

        public static object ParseDataFromString(TypeCode in_typeCode, string in_data)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return in_typeCode switch
            {
                TypeCode.String  => in_data,
                TypeCode.Int32   => in_data.StartsWith("0x") ? Convert.ToInt32(in_data, 16) : int.Parse(in_data),
                TypeCode.UInt32  => in_data.StartsWith("0x") ? Convert.ToUInt32(in_data, 16) : uint.Parse(in_data),
                TypeCode.Boolean => bool.Parse(in_data),
                _                => null,
            };
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static bool ExecuteArguments(string in_args, XeConsole in_console)
        {
            var result = false;
            var args = StringHelper.ParseArgs(in_args);
            var commands = ParseArguments(args);

            foreach (var command in Commands)
            {
                var response = (Activator.CreateInstance(command.Value) as ICommand)?.ExecuteRaw(args, in_args, in_console);

                switch (response)
                {
                    case ECommandResponse.Success:
                        result = true;
                        break;

                    case ECommandResponse.RerouteToBase:
                        return false;
                }
            }

            if (commands.Count <= 0)
                return result;

            foreach (var command in commands)
            {
                (Activator.CreateInstance(command.Type) as ICommand)?.Execute(commands, command, in_console);
                result = true;
            }

            return result;
        }

        public static void DisposeCommands()
        {
            foreach (var command in Commands)
                (Activator.CreateInstance(command.Value) as ICommand)?.Dispose();
        }
    }
}
