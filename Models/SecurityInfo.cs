using System;
using Tradentum.ViewModels;

namespace Tradentum.Models
{
    public class SecurityInfo : ViewModelBase
    {
        private decimal _price;
        private decimal _turnover;
        private decimal _change;
        private decimal _changePercent;
        private DateTime _lastUpdate;

        public string SecCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;

        // 🔹 Цена последней сделки (LAST)
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); OnPropertyChanged(nameof(PriceDisplay)); }
        }

        // 🔹 Оборот в деньгах (VALTODAY)
        public decimal Turnover
        {
            get => _turnover;
            set { _turnover = value; OnPropertyChanged(); OnPropertyChanged(nameof(TurnoverDisplay)); }
        }

        // 🔹 Изменение к предыдущей сессии (CHANGE) — из QUIK
        public decimal Change
        {
            get => _change;
            set { _change = value; OnPropertyChanged(); OnPropertyChanged(nameof(ChangeDisplay)); }
        }

        // 🔹 % изменения (PCHANGE) — из QUIK
        public decimal ChangePercent
        {
            get => _changePercent;
            set { _changePercent = value; OnPropertyChanged(); OnPropertyChanged(nameof(ChangePercentDisplay)); }
        }

        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set { _lastUpdate = value; OnPropertyChanged(); OnPropertyChanged(nameof(LastUpdateDisplay)); }
        }

        // 🔹 Форматированные строки для XAML
        public string PriceDisplay => Price.ToString("N2");
        public string TurnoverDisplay => Turnover.ToString("N0");
        public string ChangeDisplay => Change >= 0 ? $"+{Change:N2}" : $"{Change:N2}";
        public string ChangePercentDisplay => ChangePercent >= 0 ? $"+{ChangePercent:N2}%" : $"{ChangePercent:N2}%";
        public string LastUpdateDisplay => LastUpdate.ToString("HH:mm:ss");
    }
}