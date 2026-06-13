using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ControlCatalog.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] 
    public partial ControlViewModelBase SelectedControlSample { get; set; } = new(typeof(ToggleSwitch));
}