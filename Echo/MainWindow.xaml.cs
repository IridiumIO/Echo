using Echo.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace Echo;

public partial class MainWindow : INavigationWindow
{
    public MainWindow(MainViewModel mainViewModel, IContentDialogService contentDialogService)
    {
        DataContext = mainViewModel;
        
        InitializeComponent();

        NotifyIconTrayMenu.DataContext = mainViewModel;
        contentDialogService.SetDialogHost(RootContentDialog);
    }

    public void CloseWindow()
    {
        throw new NotImplementedException();
    }

    public INavigationView GetNavigation()
    {
        throw new NotImplementedException();
    }

    public bool Navigate(Type pageType)
    {
        throw new NotImplementedException();
    }

    public void SetPageService(INavigationViewPageProvider navigationViewPageProvider)
    {
        throw new NotImplementedException();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }

    public void ShowWindow()
    {
        throw new NotImplementedException();
    }

    private void FluentWindow_Closing(object sender, CancelEventArgs e)
    {

        if(Keyboard.Modifiers == ModifierKeys.Shift)
        {
            e.Cancel = false;
            Application.Current.Shutdown();
        }

        this.Hide();
        e.Cancel = true; // Prevent the window from closing
    }
}