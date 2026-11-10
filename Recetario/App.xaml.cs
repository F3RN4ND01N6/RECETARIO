using Recetario.Services;
using Recetario.Views;

namespace Recetario;


// Partial significa que la clase App está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
// App es la clase principal que representa el ciclo de vida y la configuración global de la aplicación .NET MAUI.
public partial class App : Application
{

    // readonly significa que estas variables solo pueden ser asignadas en el constructor de la clase y no pueden ser modificadas.
    private readonly RecetaService _service; // servicio central de recetas para toda la aplicación.
    private readonly AuthService _authService; // servicio de autenticación e inicio de sesión.


    // App sirve para inicializar los componentes globales, definir el tema visual y recibir los servicios inyectados.
    public App(RecetaService service, AuthService authService)
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los recursos XAML asociados a la aplicación.
        UserAppTheme = AppTheme.Light; // fuerza el tema claro en toda la aplicación para mantener la paleta de colores.
        _service = service; // asigna servicio de recetas a variable privada para usarlo en toda la clase.
        _authService = authService; // asigna servicio de autenticacion a variable privada.
    }


    // override significa que este metodo reemplaza el metodo base CreateWindow de la clase Application para configurar la ventana principal.
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Retorna la ventana principal iniciando con una página de navegación que contiene LoginPage.
        return new Window(new NavigationPage(new LoginPage(_authService, _service)))
        {
            Title = "Raccolta di ricette", // titulo de la ventana en sistemas de escritorio (Windows / macOS)
            Width = 1080, // ancho inicial de la ventana en píxeles
            Height = 780 // alto inicial de la ventana en píxeles
        };
    }
}
