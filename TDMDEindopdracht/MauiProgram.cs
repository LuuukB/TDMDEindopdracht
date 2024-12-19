using Microsoft.Extensions.Logging;
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IMap>(Map.Default);

            builder.Services.AddTransient<ViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            
            builder.Services.AddSingleton<MainPage>(s => new MainPage() 
            {
                BindingContext = s.GetRequiredService<ViewModel>()
            });
            builder.Services.AddTransient<MapViewModel>();
            builder.Services.AddSingleton<mapPage>(s => new mapPage() 
            {
                BindingContext = s.GetRequiredService<MapViewModel>()
            });

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "route.db");

            builder.Services.AddSingleton(s =>
            ActivatorUtilities.CreateInstance<DatabaseComunicator>(s, dbPath));

            return builder.Build();
        }
    }
}
