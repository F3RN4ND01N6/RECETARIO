using Recetario.Services;

namespace Recetario.Views;

public partial class RegistroPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly RecetaService _recetaService;

    public RegistroPage(AuthService authService, RecetaService recetaService)
    {
        InitializeComponent();
        _authService = authService;
        _recetaService = recetaService;

        PickerTipoCuenta.SelectedIndex = 0; // Cocinero / Usuario Normal por defecto
    }

    private void OnTipoCuentaChanged(object? sender, EventArgs e)
    {
        bool esAdmin = PickerTipoCuenta.SelectedIndex == 1;
        PanelCodigoAdmin.IsVisible = esAdmin;
    }

    private async void OnRegistrarClicked(object? sender, EventArgs e)
    {
        string nombre = TxtNombre.Text?.Trim() ?? string.Empty;
        string correo = TxtCorreo.Text?.Trim() ?? string.Empty;
        string password = TxtPassword.Text ?? string.Empty;
        bool esAdmin = PickerTipoCuenta.SelectedIndex == 1;

        if (esAdmin)
        {
            string codigo = TxtCodigoAdmin.Text?.Trim() ?? string.Empty;
            var (exito, mensaje, nuevoAdmin) = _authService.RegistrarAdministrador(nombre, correo, password, codigo);

            if (!exito)
            {
                await DisplayAlertAsync("Error en Registro de Administrador", mensaje, "Aceptar");
                return;
            }

            await DisplayAlertAsync("Administrador Registrado", mensaje, "Ir a Iniciar Sesión");
            await Navigation.PopAsync();
        }
        else
        {
            var (exito, mensaje, nuevoUsuario) = _authService.RegistrarUsuarioNormal(nombre, correo, password);

            if (!exito)
            {
                await DisplayAlertAsync("Error en Registro", mensaje, "Aceptar");
                return;
            }

            await DisplayAlertAsync("Registro Exitoso", mensaje, "Ir a Iniciar Sesión");
            await Navigation.PopAsync();
        }
    }

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
