using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ControlCatalog.Views;

public partial class ControlViewBase : UserControl
{
    public ControlViewBase()
    {
        InitializeComponent();
    }

    protected override void OnDataContextEndUpdate()
    {
        base.OnDataContextEndUpdate();
        
        
    }
}