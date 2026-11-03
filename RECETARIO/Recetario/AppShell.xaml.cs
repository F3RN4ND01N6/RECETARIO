using Recetario.Services;

namespace Recetario;

public partial class AppShell : Shell
{
    public AppShell(RecetaService service)
    {
        InitializeComponent();

        service.SolicitudCambiarPestana += async destino =>
        {
            string ruta = destino switch
            {
                "Feed" => "//FeedPage",
                "MisRecetas" => "//MisRecetasPage",
                "CrearEditar" => "//CrearEditarRecetaPage",
                "Temporada" => "//TemporadaPage",
                "Perfil" => "//PerfilPage",
                _ => "//FeedPage"
            };
            await GoToAsync(ruta);
        };
    }
}
