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

            if (propertyInfo.Type == typeof(bool))
            {
                return new ToggleSwitch()
                {
                    [!ToggleButton.IsCheckedProperty] = CompiledBinding.Create<PropertyInfo, object?>(
                        p => p.CurrentValue,
                        bindingMode),
                    IsEnabled = !isReadOnly
                };
            }

            if (propertyInfo.Type is var t2 && (t2 == typeof(double) || t2 == typeof(decimal)))
            {
                return new NumericUpDown()
                {
                    [!NumericUpDown.ValueProperty] =
                        CompiledBinding.Create<PropertyInfo, object?>(
                            p => p.CurrentValue,
                            source: propertyInfo,
                            mode: bindingMode),
                    IsReadOnly = isReadOnly
                };
            }

            if (propertyInfo.Type == typeof(string))
            {
                return new TextBox()
                {
                    [!TextBox.TextProperty] = CompiledBinding.Create<PropertyInfo, object?>(
                        p => p.CurrentValue,
                        source: propertyInfo,
                        mode: bindingMode),
                    IsReadOnly = isReadOnly
                };
            }

            if (targetType.IsEnum)
            {
                return new ComboBox()
                {
                    ItemsSource = Enum.GetNames(targetType),
                    [!SelectingItemsControl.SelectedItemProperty] = CompiledBinding.Create<PropertyInfo, object?>(
                        p => p.CurrentValue,
                        source: propertyInfo,
                        mode: bindingMode),
                    IsEnabled = !isReadOnly
                };
            }
            
            return new TextBlock()
            {
                [!TextBlock.TextProperty] =
                    CompiledBinding.Create<PropertyInfo, object?>(p => p.CurrentValue)
            };
        }

        return null;
    }

    public bool Match(object? data)
    {
        return data is PropertyInfo;
    }
}