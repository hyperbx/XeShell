namespace XeShell.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute(string in_name, string in_alias = "", Type[] in_inputs = null, Type[] in_optionalInputs = null) : Attribute
    {
        public string Name { get; set; } = in_name;
        public string Alias { get; set; } = in_alias;
        public Type[] Inputs { get; set; } = in_inputs;
        public Type[] OptionalInputs { get; set; } = in_optionalInputs;
    }
}
