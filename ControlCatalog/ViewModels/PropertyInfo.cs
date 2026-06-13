using System;
using System.Reflection;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlCatalog.Helper;

namespace ControlCatalog.ViewModels;

public partial class PropertyInfoViewModel : ViewModelBase
{
    public PropertyInfoViewModel(FieldInfo field)
    {
        if (typeof(AvaloniaProperty).IsAssignableFrom(field.FieldType))
        {
            Property = (AvaloniaProperty)field.GetValue(null)!;
            Name = Property.Name;
            Type = Property.PropertyType;
            Summary = field.DeclaringType?
                .GetProperty(Property.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)?
                .GetXmlSummary();
        }
        else
        {
            throw new ArgumentException("Specified field is not an AvaloniaProperty");
        }
    }

    public PropertyGroupViewModel? Parent { get; set; }
    
    public AvaloniaProperty Property { get; }
    public string Name { get; }
    public Type Type { get; }
    public string? Summary { get; }

    [ObservableProperty] public partial object? CurrentValue { get; set; }
}