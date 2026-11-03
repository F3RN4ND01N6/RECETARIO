using Recetario.Services;

namespace Recetario.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly RecetaService _recetaService;

    public LoginPage(AuthService authService, RecetaService recetaService)
    {
        InitializeComponent();
        _authService = authService;
        _recetaService = recetaService;
    }

    private async void OnIngresarClicked(object? sender, EventArgs e)
    {
        string identificador = TxtIdentificador.Text?.Trim() ?? string.Empty;
        string password = TxtPassword.Text ?? string.Empty;
        bool esAdmin = SwitchAdmin.IsToggled;

        var (exito, mensaje, usuario) = _authService.IniciarSesion(identificador, password, esAdmin);

        if (!exito || usuario == null)
        {
            await DisplayAlertAsync("Error de inicio de sesión", mensaje, "Aceptar");
            return;
        }

        // Asignar el usuario autenticado a la sesión activa del RecetaService
        _recetaService.EstablecerUsuarioSesion(usuario);

        // Dar bienvenida
        await DisplayAlertAsync("Bienvenido", mensaje, "Continuar");

        // Ingresar a la aplicación principal (AppShell)
        if (Application.Current != null && Application.Current.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new AppShell(_recetaService);
        }
    }

    private async void OnRegistrarseClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegistroPage(_authService, _recetaService));
    }
}
