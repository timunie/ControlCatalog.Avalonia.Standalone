using System;
using System.Reflection;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlCatalog.Helper;

namespace ControlCatalog.ViewModels;

public partial class PropertyInfo : ViewModelBase
{
    public PropertyInfo(FieldInfo field, AvaloniaObject owner)
    {
        if (typeof(AvaloniaProperty).IsAssignableFrom(field.FieldType))
        {
            Property = (AvaloniaProperty)field.GetValue(owner)!;
            Name = Property.Name;
            Type = Property.PropertyType;
            Summary = owner.GetType().GetProperty(Name)?.GetXmlSummary();
        }
        else
        {
            throw new ArgumentException("Specified field is not an AvaloniaProperty");
        }
    }

    public AvaloniaProperty Property { get; }
    public string Name { get; }
    public Type Type { get; }
    public string? Summary { get; }

    [ObservableProperty] 
    public partial object? CurrentValue { get; set; }
}