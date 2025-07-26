using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Echo.Helpers;

public class InverseBoolConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value; 
    }
}


public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = (bool)value;
        bool invert = parameter != null && parameter.ToString() == "False";
        return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (Visibility)value == Visibility.Visible;
    }
}


public class TokenisedFolderPathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null) return Binding.DoNothing;

        return ((string)value).Replace(@"\", " 🢒 ");

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class ProcessPathToSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var path = value as string;
        if (string.IsNullOrWhiteSpace(path))
            return "\uECAA"; // Default

        var lowerPath = path.ToLowerInvariant();

        // Web URL
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            return "\uE909";

        // Folder
        if (Directory.Exists(path))
            return "\uE8B7";

        // Autohotkey
        if (lowerPath.EndsWith(".ahk"))
            return "\uF000";

        // Batch/PowerShell/Command
        if (lowerPath.EndsWith(".bat") || lowerPath.EndsWith(".cmd") || lowerPath.EndsWith(".ps1"))
            return "\uE756";

        // Executable
        if (lowerPath.EndsWith(".exe"))
            return "\uE66C";

        // Default
        return "\uECAA";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}