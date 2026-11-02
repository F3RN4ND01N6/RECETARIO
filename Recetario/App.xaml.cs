using Recetario.Services;
using Recetario.Views;

namespace Recetario;

public partial class App : Application
{
    private readonly RecetaService _service;
    private readonly AuthService _authService;

    public App(RecetaService service, AuthService authService)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
        _service = service;
        _authService = authService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new LoginPage(_authService, _service)))
        {
            Title = "Raccolta di ricette — Recetario 2.0",
            Width = 1080,
            Height = 780
        };
    }
}
