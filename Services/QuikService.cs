using QuikSharp;
using System;
using System.Threading.Tasks;

namespace Tradentum.Services
{
    public class QuikService : IQuikService, IDisposable
    {
        private Quik? _quik;
        private bool _disposed;

        public bool IsConnected => _quik?.IsServiceConnected() == true;
        public Quik Client => _quik ?? throw new InvalidOperationException("Quik not initialized. Call ConnectAsync first.");

        public async Task<bool> ConnectAsync()
        {
            if (_quik != null && IsConnected)
                return true;

            try
            {
                // Создаём экземпляр (соединение устанавливается автоматически)
                _quik = new Quik(Quik.DefaultPort, new InMemoryStorage());

                // Небольшая задержка для стабилизации соединения
                await Task.Delay(100);

                return IsConnected;
            }
            catch
            {
                _quik = null;
                return false;
            }
        }

        public void Disconnect()
        {
            System.Diagnostics.Debug.WriteLine($"🔌 QuikService.Disconnect() вызван. _quik = {_quik != null}");

            if (_quik != null)
            {
                try
                {
                    // 🔹 QuikSharp имеет метод StopService() для закрытия соединения
                    _quik.StopService();
                    System.Diagnostics.Debug.WriteLine("✅ QUIK StopService() вызван");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка при StopService(): {ex.Message}");
                }
                _quik = null;
            }

            System.Diagnostics.Debug.WriteLine("✅ QuikService.Disconnect() завершён");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}