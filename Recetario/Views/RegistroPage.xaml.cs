using Recetario.Services;

namespace Recetario.Views;


// Partial significa que la clase RegistroPage está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
public partial class RegistroPage : ContentPage
{

    // readonly significa que estas variables solo pueden ser asignadas en el constructor de la clase y no pueden ser modificadas.
    private readonly AuthService _authService;
    private readonly RecetaService _recetaService;


    // RegistroPage sive para inicializar los componentes y ademas los auto servicios o los servicios automaticos.
    public RegistroPage(AuthService authService, RecetaService recetaService)
    {
        InitializeComponent();
        _authService = authService;
        _recetaService = recetaService;

        PickerTipoCuenta.SelectedIndex = 0; // Cocinero / Usuario Normal por defecto
    }


    private void OnTipoCuentaChanged(object? sender, EventArgs e)
    {
        bool esAdmin = PickerTipoCuenta.SelectedIndex == 1; // bool esAdmin = true si el usuario selecciona "Administrador",  de lo contrario es false ( pasa por clave).
        PanelCodigoAdmin.IsVisible = esAdmin; // IsVisible es una propiedad que determina si el panel de código de administrador se muestra o no en la interfaz de usuario.
    }


    // async significa que el metodo puede realizar operaciones asincronicas, como llamar a servicios web o acceder a base de datos, sin bloquear la interfaz de usuario.
    private async void OnRegistrarClicked(object? sender, EventArgs e)
    {
        string nombre = TxtNombre.Text?.Trim() ?? string.Empty; // Trim() elimina los espacios en blanco al inicio y al final del nombre, y ?? string.Empty asegura que si TxtNombre.Text
                                                                // es null, se asigne una cadena vacía en su lugar.

        string correo = TxtCorreo.Text?.Trim() ?? string.Empty; 
        string password = TxtPassword.Text ?? string.Empty;
        bool esAdmin = PickerTipoCuenta.SelectedIndex == 1;


        // Validaciones de campos vacíos 
        if (esAdmin)
        {
            string codigo = TxtCodigoAdmin.Text?.Trim() ?? string.Empty; // Validación del código de administrador 
            var (exito, mensaje, nuevoAdmin) = _authService.RegistrarAdministrador(nombre, correo, password, codigo); // Llama al metodo RegistrarAdministrador del servicio de autenticacion
            // que devuelve tres valores: exito (bool), mensaje (string) y nuevoAdmin (objeto del tipo Administrador).


            // si el registro no fue exitoso, muestra un mensaje de error y termina la ejecución del metodo.
            if (!exito)
            {
                await DisplayAlertAsync("Error en Registro de Administrador", mensaje, "Aceptar"); // await es una palabra clave que indica que la ejecucion del metodo debe esperar a que se 
                                                                                                   // complete la tarea asincrona DisplayAlertAsync antes de continuar.
                return;
            }


            await DisplayAlertAsync("Administrador Registrado", mensaje, "Ir a Iniciar Sesión"); // Muestra un mensaje de exito
            await Navigation.PopAsync(); // Te devuelve a la pestaña anterior, en este caso inicio de sesion.
        }
        else // si el usuario selecciona "Cocinero" o "Usuario Normal", se ejecuta este bloque de código.
        {
            var (exito, mensaje, nuevoUsuario) = _authService.RegistrarUsuarioNormal(nombre, correo, password); // llama al metodo RegistrarUsuarioNormal del servicio de autenticacion que 
            // devuelve tres valores: exito (bool), mensaje (string) y nuevoUsuario (objeto del tipo UsuarioNormal).

            if (!exito) // si el registro no fue exitoso, muestra un mensaje de error y termina la ejecución del metodo.
            {
                await DisplayAlertAsync("Error en Registro", mensaje, "Aceptar"); // espera a que se complete la tarea asincrona DisplayAlertAsync antes de continuar.
                return;
            }

            await DisplayAlertAsync("Registro Exitoso", mensaje, "Ir a Iniciar Sesión"); // Muestra un mensaje de exito
            await Navigation.PopAsync(); // Te devuelve a la pestaña anterior, en este caso inicio de sesion.
            // PopAsync() es un metodo asincrono que elimina la pagina actual de la pila de navegacion y vuelve a la pagina anterior.
        }
    }

    // Este metodo es asincrono y se ejecuta cuando el usuario hace clic en el boton "Volver".
    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync(); // Te devuelve a la pestaña anterior, en este caso inicio de sesion.
        // PopAsync() es un metodo asincrono que elimina la pagina actual de la pila de navegacion y vuelve a la pagina anterior.
    }
}
