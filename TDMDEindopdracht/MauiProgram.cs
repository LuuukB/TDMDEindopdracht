using Microsoft.Extensions.Logging;
using TDMDEindopdracht.Domain.Model;

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

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<ViewModel>();
            builder.Services.AddSingleton<MainPage>(s => new MainPage() 
            {
                BindingContext = s.GetRequiredService<ViewModel>()
            });
            builder.Services.AddSingleton<MapViewModel>();
            builder.Services.AddSingleton<mapPage>(s => new mapPage() 
            {
                BindingContext = s.GetRequiredService<MapViewModel>()
            });

            return builder.Build();
        }
    }
}
