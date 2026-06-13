using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using ControlCatalog.ViewModels;

namespace ControlCatalog.DataTemplateSelector;

public class PropertyInfoToDataTemplateSelector : IDataTemplate
{
    public static PropertyInfoToDataTemplateSelector Instance { get; } = new();

    public Control? Build(object? param)
    {
        if (param is PropertyInfo propertyInfo)
        {
            var targetType = propertyInfo.Type;
            var isReadOnly = propertyInfo.Property.IsReadOnly;
            var bindingMode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;

            // Explicit source for all bindings - don't rely on DataContext
            CompiledBinding CreateBinding() =>
                CompiledBinding.Create<PropertyInfo, object?>(
                    p => p.CurrentValue,
                    source: propertyInfo,
                    mode: bindingMode);

            if (targetType == typeof(bool))
            {
                return new ToggleSwitch()
                {
                    [!ToggleButton.IsCheckedProperty] = CreateBinding(),
                    IsEnabled = !isReadOnly
                };
            }

            if (targetType == typeof(double) || targetType == typeof(decimal))
            {
                return new NumericUpDown()
                {
                    [!NumericUpDown.ValueProperty] = CreateBinding(),
                    IsReadOnly = isReadOnly
                };
            }

            if (targetType == typeof(string))
            {
                return new TextBox()
                {
                    [!TextBox.TextProperty] = CreateBinding(),
                    IsReadOnly = isReadOnly
                };
            }

            if (targetType.IsEnum)
            {
                // Use GetValues so the bound value (enum) matches items in the list
                {
                    return new ComboBox()
                    {
                        ItemsSource = Enum.GetValues(targetType),
                        [!SelectingItemsControl.SelectedItemProperty] = CreateBinding(),
                        IsEnabled = !isReadOnly
                    };
                }
            }

            return new TextBlock()
            {
                [!TextBlock.TextProperty] = CreateBinding()
            };
        }

        return null;
    }

    public bool Match(object? data)
    {
        return data is PropertyInfo;
    }
}