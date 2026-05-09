using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Tradentum.Converters
{
    /// <summary>
    /// Конвертер с инвертированной логикой видимости.
    /// true  -> Collapsed (или Hidden, если настроено)
    /// false -> Visible
    /// Идеально подходит для отображения плейсхолдеров, пустых состояний и скрытия загрузчиков.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]

    public class BoolToInverseVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Значение Visibility, возвращаемое при true. По умолчанию Collapsed.
        /// </summary>
        public Visibility TrueVisibility { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// Значение Visibility, возвращаемое при false. По умолчанию Visible.
        /// </summary>
        public Visibility FalseVisibility { get; set; } = Visibility.Visible;

        /// <summary>
        /// Преобразует boolean в Visibility с инвертированной логикой.
        /// Безопасно обрабатывает null (считается false → Visible).
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = false;

            if (value == null)
            {
                isTrue = false;
            }
            else if (value is bool booleanValue)
            {
                isTrue = booleanValue;
            }
            else if (value.GetType() == typeof(bool?))
            {
                var nullableBool = (bool?)value;
                isTrue = nullableBool.HasValue && nullableBool.Value;
            }

            return isTrue ? TrueVisibility : FalseVisibility;
        }

        /// <summary>
        /// Обратное преобразование (Visibility → bool).
        /// Возвращает true, если текущее значение совпадает с TrueVisibility.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
                return vis == TrueVisibility;

            throw new NotSupportedException("ConvertBack не поддерживает типы, отличные от Visibility.");
        }
    }
}
