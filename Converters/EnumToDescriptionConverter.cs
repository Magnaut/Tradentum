using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Tradentum.Converters
{
    /// <summary>
    /// Конвертер для отображения значений enum в читаемом виде.
    /// Использует атрибут [Description]. Если атрибут отсутствует, возвращает имя элемента enum.
    /// Поддерживает двустороннее преобразование (для ComboBox, RadioButtons и т.д.).
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(string))]

    public class EnumToDescriptionConverter : IValueConverter
    {
        // Потокобезопасный кэш описаний. Инициализируется при первом обращении.
        private static readonly ConcurrentDictionary<Enum, string> _descriptionCache = new();

        /// <summary>
        /// Преобразует enum в строку.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                // GetOrAdd гарантирует, что рефлексия вызовется только один раз на каждый элемент enum
                return _descriptionCache.GetOrAdd(enumValue, GetDescription);
            }
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Обратное преобразование: строка → enum.
        /// Ищет по атрибуту [Description] или по имени элемента.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && targetType.IsEnum)
            {
                foreach (var field in targetType.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var enumVal = (Enum)field.GetValue(null);
                    var desc = _descriptionCache.GetOrAdd(enumVal, GetDescription);

                    if (desc.Equals(str, StringComparison.OrdinalIgnoreCase))
                    {
                        return enumVal;
                    }
                }

                // Fallback: попытка парсинга по имени enum
                try
                {
                    return Enum.Parse(targetType, str, true);
                }
                catch
                {
                    return System.Windows.DependencyProperty.UnsetValue;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Извлекает описание из атрибута или возвращает имя поля.
        /// </summary>
        private static string GetDescription(Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attr = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? enumValue.ToString();
        }
    }
}
