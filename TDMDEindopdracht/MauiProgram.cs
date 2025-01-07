using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using TDMDEindopdracht.Domain.Model;
using TDMDEindopdracht.Domain.Services;
using TDMDEindopdracht.Infrastructure;


namespace TDMDEindopdracht
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiMaps()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
    


            builder.Services.AddSingleton<ViewModel>();
            builder.Services.AddTransient<RouteHandler>();
            builder.Services.AddTransient<IDatabaseCommunicator, DatabaseComunicator>();
            builder.Services.AddSingleton<MainPage>(s => new MainPage() 
            {
                BindingContext = s.GetRequiredService<ViewModel>()
            });
            builder.Services.AddSingleton<MapViewModel>();
            builder.Services.AddSingleton<mapPage>(s => new mapPage(s.GetRequiredService<MapViewModel>()));
            
#if DEBUG
            builder.Logging.AddDebug();
#endif
        


            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "route.db");

            builder.Services.AddSingleton(s =>
            ActivatorUtilities.CreateInstance<DatabaseComunicator>(s, dbPath));


            return builder.Build();
        }
    }
}
