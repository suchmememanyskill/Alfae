using System;

namespace Launcher.Extensions
{
    public class CommandAttribute : Attribute
    {
        public string ButtonName { get; set; }

        public CommandAttribute(string buttonName) => ButtonName = buttonName;
    }
}