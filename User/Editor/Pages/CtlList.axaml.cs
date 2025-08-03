using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Profiler.Pages;

internal class DGModel
{
    public string Input { get; set; }
    public string[] SubMode { get; set; } = new string[8];
}

public partial class Info1 : UserControl
{
    private DGModel Modes { get; set; }

    public Info1()
    {
        InitializeComponent();
    }
}