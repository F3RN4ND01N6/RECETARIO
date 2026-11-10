using Microsoft.Extensions.Logging;
using Recetario.Services;
using Recetario.Views;

namespace Recetario;


// static significa que la clase no puede ser instanciada y todos sus miembros deben ser estáticos.
// MauiProgram es el punto de entrada principal donde se configura el host, las fuentes y el contenedor de inyección de dependencias.
public static class MauiProgram
{

    // CreateMauiApp sirve para construir y configurar la aplicación .NET MAUI registrando servicios, fuentes y páginas.
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder(); // inicializa el constructor de la aplicación MAUI.
        builder
            .UseMauiApp<App>() // especifica que la clase App será la raíz de la aplicación.
            .ConfigureFonts(fonts => // configura las fuentes tipográficas personalizadas de la aplicación.
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Lucidacalligraphy.ttf", "ScriptFont");
            });

        // Registro de Servicios (Singleton para compartir estado y persistencia JSON entre todas las pestañas)
        // AddSingleton significa que se creará una única instancia compartida durante toda la vida de la aplicación.
        builder.Services.AddSingleton<RecetaService>();
        builder.Services.AddSingleton<AuthService>();

        // Registro de Vistas de Autenticación
        // AddTransient significa que se creará una nueva instancia cada vez que se solicite la página.
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegistroPage>();

        // Registro de Vistas del TabBar Inferior
        // Se registran como Singleton para mantener el estado y los datos cargados al alternar entre pestañas.
        builder.Services.AddSingleton<FeedPage>();
        builder.Services.AddSingleton<MisRecetasPage>();
        builder.Services.AddSingleton<CrearEditarRecetaPage>();
        builder.Services.AddSingleton<TemporadaPage>();
        builder.Services.AddSingleton<PerfilPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug(); // habilita el registro de depuración en la consola de salida cuando se compila en modo DEBUG.
#endif

        return builder.Build(); // construye y devuelve la instancia configurada de MauiApp.
    }
}
