using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Soft_Givz.Data;
using Soft_Givz.Repositories;
using Soft_Givz.Services;
using Soft_Givz.ViewModels;
using Soft_Givz.Views;

namespace Soft_Givz;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Строка подключения 'DefaultConnection' не найдена в appsettings.json");

            var factory = new NpgsqlConnectionFactory(connStr);
            var doroga1Repo = new DorogaRepository(factory, "doroga1");
            var doroga2Repo = new DorogaRepository(factory, "doroga2");
            var mergeService = new MergeService(factory);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(doroga1Repo, doroga2Repo, mergeService)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}