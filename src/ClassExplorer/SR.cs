using System.Globalization;

namespace ClassExplorer
{
    internal partial class SR
    {
        public static string Format(string format, object? arg0)
        {
            return string.Format(CultureInfo.CurrentCulture, format, arg0);
        }

        public static string Format(string format, object? arg0, object? arg1)
        {
            return string.Format(CultureInfo.CurrentCulture, format, arg0, arg1);
        }

        public static string Format(string format, object? arg0, object? arg1, object? arg2)
        {
            return string.Format(CultureInfo.CurrentCulture, format, arg0, arg1, arg2);
        }

        public static string Format(string format, params object?[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }
    }
}
