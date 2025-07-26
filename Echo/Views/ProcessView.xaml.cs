using Echo.ViewModels;
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

namespace Echo.Views;

public partial class ProcessView : UserControl
{
    public ProcessView()
    {
        InitializeComponent();
    }

    private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ProcessViewModel vm)
            vm.IsEditingName = true;
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProcessViewModel vm)
            vm.IsEditingName = false;
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter)
        {
            if (DataContext is ProcessViewModel vm)
                vm.IsEditingName = false;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProcessViewModel vm)
            vm.IsEditingName = true;
    }
}
