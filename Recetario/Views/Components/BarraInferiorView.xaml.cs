namespace Recetario.Views.Components;


// ContentView representa una vista personalizada reutilizable dentro de diferentes páginas XAML.
// Partial significa que la clase BarraInferiorView está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
public partial class BarraInferiorView : ContentView
{

    // BindableProperty permite enlazar datos en XAML y escuchar cambios de valor desde otras vistas o controles.
    // readonly significa que esta propiedad solo puede ser asignada una vez y no puede ser modificada en tiempo de ejecución.
    public static readonly BindableProperty PestanaActivaProperty =
        BindableProperty.Create(
            nameof(PestanaActiva), // nombre de la propiedad enlazable asociada.
            typeof(string), // tipo de dato de la propiedad (string).
            typeof(BarraInferiorView), // tipo de la clase que posee la propiedad enlazable.
            "Feed", // valor por defecto de la propiedad.
            propertyChanged: OnPestanaActivaChanged); // metodo que se ejecuta automáticamente cuando el valor de la propiedad cambia.


    // Propiedad CLR para obtener o establecer el valor de la propiedad enlazable PestanaActivaProperty.
    public string PestanaActiva
    {
        get => (string)GetValue(PestanaActivaProperty); // GetValue obtiene el valor actual almacenado en la BindableProperty.
        set => SetValue(PestanaActivaProperty, value); // SetValue asigna el nuevo valor a la BindableProperty.
    }


    // Constructor de la barra inferior, inicializa los componentes de la vista y actualiza los estilos visuales de los botones.
    public BarraInferiorView()
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los XAML asociados a este componente.
        ActualizarEstilos(); // llama al metodo que actualiza los colores de los botones segun la pestaña activa.
    }


    // static metodo que se ejecuta cuando cambia el valor de la propiedad PestanaActivaProperty sin depender de una instancia especifica.
    private static void OnPestanaActivaChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BarraInferiorView barra) // valida si el objeto bindable es de tipo BarraInferiorView.
        {
            barra.ActualizarEstilos(); // actualiza los estilos visuales de los botones de la barra inferior.
        }
    }


    // Metodo privado que actualiza el estilo y color de cada boton dependiendo de cual pestaña es la activa actualmente.
    private void ActualizarEstilos()
    {
        AplicarEstiloBoton(BtnTabFeed, PestanaActiva == "Feed", "#758B68"); // aplica estilo al boton de Feed si esta activo.
        AplicarEstiloBoton(BtnTabMisRecetas, PestanaActiva == "MisRecetas", "#758B68"); // aplica estilo al boton de Mis Recetas si esta activo.
        AplicarEstiloBoton(BtnTabCrearEditar, PestanaActiva == "CrearEditar", "#C8785A"); // aplica estilo al boton de Crear/Editar si esta activo.
        AplicarEstiloBoton(BtnTabTemporada, PestanaActiva == "Temporada", "#758B68"); // aplica estilo al boton de Temporada si esta activo.
        AplicarEstiloBoton(BtnTabPerfil, PestanaActiva == "Perfil", "#C8785A"); // aplica estilo al boton de Perfil si esta activo.
    }


    // static metodo que cambia el diseño de un boton segun si esta activo o inactivo.
    private static void AplicarEstiloBoton(Button boton, bool activo, string colorActivoHex)
    {
        if (boton == null) return; // si el boton es nulo no hace nada y termina el metodo.

        if (activo) // si el boton esta seleccionado actualmente
        {
            boton.BackgroundColor = Color.FromArgb(colorActivoHex); // --> color de fondo activo
            boton.TextColor = Color.FromArgb("#FFFFFF"); // --> color de texto blanco para contraste
            boton.BorderColor = Color.FromArgb("#283B2D"); // --> color de borde oscuro
            boton.BorderWidth = 1.5; // grosor del borde
        }
        else // si el boton no esta seleccionado
        {
            boton.BackgroundColor = Color.FromArgb("#F7F4EA"); // --> color de fondo claro por defecto
            boton.TextColor = Color.FromArgb("#283B2D"); // --> color de texto verde oscuro
            boton.BorderColor = Color.FromArgb("#D8D3C4"); // --> color de borde gris claro
            boton.BorderWidth = 1; // grosor del borde normal
        }
    }


    // Metodos asincronos para navegar a las diferentes rutas registradas en AppShell al presionar cada boton.
    private async void OnFeedClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//FeedPage"); // navega a la ruta absoluta de la pagina del Feed.

    private async void OnMisRecetasClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MisRecetasPage"); // navega a la ruta absoluta de la pagina de Mis Recetas.

    private async void OnCrearEditarClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//CrearEditarRecetaPage"); // navega a la ruta absoluta de Crear o Editar Receta.

    private async void OnTemporadaClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//TemporadaPage"); // navega a la ruta absoluta de Temporada.

    private async void OnPerfilClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//PerfilPage"); // navega a la ruta absoluta de Perfil.
}
