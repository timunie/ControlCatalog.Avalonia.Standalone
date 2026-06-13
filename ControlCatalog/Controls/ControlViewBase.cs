using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ControlCatalog.Helper;
using ControlCatalog.ViewModels;
using DynamicData;
using DynamicData.Binding;

namespace ControlCatalog.Controls;

public class ControlViewBase : HeaderedContentControl
{
    private bool _ControlIsPropertyChanging = false;
    private bool _PropertyInfoIsChanging = false;

    private readonly SourceCache<PropertyInfoViewModel, string> _propertiesCache =
        new SourceCache<PropertyInfoViewModel, string>(p => p.Name);

    private IDisposable? _groupSubscription;
    private readonly SourceCache<PropertyGroupViewModel, string> _groupsCache =
        new SourceCache<PropertyGroupViewModel, string>(g => g.Key);

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

    public ControlViewBase()
    {
        // Stable group order. Dictionary preserves insertion order.
        var groupOrder = new string[]
        {
            "Layout", "Appearance", "Text", "Behavior", "Input", "Misc",
        };

        _groupSubscription = _propertiesCache.Connect()
            .Group(p => Categorize(p.Name))
            .OnItemAdded(group =>
            {
                var vm = new PropertyGroupViewModel(group.Key, BuildItems(group));
                _groupsCache.AddOrUpdate(vm);
            })
            .OnItemRemoved(group =>
            {
                _groupsCache.Remove(group.Key);
            })
            .Subscribe();

        _groupsCache.Connect()
            .Sort(SortExpressionComparer<PropertyGroupViewModel>.Ascending(g => Array.IndexOf(groupOrder, g.Key)))
            .Bind(out _propertyGroups)
            .Subscribe();
    }

    /// <summary>
    /// Gets the sample control
    /// </summary>
    private Control? SampleControl => Content as Control;

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

    private readonly ReadOnlyObservableCollection<PropertyGroupViewModel> _propertyGroups;

    private static ReadOnlyObservableCollection<PropertyInfoViewModel> BuildItems(
        IGroup<PropertyInfoViewModel, string, string> group)
    {
        ReadOnlyObservableCollection<PropertyInfoViewModel> result = null!;
        group.Cache
            .Connect()
            .Sort(SortExpressionComparer<PropertyInfoViewModel>.Ascending(p => p.Name))
            .Bind(out result)
            .Subscribe();
        return result;
    }

    private static string Categorize(string name) => name switch
    {
        "Width" or "Height" or "MinWidth" or "MinHeight"
            or "MaxWidth" or "MaxHeight" or "Margin" or "Padding"
            or "HorizontalAlignment" or "VerticalAlignment"
            or "HorizontalContentAlignment" or "VerticalContentAlignment"
            => "Layout",

        var n when n.EndsWith("Brush") || n.EndsWith("Color")
            => "Appearance",

        "Foreground" or "Background" or "BorderBrush"
            or "BorderThickness" or "CornerRadius" or "Opacity"
            or "ClipToBounds"
            => "Appearance",

        var n when n.StartsWith("Font") || n is "Text" or "Content"
            => "Text",
        "IsEnabled" or "IsVisible" or "IsHitTestVisible"
            or "IsFocused" or "IsPointerOver" or "IsTabStop"
            or "Focusable" or "IsSelected"
            => "Behavior",

        var n when n.EndsWith("Command") || n == "CommandParameter"
            => "Input",

        _ => "Misc",
    };

    public ReadOnlyObservableCollection<PropertyGroupViewModel> PropertyGroups => _propertyGroups;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupControlSample();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        foreach (var item in _propertiesCache.Items)
        {
            item.PropertyChanged -= PropertyInfo_OnPropertyChanged;
        }

        _groupSubscription?.Dispose();
        _propertiesCache.Clear();
        _groupsCache.Clear();
        Header = null;
        SubHeader = null;
    }

    private void SetupControlSample()
    {
        var control = Content as Control;
        if (control == null) return;

        control.PropertyChanged += Control_OnPropertyChanged;

        var controlType = control.GetType();

        if (Header == null)
        {
            Header = controlType.Name;
        }

        if (string.IsNullOrEmpty(SubHeader))
        {
            SubHeader = controlType.GetXmlSummary();
        }

        if (_propertiesCache.Count == 0)
        {
            var properties = controlType
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType.IsSubclassOf(typeof(AvaloniaProperty)))
                .Select(f => new PropertyInfoViewModel(f));

            foreach (var property in properties)
            {
                if (IsPropertyBlackListed(property.Name) || property.Property.IsReadOnly)
                    continue;

                _propertiesCache.AddOrUpdate(property);
                property.CurrentValue = control.GetValue(property.Property);

                property.PropertyChanged += PropertyInfo_OnPropertyChanged;
            }
        }
    }

    private void PropertyInfo_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_ControlIsPropertyChanging) return;


        if (e.PropertyName == nameof(PropertyInfoViewModel.CurrentValue) && sender is PropertyInfoViewModel propertyInfo)
        {
            try
            {
                _PropertyInfoIsChanging = true;
                SampleControl?.SetValue(propertyInfo.Property, propertyInfo.CurrentValue);
            }
            finally
            {
                _PropertyInfoIsChanging = false;
            }
        }
    }

    private void Control_OnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_PropertyInfoIsChanging)
            return;

        _ControlIsPropertyChanging = true;
        if (_propertiesCache.Lookup(change.Property.Name) is { HasValue: true } property)
        {
            property.Value.CurrentValue = change.NewValue;
        }

        _ControlIsPropertyChanging = false;
    }

    private bool IsPropertyBlackListed(string propertyName)
    {
        return PropertyBlackList.Contains(propertyName);
    }
}