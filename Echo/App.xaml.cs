using Echo.Services;
using Echo.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using Wpf.Ui;

namespace Echo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{


    public static IHost AppHost { get; private set; }


    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {

                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IContentDialogService, ContentDialogService>();

                services.AddSingleton<ProcessLauncherService>();
                services.AddSingleton<ProcessMonitorService>();
                services.AddSingleton<FileSaveService>();
                services.AddSingleton<ModalService>();

                services.AddSingleton<MainWindow>();
                services.AddSingleton<ViewModels.MainViewModel>();
            })
            .Build();
    }


    public static readonly Mutex SingleInstanceMutex = new Mutex(true, @"Global\EchoAppSingleInstanceMutex");
    private CancellationTokenSource pipeServerCancellation = new();
    private Task pipeServerTask;

    protected override async void OnStartup(StartupEventArgs e)
    {

        if (!SingleInstanceMutex.WaitOne(0, false))
        {
            MessageBox.Show("Echo is already running.", "Echo", MessageBoxButton.OK, MessageBoxImage.Information);
            Current.Shutdown();
            return;
        }




        await AppHost.StartAsync();
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        base.OnStartup(e);

        if (Environment.GetCommandLineArgs().Contains("-tray"))
        {
            mainWindow.Hide();
            AppHost.Services.GetService<MainViewModel>()?.StartMonitoringCommand.Execute(null);
        }

    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        base.OnExit(e);
    }

}
