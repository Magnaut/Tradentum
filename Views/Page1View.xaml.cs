using System.Windows;
using System.Windows.Controls;

namespace Tradentum.Views
{
    public partial class Page1View : UserControl
    {
        public Page1View()
        {
            InitializeComponent();
            Loaded += Page1View_Loaded;
        }

        private async void Page1View_Loaded(object sender, RoutedEventArgs e)
        {
            // 🔹 Инициализация WebView2 (обязательно!)
            await RutubePlayer.EnsureCoreWebView2Async(null);
        }
    }
}