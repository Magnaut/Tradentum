using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Tradentum.Converters
{
    /// <summary>
    /// Конвертер значений WPF для раскраски элементов в зависимости от знака числа.
    /// Используется для подсветки PnL, изменений цены, процентов доходности и т.д.
    /// </summary>
    [ValueConversion(typeof(decimal), typeof(Brush))]

    public class SignToBrushConverter : IValueConverter
    {
        /// <summary>Цвет для положительных значений.</summary>
        public Brush PositiveBrush { get; set; } = Brushes.LimeGreen;

        /// <summary>Цвет для отрицательных значений.</summary>
        public Brush NegativeBrush { get; set; } = Brushes.IndianRed;

        /// <summary>Цвет для нулевых значений.</summary>
        public Brush ZeroBrush { get; set; } = Brushes.Gray;

        /// <summary>
        /// Преобразует decimal в Brush на основе знака.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal dec)
            {
                if (dec > 0m) return PositiveBrush;
                if (dec < 0m) return NegativeBrush;
                return ZeroBrush;
            }

            // Поддержка nullable decimal и строкового парсинга на случай нестандартных биндингов
            if (value is double d)
            {
                if (d > 0) return PositiveBrush;
                if (d < 0) return NegativeBrush;
                return ZeroBrush;
            }

            if (value is string str && decimal.TryParse(str, out var parsedDec))
            {
                if (parsedDec > 0m) return PositiveBrush;
                if (parsedDec < 0m) return NegativeBrush;
                return ZeroBrush;
            }

            return Binding.DoNothing;
        }

        /// <summary>
        /// Обратное преобразование не поддерживается.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
