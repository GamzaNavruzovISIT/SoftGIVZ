using System.Collections.Generic;
using System.ComponentModel;

namespace Soft_Givz.Models;

public class DbRow : INotifyPropertyChanged
{
    private readonly Dictionary<string, object?> _values = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public object? this[string column]
    {
        get => _values.TryGetValue(column, out var v) ? v : null;
        set
        {
            if (_values.TryGetValue(column, out var old) && Equals(old, value))
                return;

            _values[column] = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(column));
        }
    }

    public IEnumerable<string> Columns => _values.Keys;
}