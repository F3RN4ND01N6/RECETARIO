using Recetario.Services;

namespace Recetario.Views;


// Partial significa que la clase LoginPage está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
public partial class LoginPage : ContentPage
{

    // readonly significa que estas variables solo pueden ser asignadas en el constructor de la clase y no pueden ser modificadas.
    private readonly AuthService _authService;
    private readonly RecetaService _recetaService;


    // LoginPage sirve para inicializar los componentes y recibir los servicios de autenticación y recetas.
    public LoginPage(AuthService authService, RecetaService recetaService)
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los XAML asociados a esta clase.
        _authService = authService; // asigna servicio de autenticación a variable privada para usarlo en toda la clase.
        _recetaService = recetaService; // asigna servicio de recetas a variable privada para usarlo en toda la clase.
    }


    // async significa que el metodo puede realizar operaciones asincronicas sin bloquear la interfaz de usuario.
    private async void OnIngresarClicked(object? sender, EventArgs e)
    {
        string identificador = TxtIdentificador.Text?.Trim() ?? string.Empty; // Trim() elimina los espacios en blanco al inicio y al final, y ?? string.Empty asegura una cadena vacía si es null.
        string password = TxtPassword.Text ?? string.Empty; // obtiene la contraseña ingresada en el campo de texto.
        bool esAdmin = SwitchAdmin.IsToggled; // IsToggled es una propiedad booleana que indica si el switch de administrador está activado o desactivado.


        // Llama al metodo IniciarSesion del servicio de autenticacion que devuelve exito (bool), mensaje (string) y usuario autenticado.
        var (exito, mensaje, usuario) = _authService.IniciarSesion(identificador, password, esAdmin);


        // Valida si el inicio de sesion no fue exitoso o si el usuario devuelto es nulo, mostrando un mensaje de error.
        if (!exito || usuario == null)
        {
            await DisplayAlertAsync("Error de inicio de sesión", mensaje, "Aceptar"); // await espera a que se complete la tarea asincrona DisplayAlertAsync antes de continuar.
            return;
        }

        // Asignar el usuario autenticado a la sesión activa del RecetaService
        _recetaService.EstablecerUsuarioSesion(usuario); // establece el usuario que inició sesión como el usuario activo de la aplicación.

        // Dar bienvenida
        await DisplayAlertAsync("Bienvenido", mensaje, "Continuar"); // muestra mensaje de bienvenida con el resultado o saludo.

        // Ingresar a la aplicación principal (AppShell)
        if (Application.Current != null && Application.Current.Windows.Count > 0) // valida que la aplicacion y la ventana principal existan antes de cambiar la pagina.
        {
            Application.Current.Windows[0].Page = new AppShell(_recetaService); // cambia la pagina principal de la aplicacion al AppShell pasando el servicio de recetas.
        }
    }


    // async metodo que se ejecuta cuando el usuario hace clic en el boton "Registrarse".
    private async void OnRegistrarseClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegistroPage(_authService, _recetaService)); // PushAsync() es un metodo asincrono que navega a la pantalla de RegistroPage apilándola en la navegación.
    }
}
