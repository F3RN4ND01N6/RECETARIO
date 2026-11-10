using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;


// Pestaña que muestra el apartado de temporada, sirve para filtrar las recetas por temporada y mostrar los ingredientes 
// de temporada en pantalla 

public partial class TemporadaPage : ContentPage
{
    // Servicio principal para consultar datos y devolver eventos globales 
    // readonly es para que no se pueda modificar la referencia del servicio una vez asignado en el constructor 
    private readonly RecetaService _service;


    // Variable para almacenar la estación actual seleccionada, se inicia con el valor de TODOELANHIO,
    // lo que significa que se mostrarán todos los ingredientes.
    private Estacion _estacionActual = Estacion.TODOELANHIO;


    // Constructor de la pestaña TemporadaPage, recibe un servicio de recetas como parámetro y lo asigna a la variable privada _service.
    // InicializeComponent() es un método que inicializa los comoponentes de la interfaz de usuario definidos en el archivo XAML asociado a esta clase.
    // _service = service; asigna el servicio recibido al apartado privado _service para que pueda ser utilizado en otros métodos de la clase.
    // CargarEstacion(Estacion.TODOELANHIO); llama al método CargarEstacion con el valor de TODOELANHIO para cargar los ingredientes de la temporada.
    public TemporadaPage(RecetaService service)
    {
        InitializeComponent();
        _service = service;
        CargarEstacion(Estacion.TODOELANHIO);
    }


    // Método privado que carga los ingredientes de la temporada según la estación seleccionada.
    private void CargarEstacion(Estacion estacion)
    {
        _estacionActual = estacion; // Actualiza la estación actual seleccionada con el valor recibido como parámetro.
        ActualizarBotonesEstacion(estacion); // Llama a ActualizarBotonesEstacion para cambiar el diseño de los botones según la estacion.
        var lista = _service.ConsultarCalendarioTemporada(estacion); // Consulta la lista de ingredientes de la temporada.

        LblEstacionSeleccionada.Text =  // Actualiza el texto de LblEstacionSeleccionada Mostrando la estacion seleccionada.
            $"Estación seleccionada: ({lista.Count} frutas y verduras registradas)"; // Muestra la lista de ingredientes de la temporada. 
        // {lista.Count} cuenta la cantidad de ingredientes de la temporada selecionada.


        StackIngredientesTemporada.Children.Clear(); // Limpia los elementos de StackIngredientesTemporada para que no se acumulen los ingredientes.
        // Children es una propiedad que representa la colección de elementos de un contenedor.



        // Recorre la lista de ingredientes de la temporada y crea un diseño para cada uno.
        foreach (var item in lista) 
        {

            // bool esFruta es una variable booleana que indica si el ingrediente es una fruta o no.
            bool esFruta = item.Tipo() == TipoVegetal.FRUTA; // item es un objeto de la clase Ingrediente, el método Tipo() devuelve el tipo de 
                                                            // vegetal o fruta, y se compara con el valor TipoVegetal.FRUTA para determinar si es fruta o no. 



            // Crea un borde que contiene un texto.
            var badgeTipo = new Border 
            {
                BackgroundColor = esFruta ? Color.FromArgb("#F5E5DF") : Color.FromArgb("#E8EDE4"), // si es fruta el color de fondo es #F5E5DF, si no es fruta el color es #E8EDE4
                Stroke = esFruta ? Color.FromArgb("#C8785A") : Color.FromArgb("#758B68"),  // si es fruta el color del borde es #C8785A, si no es fruta el color es #758B68
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 }, // strokeShape define la forma del borde, Microsoft.Maui.Controls.Shapes.RoundRectangle
                                                                                                      // crea un rectángulo redondeado con un radio de 8.
                Padding = new Thickness(10, 4), // Thickness define el espaciado interno del borde, en este caso 10 unidades a los lados y 4 unidades arriba y abajo.
                VerticalOptions = LayoutOptions.Center, // LayoutOptions.Center indica que el borde se centrará verticalmente dentro de su contenedor.

                Content = new Label // Content define el contenido del borde, en este caso un Label que muestra el tipo de vegetal o fruta y la estación principal.
                {
                    Text = $"{item.TipoTexto}  •  {item.EstacionPrincipal()}", // item.TipoTexto devuelve el texto del tipo de vegetal/fruta, y item.EstacionPrincipal() devuelve la estacion prinicipal.
                    FontSize = 12, //tamaño de la fuente (texto) del label.
                    FontAttributes = FontAttributes.Bold, // FontAttributes.Bold indica que el texto del label se mostrará en negrita.
                    TextColor = Color.FromArgb("#283B2D") // FromArgb es un método que crea un color a partir de un valor hexadecimal, como ("#283B2D") que es un color verde oscuro.
                }
            };



            // Crea un Grid que contiene tres columnas, una para el icono, otra para el nombre y otra para el badge.
            var fila = new Grid 
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto }, //GridLength es una estructura que define el tamaño de una columna o fila de un Grid. y con auto se ajusta al contenido.
                    new ColumnDefinition { Width = GridLength.Star }, // Star indica que la columna ocupará el espacio restante disponible en el Grid.
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 14  // Define el espacion entre columnas.
            };


            // Agrega el icono del vegetal/fruta, nombre etc 
            fila.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#E8EDE4"), // --> color fondo 
                Stroke = Color.FromArgb("#758B68"), // --> color borde
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 }, // --> forma del borde, rectángulo redondeado con radio de 14
                WidthRequest = 56, // --> ancho del borde
                HeightRequest = 56, // --> alto del borde

                Content = new Label // --> contenido del borde, un label que muestra el icono del vegetal/fruta
                {
                    Text = item.IconoVegetal,
                    FontSize = 28, // tamaño letra del label.
                    HorizontalOptions = LayoutOptions.Center, // centra label horizontalmente.
                    VerticalOptions = LayoutOptions.Center // centra label verticalmente.
                }
            }, 0, 0); // Agrega el borde a la primera columna del Grid (columna 0, fila 0).



            // Agrega el nombre del vegetal/fruta, pais de origen y zona etc
            fila.Add(new VerticalStackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.Center, // LayoutOptions.Center --> centra el contenido verticalmente.
                Children =  // Children es una propiedad que representa la colección de elementos de un contenedor.
                {
                    new Label
                    {
                        Text = item.NombreFrutaVerdura(),  // item.NombreFrutaVerdura() devuelve el nombre del vegetal/fruta.
                        FontSize = 17,
                        FontAttributes = FontAttributes.Bold, // FontAttributes.Bold indica que el texto del label se mostrará en negrita.
                        TextColor = Color.FromArgb("#283B2D")
                    },
                    new Label
                    {
                        Text = $"País de origen: {item.PaisOrigen()}  •  Zona de cultivo: {item.ZonaCultivo()}", // item.PaisOrigen() devuelve el país de origen del vegetal/fruta, y item.ZonaCultivo() devuelve la zona de cultivo.
                        FontSize = 13,
                        TextColor = Color.FromArgb("#5A6B5C")
                    }
                }
            }, 1, 0);

            fila.Add(badgeTipo, 2, 0); // Agrega el badgeTipo a la tercera columna del Grid (columna 2, fila 0). badgeTipo es el borde que contiene el tipo de vegetal/fruta y la estación principal.

            StackIngredientesTemporada.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Padding = 14,
                Content = fila
            });
        }
    }


    // Método privado que actualiza el diseño de los botones de estación según la estación activa.
    private void ActualizarBotonesEstacion(Estacion activa)
    {
        PintarBotonEstacion(BtnEstacionTodo, activa == Estacion.TODOELANHIO); // --> cambia el diseño a TODOELANHIO.
        PintarBotonEstacion(BtnEstacionPrimavera, activa == Estacion.PRIMAVERA); // --> cambia el diseño a PRIMAVERA.
        PintarBotonEstacion(BtnEstacionVerano, activa == Estacion.VERANO); // --> cambia el diseño a VERANO.
        PintarBotonEstacion(BtnEstacionOtonio, activa == Estacion.OTONIO); // --> cambia el diseño a OTONIO.
        PintarBotonEstacion(BtnEstacionInvierno, activa == Estacion.INVIERNO); // --> cambia el diseño a INVIERNO.
    }


    // Método privado que cambia el diseño de un botón según si está activo o no.
    private static void PintarBotonEstacion(Button boton, bool activo) // static puede ser llamado sin necesidad de crear una instancia de la clase, no depende de la clase.(si de la funcion).
    {
        if (boton == null) return;  // --> si el boton es nulo, no hace nada y sale del método.
        boton.BackgroundColor = activo ? Color.FromArgb("#758B68") : Color.FromArgb("#ECE8DC"); // --> si el boton está activo, el color de fondo es #758B68, si no es activo el color de fondo es #ECE8DC
        boton.TextColor = activo ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#283B2D"); // --> si el boton está activo, el color del texto es #FFFFFF, si no es activo el color del texto es #283B2D
    }

    private void OnTodoElAnioClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.TODOELANHIO); // carga la estacion TODOELANHIO cuando se hace clic en el botón correspondiente.
    private void OnPrimaveraClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.PRIMAVERA); // carga la estacion PRIMAVERA cuando se hace clic en el botón correspondiente.
    private void OnVeranoClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.VERANO); // carga la estacion VERANO cuando se hace clic en el botón correspondiente.
    private void OnOtonioClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.OTONIO); // carga la estacion OTONIO cuando se hace clic en el botón correspondiente. 
    private void OnInviernoClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.INVIERNO); // carga la estacion INVIERNO cuando se hace clic en el botón correspondiente.



    // Método privado que dispara un evento para filtrar las recetas en el feed según la estación actual seleccionada.
    private void OnFiltrarFeedPorEstacionActualClicked(object? sender, EventArgs e) 
    {
        _service.DispararFiltroEstacionEnFeed(_estacionActual); // pasa la estación actual al servicio para que filtre las recetas en el feed.
    }
}
