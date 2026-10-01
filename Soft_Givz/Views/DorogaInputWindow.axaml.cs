using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Soft_Givz.ViewModels;

namespace Soft_Givz.Views;

public partial class DorogaInputWindow : Window
{
    private DorogaInputViewModel? _subscribedVm;

    public DorogaInputWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_subscribedVm != null)
        {
            _subscribedVm.ConfirmSaveAnyway = null;
            _subscribedVm.ConfirmUnknownOtpravitel = null;
        }

        if (DataContext is DorogaInputViewModel vm)
        {
            _subscribedVm = vm;
            vm.ConfirmSaveAnyway = ShowConfirmDialog;
            vm.ConfirmUnknownOtpravitel = ShowConfirmDialog;
        }
    }

    private async Task<bool> ShowConfirmDialog(string message)
    {
        var result = false;

        var yesButton = new Button
        {
            Content = "Да",
            Padding = new Avalonia.Thickness(25, 8),
            MinWidth = 90
        };

        var noButton = new Button
        {
            Content = "Нет",
            Padding = new Avalonia.Thickness(25, 8),
            MinWidth = 90
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 15, 0, 0)
        };
        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);

        var panel = new StackPanel
        {
            Spacing = 10,
            Margin = new Avalonia.Thickness(20)
        };

        panel.Children.Add(new TextBlock
        {
            Text = "Предупреждение",
            FontWeight = FontWeight.Bold,
            FontSize = 16
        });

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 440
        });

        panel.Children.Add(buttonPanel);

        var win = new Window
        {
            Title = "Предупреждение",
            Width = 500,
            Height = 320,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = panel
        };

        yesButton.Click += (_, _) => { result = true; win.Close(); };
        noButton.Click += (_, _) => { result = false; win.Close(); };

        await win.ShowDialog(this);
        return result;
    }
}