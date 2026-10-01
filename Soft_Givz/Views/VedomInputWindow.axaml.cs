using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Soft_Givz.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soft_Givz.Views;

public partial class VedomInputWindow : Window
{
    private VedomInputViewModel? _subscribedVm;

    public VedomInputWindow()
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

        if (DataContext is VedomInputViewModel vm)
        {
            _subscribedVm = vm;
            vm.ConfirmSaveAnyway = ShowConfirmDialog;
            vm.ConfirmUnknownOtpravitel = ShowConfirmDialog;
        }
    }

    // ====== Навигация по стрелкам вверх/вниз и Enter ======
    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        // Если фокус на ComboBox с открытым списком — не перехватываем
        var focused = FocusManager?.GetFocusedElement();
        if (focused is ComboBox cb && cb.IsDropDownOpen)
            return;

        if (e.Key == Key.Down || e.Key == Key.Enter)
        {
            MoveFocusForward();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            MoveFocusBackward();
            e.Handled = true;
        }
    }

    private void MoveFocusForward()
    {
        var current = FocusManager?.GetFocusedElement() as Control;
        if (current == null) return;

        var all = GetFocusableControls();
        int idx = all.IndexOf(current);

        if (idx >= 0 && idx < all.Count - 1)
            all[idx + 1].Focus();
    }

    private void MoveFocusBackward()
    {
        var current = FocusManager?.GetFocusedElement() as Control;
        if (current == null) return;

        var all = GetFocusableControls();
        int idx = all.IndexOf(current);

        if (idx > 0)
            all[idx - 1].Focus();
    }

    // Возвращает список всех полей в порядке их появления в визуальном дереве
    private List<Control> GetFocusableControls()
    {
        var list = new List<Control>();
        CollectFocusable(this, list);
        return list;
    }

    private static void CollectFocusable(Visual parent, List<Control> list)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            if (child is TextBox tb && !tb.IsReadOnly)
                list.Add(tb);
            else if (child is ComboBox cb)
                list.Add(cb);

            CollectFocusable(child, list);
        }
    }

    // ====== Диалог Да/Нет ======
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