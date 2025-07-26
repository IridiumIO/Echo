using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Echo.Views;

[ObservableObject]
public partial class AddRippleDialog : ContentDialog
{

    [ObservableProperty] private string? _displayName;
    [ObservableProperty] private string? _rippleProcessPath;

    public AddRippleDialog(ContentPresenter contentPresenter) : base(contentPresenter)
    {
        InitializeComponent();
        DataContext = this;
    }


    [RelayCommand]    
    private void AddRipple()
    {
        var filedialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            Title = "Select a Process to Ripple"
        };
        if (filedialog.ShowDialog() == true)
        {
            DisplayName = System.IO.Path.GetFileNameWithoutExtension(filedialog.FileName);
            RippleProcessPath = filedialog.FileName;
        }

    }

}
