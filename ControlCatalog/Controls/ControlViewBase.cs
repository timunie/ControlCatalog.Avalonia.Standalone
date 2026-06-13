using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using ControlCatalog.Helper;
using PropertyInfo = ControlCatalog.ViewModels.PropertyInfo;

namespace ControlCatalog.Controls;

public class ControlViewBase : HeaderedContentControl
{

    private static readonly HashSet<string> PropertyBlackList =
    [
        "DataContext",
        "Parent",
        "TemplatedParent",
        "VisualParent",
        "LogicalParent",
        "IsVisible",
        "IsEnabled",
        "IsHitTestVisible",
        "IsFocused",
        "IsPointerOver",
        "IsPointerDirectlyOver",
        "Bounds",
        "RenderSize",
        "RenderTransform",
        "RenderTransformOrigin",
        "LayoutTransform",
        "LayoutTransformOrigin",
        "Theme",
    ];
    
    public static readonly StyledProperty<string?> SubHeaderProperty =
        AvaloniaProperty.Register<ControlViewBase, string?>(
            nameof(SubHeader));

    /// <summary>
    /// Gets or sets the subheader content of the control view. This property is used to display additional information or context about the control being showcased.
    /// </summary>
    public string? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    public ObservableCollection<PropertyInfo> Properties { get; } = new ObservableCollection<PropertyInfo>();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupControlSample();
    }

    private void SetupControlSample()
    {
        var control = Content as Control;
        if (control == null) return;

        var controlType = control.GetType();
        
        if (Header == null)
        {
            Header = controlType.Name;
        }

        if (string.IsNullOrEmpty(SubHeader))
        {
            SubHeader = controlType.GetXmlSummary();
        }
        
        if (Properties.Count == 0)
        {
            var properties = controlType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType.IsSubclassOf(typeof(AvaloniaProperty)))
                .Select(f => new PropertyInfo(f));

            foreach (var property in properties)
            {
                if (IsPropertyBlackListed(property.Name)) continue;
                
                Properties.Add(property);
                
                property.CurrentValue = control.GetValue(property.Property);
                
                control.Bind(
                    property.Property,
                    CompiledBinding.Create<PropertyInfo, object?>(
                        p => p.CurrentValue,
                        source: property,
                        mode: property.Property.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay));
            }
        }
    }

    private bool IsPropertyBlackListed(string propertyName)
    {
        return PropertyBlackList.Contains(propertyName);
    }
}