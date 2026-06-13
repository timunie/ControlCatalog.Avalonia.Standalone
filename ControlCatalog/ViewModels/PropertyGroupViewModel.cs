using System.Collections.ObjectModel;

namespace ControlCatalog.ViewModels;

public sealed class PropertyGroupViewModel
{
    public PropertyGroupViewModel(string key, ReadOnlyObservableCollection<PropertyInfoViewModel> items)
    {
        Key = key;
        Items = items;
    }

    public string Key { get; }
    public ReadOnlyObservableCollection<PropertyInfoViewModel> Items { get; }
}