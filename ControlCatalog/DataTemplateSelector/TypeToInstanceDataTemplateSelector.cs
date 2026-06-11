using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;
using ControlCatalog.ViewModels;

namespace ControlCatalog.DataTemplateSelector;

public class TypeToInstanceDataTemplateSelector : IDataTemplate
{
    public static TypeToInstanceDataTemplateSelector Instance { get; } = new TypeToInstanceDataTemplateSelector();

    public Control? Build(object? param)
    {
        if (param is ControlViewModelBase controlViewModel)
        {
            var control = Activator.CreateInstance(controlViewModel.ControlType) as Control;

            if (control is null)
            {
                return new TextBlock()
                {
                    Text = $"Could not create instance of type {controlViewModel.ControlType.FullName}"
                };
            }
            
            // Init the property bindings only after control is attached
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var property in controlViewModel.Properties)
                {
                    property.CurrentValue = control.GetValue(property.Property);

                    control.Bind(
                        property.Property,
                        CompiledBinding.Create<PropertyInfo, object?>(
                            p => p.CurrentValue,
                            source: property,
                            mode: property.Property.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay));
                }
            }, DispatcherPriority.Loaded);
            
            return control;
        }
        else
        {
            throw new ArgumentException($"Type {param?.GetType()} not supported", nameof(param));
        }
    }

    public bool Match(object? data)
    {
        return data is ControlViewModelBase;
    }
}