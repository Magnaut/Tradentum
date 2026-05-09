using QuikSharp;
using System.Threading.Tasks;

namespace Tradentum.Services
{
    public interface IQuikService
    {
        bool IsConnected { get; }
        Quik Client { get; }

        Task<bool> ConnectAsync();
        void Disconnect();
    }
}