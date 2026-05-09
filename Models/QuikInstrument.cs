using System;
using System.Threading.Tasks;
using QuikSharp;
using QuikSharp.DataStructures;

namespace Tradentum.Models
{
    public class QuikInstrument
    {
        private readonly Quik _quik;
        private readonly char _separator;

        public string SecurityCode { get; }
        public string ClassCode { get; }

        public QuikInstrument(Quik quik, string securityCode, string classCode)
        {
            _quik = quik ?? throw new ArgumentNullException(nameof(quik));
            SecurityCode = securityCode;
            ClassCode = classCode;

            var culture = System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU");
            _separator = culture.NumberFormat.NumberDecimalSeparator[0];
        }

        // 🔹 Цена последней сделки (LAST)
        public async Task<decimal> GetPriceAsync() =>
            await ParseParamAsync(ParamNames.LAST);

        // 🔹 Оборот в деньгах (VALTODAY)
        public async Task<decimal> GetTurnoverAsync() =>
            await ParseParamAsync(ParamNames.VALTODAY);

        // 🔹 Изменение к предыдущей сессии (CHANGE)
        public async Task<decimal> GetChangeAsync() =>
            await ParseParamAsync(ParamNames.CHANGE);

        // 🔹 % изменения (LASTCHANGE вместо PCHANGE)
        public async Task<decimal> GetChangePercentAsync() =>
            await ParseParamAsync(ParamNames.LASTCHANGE);

        // 🔹 Универсальный парсер
        private async Task<decimal> ParseParamAsync(ParamNames paramName)
        {
            try
            {
                var param = await _quik.Trading.GetParamEx(ClassCode, SecurityCode, paramName);

                if (!string.IsNullOrEmpty(param?.ParamValue))
                {
                    var normalized = param.ParamValue.Replace(" ", "").Replace('.', ',');
                    return decimal.Parse(normalized,
                        System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {SecurityCode}.{paramName}: {ex.Message}");
            }
            return 0;
        }
    }
}