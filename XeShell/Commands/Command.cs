namespace XeShell.Commands
{
    public class Command(string in_invokedName, CommandAttribute in_attribute, Type in_type, List<object> in_inputs)
    {
        public string InvokedName { get; set; } = in_invokedName;
        public CommandAttribute Attribute { get; set; } = in_attribute;
        public Type Type { get; set; } = in_type;
        public List<object> Inputs { get; set; } = in_inputs;
    }
}
