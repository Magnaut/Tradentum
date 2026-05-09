using Tradentum.Services;
using Tradentum.ViewModels;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tradentum.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IQuikService _quikService;
        private bool _isConnected;
        private string _statusMessage = "Не подключено";
        private ViewModelBase? _currentPageViewModel;

        // 🔹 Статус подключения
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                UpdateStatusMessage();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // 🔹 Текущая страница (для ContentControl)
        public ViewModelBase? CurrentPageViewModel
        {
            get => _currentPageViewModel;
            set { _currentPageViewModel = value; OnPropertyChanged(); }
        }

        // 🔹 Экземпляры страниц (создаём один раз)
        public Page1ViewModel Page1VM { get; }
        public Page2ViewModel Page2VM { get; }

        // 🔹 Команды
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand NavigateToPage1Command { get; }
        public ICommand NavigateToPage2Command { get; }
        public ICommand ExitCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AboutCommand { get; }

        public MainViewModel(IQuikService? quikService = null)
        {
            _quikService = quikService ?? new QuikService();

            // 🔹 Создаём страницы (с тем же сервисом для Page1)
            Page1VM = new Page1ViewModel(_quikService);
            Page2VM = new Page2ViewModel();

            // 🔹 Команды навигации
            NavigateToPage1Command = new RelayCommand(_ => CurrentPageViewModel = Page1VM);
            NavigateToPage2Command = new RelayCommand(_ => CurrentPageViewModel = Page2VM);

            // 🔹 Остальные команды
            ConnectCommand = new RelayCommand(async _ => await ToggleConnectionAsync());
            DisconnectCommand = new RelayCommand(_ => Disconnect());
            ExitCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            AboutCommand = new RelayCommand(_ => ShowAbout());

            // 🔹 Страница по умолчанию
            CurrentPageViewModel = Page1VM;
        }

        private void UpdateStatusMessage()
        {
            StatusMessage = IsConnected ? "✅ Подключено к QUIK" : "❌ Отключено";
        }

        /// <summary>
        /// Переключить состояние подключения (подключить/отключить)
        /// </summary>
        private async Task ToggleConnectionAsync()
        {
            System.Diagnostics.Debug.WriteLine($"🔘 ToggleConnectionAsync вызван. IsConnected = {IsConnected}");

            if (IsConnected)
            {
                System.Diagnostics.Debug.WriteLine("🔌 Выполняем отключение...");
                Disconnect();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔗 Выполняем подключение...");
                try
                {
                    StatusMessage = "⏳ Подключение...";
                    bool connected = await _quikService.ConnectAsync();
                    IsConnected = connected;
                    System.Diagnostics.Debug.WriteLine($"✅ Подключение завершено. IsConnected = {IsConnected}");
                }
                catch (System.Exception ex)
                {
                    IsConnected = false;
                    StatusMessage = $"❌ Ошибка: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка подключения: {ex.Message}");
                }
            }
        }

        private void Disconnect()
        {
            _quikService.Disconnect();
            IsConnected = false;
            StatusMessage = "🔌 Отключено";
        }

        private async Task RefreshAsync()
        {
            // Если текущая страница — Page1, обновляем данные
            if (CurrentPageViewModel is Page1ViewModel p1)
                await p1.LoadDataAsync();
        }

        private void ToggleTheme() =>
            System.Diagnostics.Debug.WriteLine("🎨 ToggleTheme");

        private void ShowAbout() =>
            System.Windows.MessageBox.Show(
                "Tradentum v0.1.0-alpha\n\nWPF + QuikSharp + .NET 10",
                "О программе",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

        // 🔹 Корректное завершение
        public async Task ShutdownAsync()
        {
            Page1VM?.Stop(); // Остановить таймер авто-обновления
            if (IsConnected) Disconnect();
            await Task.Delay(50);
        }
    }
}