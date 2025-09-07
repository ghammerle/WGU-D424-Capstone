namespace C424Assessment
{
    public partial class App : Application
    {
        public App()
        {
            Console.WriteLine("App constructor started");
            InitializeComponent();
            Console.WriteLine("App constructor - after InitializeComponent");
        }

        private Window _window;
        protected override Window CreateWindow(IActivationState? activationState)
        {
            _window = new Window(new LoadingPage());
            InitAndShowMainPageAsync();
            return _window;
        }

        private async void InitAndShowMainPageAsync()
        {
            // Now switch to main UI on the UI thread
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _window.Page = new AppShell();
            });
        }
    }

    public class LoadingPage : ContentPage
    {
        public LoadingPage()
        {
            Content = new ActivityIndicator
            {
                IsRunning = true,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
        }
    }
}