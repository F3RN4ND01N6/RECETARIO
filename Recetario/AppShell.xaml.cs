using Recetario.Services;

namespace Recetario;


// Partial significa que la clase AppShell está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
// AppShell define la jerarquía de navegación y la estructura principal de la aplicación mediante pestañas (TabBar y Shell).
public partial class AppShell : Shell
{

    // AppShell sirve para inicializar los componentes de navegación y suscribirse a eventos globales de cambio de pestaña.
    public AppShell(RecetaService service)
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los componentes XAML de la estructura Shell.

        // Suscribe una expresión lambda asíncrona al evento SolicitudCambiarPestana del servicio de recetas.
        service.SolicitudCambiarPestana += async destino =>
        {
            // switch expression que mapea el nombre lógico de la pestaña con la ruta de navegación absoluta de Shell.
            string ruta = destino switch
            {
                "Feed" => "//FeedPage", // ruta hacia la pagina del Feed
                "MisRecetas" => "//MisRecetasPage", // ruta hacia la pagina de Mis Recetas
                "CrearEditar" => "//CrearEditarRecetaPage", // ruta hacia el formulario Crear o Editar
                "Temporada" => "//TemporadaPage", // ruta hacia la pagina de Temporada
                "Perfil" => "//PerfilPage", // ruta hacia la pagina de Perfil
                _ => "//FeedPage" // ruta por defecto si no coincide
            };

            await GoToAsync(ruta); // GoToAsync navega de forma asincrona a la ruta especificada sin bloquear la interfaz.
        };
    }
}
