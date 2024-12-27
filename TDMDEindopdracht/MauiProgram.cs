using Microsoft.Extensions.Logging;
using TDMDEindopdracht.Domain.Services;


namespace TDMDEindopdracht
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
    


            builder.Services.AddSingleton<ViewModel>();
            builder.Services.AddSingleton<MainPage>(s => new MainPage() 
            {
                BindingContext = s.GetRequiredService<ViewModel>()
            });
            builder.Services.AddSingleton<MapViewModel>();
            builder.Services.AddSingleton<mapPage>(s => new mapPage(s.GetRequiredService<MapViewModel>()));
            
#if DEBUG
            builder.Logging.AddDebug();
#endif
            

            return builder.Build();
        }
    }
}
