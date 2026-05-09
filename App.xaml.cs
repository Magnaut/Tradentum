using System.Windows;
using System.Text; // Для кодировки 1251
using Microsoft.Extensions.DependencyInjection; // Добавь using
using Microsoft.Extensions.Hosting;             // Добавь using
using Tradentum.Services;
using Tradentum.ViewModels;

namespace Tradentum
{
    public partial class App : Application
    {
        // Храним ссылку на хост, чтобы приложение жило, пока жив хост
        private static IHost? _host;

        static App()
        {
            // Регистрация кодировок (твое решение проблемы с 1251)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // 🔹 Глобальный перехват необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine($"💥 FATAL: {ex?.Message}\n{ex?.StackTrace}");
                System.Windows.MessageBox.Show(
                    $"Критическая ошибка:\n{ex?.Message}",
                    "Ошибка приложения",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            };

            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"💥 Task Exception: {args.Exception.Message}");
                args.SetObserved(); // Предотвращаем краш
            };

            base.OnStartup(e);

            // 1. Создаем и настраиваем Host (DI-контейнер)
            var builder = Host.CreateDefaultBuilder();

            builder.ConfigureServices((context, services) =>
            {
                // Регистрируем сервисы (Singleton — один экземпляр на всё приложение)
                services.AddSingleton<IQuikService, QuikService>();

                // Регистрируем ViewModel. 
                // DI сам поймет, что MainViewModel нужен IQuikService, и передаст его.
                services.AddSingleton<MainViewModel>();

                // Регистрируем само окно
                services.AddSingleton<MainWindow>();
            });

            _host = builder.Build();

            // 2. Получаем экземпляр MainWindow ИЗ контейнера
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();

            // 3. Получаем ViewModel ИЗ контейнера
            var viewModel = _host.Services.GetRequiredService<MainViewModel>();

            // 4. Вручную связываем (DataContext)
            mainWindow.DataContext = viewModel;

            // 5. Показываем окно
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // Корректное завершение работы (выгрузка сервисов)
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }
    }
}