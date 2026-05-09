using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Tradentum.Models;
using Tradentum.Services;

namespace Tradentum.ViewModels
{
    public class Page1ViewModel : ViewModelBase
    {
        private readonly IQuikService? _quikService;
        private readonly DispatcherTimer _timer;
        private readonly Dictionary<string, SecurityInfo> _instruments = new();
        private bool _isLoading;

        public ObservableCollection<SecurityInfo> TopSecurities { get; } = new();
        public ObservableCollection<SecurityInfo> TopGainers { get; } = new();
        public ObservableCollection<SecurityInfo> TopLosers { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public Page1ViewModel(IQuikService? quikService = null)
        {
            _quikService = quikService;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += async (s, e) => await LoadDataAsync();
            _timer.Start();
        }

        public async Task LoadDataAsync()
        {
            if (IsLoading || _quikService == null || !_quikService.IsConnected)
                return;

            IsLoading = true;

            try
            {
                var tickers = new[]
                {
                    ("SBER", "TQBR"), ("GAZP", "TQBR"), ("LKOH", "TQBR"),
                    ("YNDX", "TQBR"), ("VTBR", "TQBR"), ("ROSN", "TQBR"),
                    ("MGNT", "TQBR"), ("POLY", "TQBR"), ("ALRS", "TQBR"),
                    ("NVTK", "TQBR"), ("GMKN", "TQBR")
                };

                foreach (var (secCode, classCode) in tickers)
                {
                    if (!_instruments.ContainsKey(secCode))
                    {
                        _instruments[secCode] = new SecurityInfo
                        {
                            SecCode = secCode,
                            Name = secCode, // Имя подгрузится при первом обновлении
                            ClassCode = classCode
                        };
                    }

                    await UpdateSecurityAsync(_instruments[secCode]);
                }

                UpdateCollections();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // 🔹 Обновление 4 полей напрямую из QUIK
        private async Task UpdateSecurityAsync(SecurityInfo security)
        {
            try
            {
                var instrument = new QuikInstrument(_quikService!.Client, security.SecCode, security.ClassCode);

                System.Diagnostics.Debug.WriteLine($"🔄 Loading {security.SecCode}...");

                // 🔹 Получаем все 4 параметра параллельно
                var tasks = new[]
                {
                    instrument.GetPriceAsync(),
                    instrument.GetTurnoverAsync(),
                    instrument.GetChangeAsync(),
                    instrument.GetChangePercentAsync()
                };

                await Task.WhenAll(tasks);

                var price = tasks[0].Result;
                var turnover = tasks[1].Result;
                var change = tasks[2].Result;
                var changePercent = tasks[3].Result;

                System.Diagnostics.Debug.WriteLine(
                    $"✅ {security.SecCode}: Price={price}, Turnover={turnover}, Change={change}, ChangePercent={changePercent}");

                security.Price = price;
                security.Turnover = turnover;
                security.Change = change;
                security.ChangePercent = changePercent;
                security.LastUpdate = DateTime.Now;

                // 🔹 Подгружаем имя, если ещё не известно
                if (security.Name == security.SecCode)
                {
                    var secInfo = await _quikService.Client.Class.GetSecurityInfo(security.ClassCode, security.SecCode);
                    if (secInfo != null)
                    {
                        security.Name = secInfo.ShortName;
                        System.Diagnostics.Debug.WriteLine($"📛 {security.SecCode} name: {security.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Update {security.SecCode}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void UpdateCollections()
        {
            var all = _instruments.Values.ToList();

            // 🔹 Сортировка по обороту (VALTODAY)
            var byTurnover = all.OrderByDescending(x => x.Turnover).Take(20).ToList();
            SyncCollection(TopSecurities, byTurnover);

            // 🔹 Лидеры роста (PCHANGE > 0)
            var gainers = all.Where(x => x.ChangePercent > 0)
                .OrderByDescending(x => x.ChangePercent).Take(20).ToList();
            SyncCollection(TopGainers, gainers);

            // 🔹 Лидеры падения (PCHANGE < 0)
            var losers = all.Where(x => x.ChangePercent < 0)
                .OrderBy(x => x.ChangePercent).Take(20).ToList();
            SyncCollection(TopLosers, losers);
        }

        private void SyncCollection(ObservableCollection<SecurityInfo> target, List<SecurityInfo> source)
        {
            var ids = source.Select(x => x.SecCode).ToHashSet();

            for (int i = target.Count - 1; i >= 0; i--)
                if (!ids.Contains(target[i].SecCode)) target.RemoveAt(i);

            foreach (var item in source)
            {
                var existing = target.FirstOrDefault(x => x.SecCode == item.SecCode);
                if (existing != null)
                {
                    existing.Price = item.Price;
                    existing.Turnover = item.Turnover;
                    existing.Change = item.Change;
                    existing.ChangePercent = item.ChangePercent;
                    existing.LastUpdate = item.LastUpdate;
                    existing.Name = item.Name;
                }
                else
                {
                    target.Add(item);
                }
            }
        }

        public void Stop() => _timer?.Stop();
    }
}