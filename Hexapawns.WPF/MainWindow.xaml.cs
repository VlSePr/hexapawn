using System.Windows;

namespace Hexapawns.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainViewModel();
        DataContext = viewModel;
        
        // Connect board control move requests to view model
        BoardControl.MoveRequested += viewModel.OnHumanMove;
    }
}