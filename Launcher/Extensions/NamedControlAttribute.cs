using System;

namespace Launcher.Extensions
{
    public class NamedControlAttribute : Attribute
    {
        public string? Name { get; set; }

        public NamedControlAttribute()
        {
        }

        public NamedControlAttribute(string name) => Name = name;
    }
}