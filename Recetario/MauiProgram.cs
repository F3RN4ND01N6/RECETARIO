using Microsoft.Extensions.Logging;
using Recetario.Services;
using Recetario.Views;

namespace Recetario;

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
                fonts.AddFont("Lucidacalligraphy.ttf", "ScriptFont");
            });

        // Registro de Servicios (Singleton para compartir estado y persistencia JSON entre todas las pestañas)
        builder.Services.AddSingleton<RecetaService>();
        builder.Services.AddSingleton<AuthService>();

        // Registro de Vistas de Autenticación
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegistroPage>();

        // Registro de Vistas del TabBar Inferior
        builder.Services.AddSingleton<FeedPage>();
        builder.Services.AddSingleton<MisRecetasPage>();
        builder.Services.AddSingleton<CrearEditarRecetaPage>();
        builder.Services.AddSingleton<TemporadaPage>();
        builder.Services.AddSingleton<PerfilPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
