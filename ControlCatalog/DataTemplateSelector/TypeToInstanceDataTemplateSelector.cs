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

            control.DataContext = controlViewModel;
            control.AttachedToVisualTree += Contol_OnAttachedToVisualTree;
            return control;
        }
        return null;
    }

    private void Contol_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control {DataContext: ControlViewModelBase controlViewModel} control)
        {
            control.AttachedToVisualTree -= Contol_OnAttachedToVisualTree;
            
            foreach (var property in controlViewModel.Properties)
            {
                property.CurrentValue = control.GetValue(property.Property);

                control.Bind(
                    property.Property,
                    CompiledBinding.Create<PropertyInfoViewModel, object?>(
                        p => p.CurrentValue,
                        source: property,
                        mode: property.Property.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay));
            }
        }
    }

    public bool Match(object? data)
    {
        return data is ControlViewModelBase;
    }
}