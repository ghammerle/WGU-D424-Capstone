using C424Assessment.DataRepository;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using System.Diagnostics;

namespace C424Assessment
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Debug.WriteLine("CreateMauiApp started");
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
                builder.Services.AddSingleton<IDataRepository>(
    _ => new SQLiteRepository(Path.Combine(FileSystem.AppDataDirectory, "C424Database.db3")));
            // Path.Combine(FileSystem.Current.AppDataDirectory, "C424Database.db3");

#if DEBUG
            builder.Logging.AddDebug();
#endif
            Debug.WriteLine("CreateMauiApp completed");
            return builder.Build();
        }
    }
}
