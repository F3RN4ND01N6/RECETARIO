using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;


// Partial significa que la clase PerfilPage está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
public partial class PerfilPage : ContentPage
{

    // readonly significa que estas variables solo pueden ser asignadas en el constructor de la clase y no pueden ser modificadas.
    private readonly RecetaService _service;


    // PerfilPage sirve para inicializar los componentes y ademas los auto servicios o los servicios automaticos.
    public PerfilPage(RecetaService service)
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los XAML asociados a esta clase.
        _service = service; // asigna servicio de recetas a variable privada para usarlo en toda la clase.

        PickerEstacionPref.ItemsSource = Enum.GetNames<Estacion>().ToList(); // Enum.GetNames<Estacion>() obtiene los nombres de los valores del enum Estacion y ToList()
                                                                             // los convierte en una lista para asignarlos como fuente de datos del Picker.

        PickerOrigenPref.ItemsSource = Enum.GetNames<Origen>().ToList(); // Enum.GetNames<Origen>() obtiene los nombres de los valores del enum Origen y ToList()
                                                                         // los convierte en una lista para asignarlos como fuente de datos del Picker.

        _service.DatosActualizados += CargarDatosEnPantalla; // Carga los datos en la pantalla cuando el servicio de recetas notifica que los datos han sido actualizados.
        CargarDatosEnPantalla(); // Carga los datos en la pantalla cuando se crea la página por primera vez.
    }


    // override significa que este metodo reemplaza el metodo base OnAppearing() de la clase ContentPage, que se llama cuando la página aparece en pantalla.
    protected override void OnAppearing()
    {
        base.OnAppearing(); // llama OnAppearing() de clase base para mantener el comportamiento original de la página.
        CargarDatosEnPantalla(); // Actualiza los datos de la pantalla cada vez que la página aparece. ( refresh de datos )
    }


    // Carga datos en pantalla desde los servicios usuario activo y configuracion de perfil, mostrando resumen, estadisticas y rutas JSON.
    private void CargarDatosEnPantalla()
    {
        var usuario = _service.UsuarioActivo; // devuelve el usuario activo.
        usuario.EjecutarAccionRol(); // actualiza el rol activo del usuario y genera un mensaje de resultado de la accion.


        LblResumenActor.Text = $"ID: {usuario.ObtenerIdentificador()}  |  {usuario.ObtenerResumenEntidad()}"; // Muestra ID y resumen del usuario activo
        
        LblResultadoAccionRol.Text = $"{usuario.UltimaAccionRolMensaje}"; // Muestra el mensaje de resultado de la ultima accion de cambio de rol del usuario activo.


        EntryNombreUsuario.Text = usuario.NombreUsuario(); // Muestra el nombre del usuario activo
        EntryCorreoUsuario.Text = usuario.Correo(); // Muestra el correo del usuario activo
        EntryBiografiaUsuario.Text = usuario.Biografia(); // Muestra la biografia del usuario activo


        // config es un objeto de tipo ConfiguracionPerfil que contiene la configuracion del perfil del usuario activo.

        var config = usuario.ConfiguracionPerfil(); // devuelve la configuracion de perfil del usuario activo
        PickerEstacionPref.SelectedItem = config.EstacionPreferida().ToString(); // Muestra la estacion preferida del usuario activo
        PickerOrigenPref.SelectedItem = config.Origen().ToString(); // Muestra el origen del usuario activo
        EntryRegionPref.Text = config.RegionPreferida(); // Muestra la region preferida del usuario activo
        ChkIngredientesEstacion.IsChecked = config.IngredientesEstacion(); // Muestra si el usuario activo prefiere ingredientes de temporada o no


        // Muestra las estadisticas de recetas publicas, privadas y favoritas del usuario activo
        LblEstadisticasJson.Text =
            $"• Recetas Públicas en Feed Global: {_service.TotalRecetasPublicas}\n" + // Muestra el total de recetas publicas en el feed global
            $"• Recetas Privadas en Respaldo Local: {_service.TotalRecetasPrivadas}\n" + // Muestra el total de recetas privadas en el respaldo local
            $"• Recetas Marcadas como Favoritas: {_service.TotalFavoritas}"; // Muestra el total de recetas marcadas como favoritas por el usuario activo


        // Muestra las rutas de los archivos JSON donde se almacenan las recetas privadas y publicas del usuario activo
        LblRutasJson.Text =
            $"Privado: {_service.ObtenerRutaJsonPrivado()}\n" + // Muestra la ruta del archivo JSON donde se almacenan las recetas privadas del usuario activo
            $"Global: {_service.ObtenerRutaJsonPublico()}"; // Muestra la ruta del archivo JSON donde se almacenan las recetas publicas del usuario activo
    }


    // sirve para cambiar el rol del usuario 
    private void OnSeleccionarUsuarioNormalClicked(object? sender, EventArgs e)
    {
        _service.CambiarRolActivo(TipoPerfilUsuario.USUARIO_NORMAL); // cambio de rol activo del usuario a "Usuario Normal" mediante el servicio de recetas.
    }


    // sirve para cambiar el rol del usuario a "Chef Verificado" mediante el servicio de recetas.
    private void OnSeleccionarChefClicked(object? sender, EventArgs e)
    {
        _service.CambiarRolActivo(TipoPerfilUsuario.CHEF_VERIFICADO);
    }

    // sirve para cambiar el rol del usuario a "Administrador" mediante el servicio de recetas.
    private void OnSeleccionarAdminClicked(object? sender, EventArgs e)
    {
        _service.CambiarRolActivo(TipoPerfilUsuario.ADMINISTRADOR);
    }


    // sirve para cerrar la sesion del usuario y volver a la pagina de login. async metodo para realizar operaciones asincronas sin bloquear la interfaz de usuario.
    private async void OnGuardarPerfilClicked(object? sender, EventArgs e)
    {
        var usuario = _service.UsuarioActivo; // obtiene el usuario activo desde el servicio de recetas


        // Actualiza los datos basicos del usuario activo con los valores ingresados en los campos de texto de la interfaz de usuario.
        usuario.ActualizarDatosBasicos(
            EntryNombreUsuario.Text ?? string.Empty, // texto ingresado en el campo de nombre de usuario, si es null se asigna una cadena vacía.
            EntryCorreoUsuario.Text ?? string.Empty, // texto ingresado en el campo de correo del usuario, si es null se asigna una cadena vacía.
            EntryBiografiaUsuario.Text ?? string.Empty); // texto ingresado en el campo de biografia del usuario, si es null se asigna una cadena vacía.

        // Actualiza la configuracion del perfil del usuario activo con los valores seleccionados en los pickers y campos de texto de la interfaz de usuario.
        var est = Enum.Parse<Estacion>(PickerEstacionPref.SelectedItem?.ToString() ?? "PRIMAVERA"); // cambia el valor seleccionao, si es null se asigna "PRIMAVERA" por defecto.
        var orig = Enum.Parse<Origen>(PickerOrigenPref.SelectedItem?.ToString() ?? "CHILE"); // cambia el valor seleccionao, si es null se asigna "CHILE" por defecto.

        // Actualiza la configuracion del perfil del usuario activo con los valores seleccionados en los pickers y campos de texto de la interfaz de usuario.
        usuario.ConfiguracionPerfil().ActualizarConfiguracion(
            est, // Estacion preferida seleccionada en el picker, si es null se asigna "PRIMAVERA" por defecto.
            orig, // Origen seleccionado en el picker, si es null se asigna "CHILE" por defecto.
            EntryRegionPref.Text ?? "Zona Central", // Region preferida ingresada en el campo de texto, si es null se asigna "Zona Central" por defecto.
            ChkIngredientesEstacion.IsChecked); // Valor booleano que indica si el usuario prefiere ingredientes de temporada o no, obtenido del checkbox.


        // valida si es usuario y valida su configuracion de perfil, si no es valido muestra un mensaje de error y termina la ejecucion del metodo.
        if (!usuario.EsValido())
        {
            await DisplayAlertAsync("Validación de Usuario (IValidable)", // muestra un mensaje de error si el usuario no es valido
                string.Join("\n• ", usuario.ObtenerErroresValidacion()), // obtiene los errores de validacion del usuario y los une en una cadena
                                                                         // separada por saltos de linea y viñetas.
                "Corregir");
            return;
        }


        // carga datos en pantalla y muestra un mensaje de exito si el usuario es valido y su configuracion de perfil tambien es valida.
        CargarDatosEnPantalla();
        await DisplayAlertAsync("Perfil Actualizado", // muestra un mensaje de exito si el usuario y su configuracion de perfil son validos 
            $"Los datos de '{usuario.NombreUsuario()}' y su ConfiguracionPerfil se guardaron y validaron correctamente.", // muestra un mensaje de exito si el usuario
                                                                                                                          // y su configuracion de perfil son validos
            "Aceptar");
    }


    // sirve para cerrar la sesion del usuario y volver a la pagina de login. async metodo para realizar operaciones asincronas sin bloquear la interfaz de usuario.
    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync("Cerrar Sesión", "¿Estás seguro de que deseas salir de tu cuenta?", "Sí, cerrar sesión", "Cancelar"); // muestra un mensaje
                                                                                    // de confirmacion para cerrar sesion, si el usuario confirma se cierra la sesion y se vuelve a la pagina de login.

        if (confirmar && Application.Current != null && Application.Current.Windows.Count > 0) // valida que la aplicacion y la ventana principal
                                                                                               // existan antes de intentar cambiar la pagina.
        {
            var authService = Handler?.MauiContext?.Services.GetService<AuthService>() ?? new AuthService(); // obtiene el servicio de autenticacion desde el contexto
                                                                                                             // de la aplicacion, si no existe crea uno nuevo.


            Application.Current.Windows[0].Page = new NavigationPage(new LoginPage(authService, _service)); // cambia la pagina principal de la aplicacion a la
                                                                                                            // pagina de login, pasando el servicio de autenticacion
                                                                                                            // y el servicio de recetas como parametros.
        }
    }
}
