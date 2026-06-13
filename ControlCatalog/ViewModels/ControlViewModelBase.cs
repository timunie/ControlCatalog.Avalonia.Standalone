using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlCatalog.Helper;

namespace ControlCatalog.ViewModels;

/// <summary>
/// Just a sample
/// </summary>
public partial class ControlViewModelBase : ViewModelBase
{
    public ControlViewModelBase(Type controlType)
    {
        ControlType = controlType;
        
        Title = ControlType.Name;
        Description = ControlType.GetXmlSummary();
        
        Properties = ControlType.GetFields(BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType.IsSubclassOf(typeof(AvaloniaProperty)))
            .Select(f => new PropertyInfoViewModel(f))
            .ToArray();
    }
    
    [ObservableProperty] 
    public partial string? Title {get; set;}
    
    [ObservableProperty]
    public partial string? Description { get; set; } = "Don't use this control, it is just a test as of now";
    
    public Type ControlType { get; }
    
    public PropertyInfoViewModel[] Properties { get; }
}