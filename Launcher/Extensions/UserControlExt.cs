using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Launcher.Utils;

namespace Launcher.Extensions
{
    public abstract class UserControlExt<T> : UserControl
    {
        private List<BindingAttribute> bindings = new();

        public void Init()
        {
            AvaloniaXamlLoader.Load(this);
            SetControls();
        }

        public void SetControls()
        {
            bindings = new();
            Dictionary<string, Button> foundButtons = new();
            Dictionary<string, object> foundControls = new();
            
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                foreach (var customAttribute in propertyInfo.GetCustomAttributes(true))
                {
                    if (customAttribute is NamedControlAttribute attribute)
                    {
                        string? name = attribute.Name;
                        if (name == null)
                            name = propertyInfo.Name;
                        
                        object value = this.FindNameScope().Find(name);
                        foundControls.Add(name, value);
                        if (value is Button button)
                            foundButtons.Add(name, button);
                        
                        propertyInfo.SetValue(this, value);
                        break;
                    }
                    
                    if (customAttribute is BindingAttribute bindingAttribute)
                    {
                        if (!foundControls.ContainsKey(bindingAttribute.ControlName))
                        {
                            object value = this.FindNameScope().Find(bindingAttribute.ControlName);
                            foundControls.Add(bindingAttribute.ControlName, value);
                        }

                        bindingAttribute.Control = foundControls[bindingAttribute.ControlName];
                        bindingAttribute.ControlField = bindingAttribute.Control.GetType().GetProperty(bindingAttribute.FieldName)!;
                        bindingAttribute.Instance = this;
                        bindingAttribute.AttachedField = propertyInfo;
                        bindings.Add(bindingAttribute);
                    }
                }
            }

            foreach (var methodInfo in typeof(T).GetMethods())
            {
                foreach (var customAttribute in methodInfo.GetCustomAttributes(true))
                {
                    if (customAttribute is CommandAttribute attribute)
                    {
                        Button button;
                        if (foundButtons.ContainsKey(attribute.ButtonName))
                            button = foundButtons[attribute.ButtonName];
                        else
                        {
                            object value = this.FindNameScope().Find(attribute.ButtonName);
                            if (value is Button foundButton)
                                button = foundButton;
                            else
                                throw new InvalidDataException("Name is not a button");
                        }

                        if (methodInfo.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null)
                            button.Command = new LambdaCommand(x =>
                                Dispatcher.UIThread.Post(() => methodInfo.Invoke(this, Array.Empty<object?>())));
                        else
                            button.Command = new LambdaCommand(x => methodInfo.Invoke(this, Array.Empty<object?>()));
                    }
                }
            }
        }

        public void UpdateView() => bindings.ForEach(x => x.Set());
    }
}