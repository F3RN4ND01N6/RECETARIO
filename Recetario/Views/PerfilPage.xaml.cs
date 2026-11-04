using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;

public partial class PerfilPage : ContentPage
{
    private readonly RecetaService _service;

    public PerfilPage(RecetaService service)
    {
        InitializeComponent();
        _service = service;

        PickerEstacionPref.ItemsSource = Enum.GetNames<Estacion>().ToList();
        PickerOrigenPref.ItemsSource = Enum.GetNames<Origen>().ToList();

        _service.DatosActualizados += CargarDatosEnPantalla;
        CargarDatosEnPantalla();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarDatosEnPantalla();
    }

    private void CargarDatosEnPantalla()
    {
        var usuario = _service.UsuarioActivo;
        usuario.EjecutarAccionRol();

        LblResumenActor.Text = $"ID: {usuario.ObtenerIdentificador()}  |  {usuario.ObtenerResumenEntidad()}";
        LblResultadoAccionRol.Text = $"{usuario.UltimaAccionRolMensaje}";

        EntryNombreUsuario.Text = usuario.NombreUsuario();
        EntryCorreoUsuario.Text = usuario.Correo();
        EntryBiografiaUsuario.Text = usuario.Biografia();

        var config = usuario.ConfiguracionPerfil();
        PickerEstacionPref.SelectedItem = config.EstacionPreferida().ToString();
        PickerOrigenPref.SelectedItem = config.Origen().ToString();
        EntryRegionPref.Text = config.RegionPreferida();
        ChkIngredientesEstacion.IsChecked = config.IngredientesEstacion();

        LblEstadisticasJson.Text =
            $"• Recetas Públicas en Feed Global: {_service.TotalRecetasPublicas}\n" +
            $"• Recetas Privadas en Respaldo Local: {_service.TotalRecetasPrivadas}\n" +
            $"• Recetas Marcadas como Favoritas: {_service.TotalFavoritas}";

        LblRutasJson.Text =
            $"Privado: {_service.ObtenerRutaJsonPrivado()}\n" +
            $"Global: {_service.ObtenerRutaJsonPublico()}";
    }

    private void OnSeleccionarUsuarioNormalClicked(object? sender, EventArgs e)
    {
        _service.CambiarRolActivo(TipoPerfilUsuario.USUARIO_NORMAL);
    }

    private void OnSeleccionarChefClicked(object? sender, EventArgs e)
    {
        _service.CambiarRolActivo(TipoPerfilUsuario.CHEF_VERIFICADO);
    }

    private void OnSeleccionarAdminClicked(object? sender, EventArgs e)
    {
        _service.CambiarRolActivo(TipoPerfilUsuario.ADMINISTRADOR);
    }

    private async void OnGuardarPerfilClicked(object? sender, EventArgs e)
    {
        var usuario = _service.UsuarioActivo;

        usuario.ActualizarDatosBasicos(
            EntryNombreUsuario.Text ?? string.Empty,
            EntryCorreoUsuario.Text ?? string.Empty,
            EntryBiografiaUsuario.Text ?? string.Empty);

        var est = Enum.Parse<Estacion>(PickerEstacionPref.SelectedItem?.ToString() ?? "PRIMAVERA");
        var orig = Enum.Parse<Origen>(PickerOrigenPref.SelectedItem?.ToString() ?? "CHILE");

        usuario.ConfiguracionPerfil().ActualizarConfiguracion(
            est,
            orig,
            EntryRegionPref.Text ?? "Zona Central",
            ChkIngredientesEstacion.IsChecked);

        if (!usuario.EsValido())
        {
            await DisplayAlertAsync("Validación de Usuario (IValidable)",
                string.Join("\n• ", usuario.ObtenerErroresValidacion()),
                "Corregir");
            return;
        }

        CargarDatosEnPantalla();
        await DisplayAlertAsync("Perfil Actualizado",
            $"Los datos de '{usuario.NombreUsuario()}' y su ConfiguracionPerfil se guardaron y validaron correctamente.",
            "Aceptar");
    }

    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync("Cerrar Sesión", "¿Estás seguro de que deseas salir de tu cuenta?", "Sí, cerrar sesión", "Cancelar");
        if (confirmar && Application.Current != null && Application.Current.Windows.Count > 0)
        {
            var authService = Handler?.MauiContext?.Services.GetService<AuthService>() ?? new AuthService();
            Application.Current.Windows[0].Page = new NavigationPage(new LoginPage(authService, _service));
        }
    }
}
