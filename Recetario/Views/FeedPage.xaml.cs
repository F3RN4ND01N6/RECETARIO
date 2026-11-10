using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;


// Partial significa que la clase FeedPage está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
// FeedPage representa la pantalla principal del feed global donde se exploran, filtran y visualizan todas las recetas públicas.
public partial class FeedPage : ContentPage
{

    // readonly significa que estas variables solo pueden ser asignadas en el constructor de la clase y no pueden ser modificadas.
    private readonly RecetaService _service; // servicio central de recetas para consultar el feed, favoritos y filtros.

    // Variable booleana que previene ejecuciones repetidas de eventos de filtro durante la inicialización de los componentes.
    private bool _inicializando = true;


    // FeedPage sirve para inicializar los componentes de la interfaz, configurar los filtros desplegables y suscribirse a eventos de datos.
    public FeedPage(RecetaService service)
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los XAML asociados a esta clase.
        _service = service; // asigna servicio de recetas a variable privada para usarlo en toda la clase.

        ConfigurarSelectores(); // llena los pickers desplegables con las opciones de filtros.
        _service.DatosActualizados += RefrescarVista; // suscribe el refresco de la vista al evento de datos actualizados del servicio.
        _service.SolicitudFiltrarFeedPorEstacion += AplicarFiltroDesdeCalendario; // escucha solicitudes de filtrado provenientes de la pestaña Temporada.

        _inicializando = false; // finaliza la etapa de inicializacion para permitir que los filtros refresquen la vista.
        RefrescarVista(); // realiza la primera carga de recetas en pantalla.
    }


    /// <summary>
    /// LLAMADO DE ENUMERADORES 
    /// </summary>
    // Metodo privado que configura las listas de opciones de cada Picker desplegable en la interfaz.
    private void ConfigurarSelectores()
    {
        // Asigna opciones al selector de Tipo de Vegetal/Fruta y selecciona "TODOS" por defecto.
        PickerVegetal.ItemsSource = new List<string> { "TODOS", "FRUTA", "VERDURA", "OTRO" };
        PickerVegetal.SelectedIndex = 0; // indice 0 equivale a "TODOS".

        // Asigna opciones al selector de Estación y selecciona "TODAS" por defecto.
        PickerEstacion.ItemsSource = new List<string> { "TODAS", "PRIMAVERA", "VERANO", "OTOÑO", "INVIERNO", "TODO EL AÑO" };
        PickerEstacion.SelectedIndex = 0;

        // Asigna opciones al selector de País de Origen y selecciona "TODOS" por defecto.
        PickerOrigen.ItemsSource = new List<string> { "TODOS", "CHILE", "ARGENTINA", "PERU", "COLOMBIA", "OTRO" };
        PickerOrigen.SelectedIndex = 0;

        // Asigna opciones al selector de Dificultad y selecciona "TODAS" por defecto.
        PickerDificultad.ItemsSource = new List<string> { "TODAS", "FACIL", "MEDIA", "DIFICIL" };
        PickerDificultad.SelectedIndex = 0;
    }


    // override significa que este metodo reemplaza el metodo base OnAppearing() de ContentPage, ejecutandose cada vez que la pagina se muestra.
    protected override void OnAppearing()
    {
        base.OnAppearing(); // llama al metodo de la clase base para mantener el ciclo de vida normal.
        RefrescarVista(); // actualiza la lista de recetas públicas cada vez que el usuario ingresa a la pestaña.
    }


    // Metodo privado que aplica un filtro de estacion recibido desde la pestaña de Temporada.
    private void AplicarFiltroDesdeCalendario(Estacion estacion)
    {
        _inicializando = true; // bloquea disparos intermedios del evento de cambio
        PickerEstacion.SelectedItem = estacion.ToString(); // selecciona la estacion recibida en el picker
        _inicializando = false; // reactiva los eventos
        RefrescarVista(); // refresca las tarjetas con el nuevo filtro aplicado
    }


    // Metodo que se ejecuta cada vez que el usuario cambia la seleccion en algun picker o campo de filtro.
    private void OnFiltroCambiado(object? sender, EventArgs e)
    {
        if (_inicializando) // si la pagina aun se esta configurando no ejecuta nada para evitar llamadas innecesarias.
            return;
        RefrescarVista(); // refresca los resultados en pantalla con los filtros activos.
    }


    // Metodo principal que lee los filtros seleccionados, consulta al servicio y renderiza las recetas públicas.
    private void RefrescarVista()
    {
        if (_inicializando) // si esta en fase de inicializacion omite la actualizacion.
            return;

        LblRolActivo.Text = $"Actor: {_service.UsuarioActivo.ObtenerTipoPerfil()}"; // muestra en la cabecera el rol o perfil del usuario actual.

        // Convierte el valor seleccionado en PickerVegetal al enum TipoVegetal, o null si esta en "TODOS".
        TipoVegetal? tipoVeg = PickerVegetal.SelectedIndex > 0
            ? Enum.Parse<TipoVegetal>(PickerVegetal.SelectedItem.ToString()!)
            : null;

        // Convierte el valor seleccionado en PickerEstacion al enum Estacion, o null si esta en "TODAS".
        Estacion? estacion = PickerEstacion.SelectedIndex > 0
            ? Enum.Parse<Estacion>(PickerEstacion.SelectedItem.ToString()!)
            : null;

        // Convierte el valor seleccionado en PickerOrigen al enum Origen, o null si esta en "TODOS".
        Origen? origen = PickerOrigen.SelectedIndex > 0
            ? Enum.Parse<Origen>(PickerOrigen.SelectedItem.ToString()!)
            : null;

        // Convierte el valor seleccionado en PickerDificultad al enum NivelDificultad, o null si esta en "TODAS".
        NivelDificultad? dificultad = PickerDificultad.SelectedIndex > 0
            ? Enum.Parse<NivelDificultad>(PickerDificultad.SelectedItem.ToString()!)
            : null;

        // Construye el objeto FiltroReceta con todos los parametros ingresados por el usuario.
        var filtro = new FiltroReceta(
            tipoFrutaOVerdura: tipoVeg,
            nombreIngrediente: EntryBusqueda.Text ?? string.Empty, // texto de busqueda o cadena vacia si es nulo
            estacionTemporada: estacion,
            pais: origen,
            categoriaFiltro: string.Empty,
            dietaFiltro: string.Empty,
            dificultad: dificultad,
            soloVerificadasPorChef: ChkSoloChefs.IsChecked); // valor del checkbox para ver solo recetas verificadas

        // Consulta al servicio de recetas para obtener las recetas publicas que coinciden con los criterios.
        var resultados = _service.FiltrarFeedGlobal(filtro);

        if (ChkSoloFavoritas.IsChecked) // si el checkbox de favoritas esta activo filtra solo las marcadas como favoritas con LINQ Where.
        {
            resultados = resultados.Where(r => _service.EsRecetaFavorita(r.ObtenerIdentificador())).ToList();
        }

        // Actualiza el texto de estadisticas del feed con la cantidad de resultados encontrados.
        LblEstadisticasFeed.Text =
            $"Mostrando {resultados.Count} de {_service.TotalRecetasPublicas} recetas públicas  •  ♥ {_service.TotalFavoritas} favoritas  •  Rol activo: {_service.UsuarioActivo.NombreUsuario()}";

        RenderizarTarjetas(resultados); // genera dinamicamente los controles visuales para cada receta en la lista.
    }


    // Metodo privado que recorre las recetas publicas y genera una tarjeta visual (Border + Grid) para cada una.
    private void RenderizarTarjetas(List<RecetaPublica> recetas)
    {
        StackTarjetasFeed.Children.Clear(); // limpia las tarjetas existentes antes de redibujar.

        if (recetas.Count == 0) // si la lista esta vacia muestra un mensaje informativo indicando que no hay coincidencias.
        {
            StackTarjetasFeed.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#FFFFFF"), // fondo blanco
                Stroke = Color.FromArgb("#D8D3C4"), // borde gris claro
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Padding = 24,
                Content = new Label
                {
                    Text = "No se encontraron recetas con los filtros seleccionados. Prueba limpiando los filtros.",
                    HorizontalTextAlignment = TextAlignment.Center, // alinea texto al centro
                    TextColor = Color.FromArgb("#283B2D"),
                    FontSize = 15
                }
            });
            return;
        }

        bool esAdmin = _service.UsuarioActivo is Administrador; // valida si el usuario activo tiene rol de Administrador para mostrar botones de moderacion.

        foreach (var receta in recetas) // recorre cada receta publica devuelta por la busqueda.
        {
            bool esFav = _service.EsRecetaFavorita(receta.ObtenerIdentificador()); // comprueba si la receta es favorita del usuario.

            // Badge visual que indica si la receta fue verificada por un chef y su promedio de estrellas.
            var badgeVerificacion = new Border
            {
                BackgroundColor = receta.VerificadaPorChef() ? Color.FromArgb("#E8EDE4") : Color.FromArgb("#F7F4EA"),
                Stroke = receta.VerificadaPorChef() ? Color.FromArgb("#758B68") : Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(8, 4),
                HorizontalOptions = LayoutOptions.Start,
                Content = new Label
                {
                    Text = $"{receta.VerificacionBadgeTexto}  •  {receta.PromedioEstrellasTexto}",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#283B2D")
                }
            };

            // Botón Favorita en Terracota (#C8785A) según paleta ("Favoritos y detalles")
            var btnFav = new Button
            {
                Text = esFav ? "♥ Favorita" : "♡ Favorita", // muestra corazon lleno o vacio segun estado
                BackgroundColor = esFav ? Color.FromArgb("#C8785A") : Color.FromArgb("#F5E5DF"),
                TextColor = esFav ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#C8785A"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 14,
                Padding = new Thickness(14, 6),
                FontSize = 12
            };
            btnFav.Clicked += (_, _) => // evento al presionar favorita
            {
                _service.AlternarFavorita(receta.ObtenerIdentificador()); // alterna estado de favorita en el servicio
            };

            // Botón Ver Receta en Terracota (#C8785A) o Verde Salvia (#758B68) con texto blanco
            var btnDetalle = new Button
            {
                Text = "Ver Receta y Calificar",
                BackgroundColor = Color.FromArgb("#C8785A"),
                TextColor = Color.FromArgb("#FFFFFF"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 14,
                Padding = new Thickness(16, 6),
                FontSize = 12
            };
            btnDetalle.Clicked += async (_, _) => // evento asincrono al presionar ver receta
            {
                await Navigation.PushModalAsync(new DetalleRecetaPage(receta, _service)); // abre la pagina de detalle modal
            };

            // Contenedor horizontal que agrupa los botones de la tarjeta
            var barraBotones = new HorizontalStackLayout
            {
                Spacing = 8,
                Margin = new Thickness(0, 6, 0, 0),
                Children = { btnDetalle, btnFav }
            };

            if (esAdmin) // si el usuario es administrador agrega un boton exclusivo de moderacion y borrado
            {
                var btnAdminEliminar = new Button
                {
                    Text = "Eliminar",
                    BackgroundColor = Color.FromArgb("#283B2D"), // verde muy oscuro
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 14,
                    Padding = new Thickness(12, 6),
                    FontSize = 12
                };
                btnAdminEliminar.Clicked += async (_, _) => // evento de confirmacion de eliminacion por moderacion
                {
                    bool confirmar = await DisplayAlertAsync(
                        "Moderación de Administrador",
                        $"¿Forzar eliminación de '{receta.Titulo}' del Feed Público Global?",
                        "Eliminar", "Cancelar");
                    if (confirmar)
                    {
                        _service.EliminarRecetaPublicaComoAdmin(receta.ObtenerIdentificador()); // elimina la receta forzadamente
                    }
                };
                barraBotones.Children.Add(btnAdminEliminar); // añade el boton de admin a la barra
            }

            // Grid de dos columnas: una para la imagen y otra para la informacion de la receta
            var gridContenido = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto }, // columna auto para el tamaño de la foto
                    new ColumnDefinition { Width = GridLength.Star } // columna star para expandir el texto
                },
                ColumnSpacing = 16 // separacion entre columnas
            };

            // Cuadro contenedor con bordes redondeados para la imagen del platillo
            var cuadroIcono = new Border
            {
                BackgroundColor = Color.FromArgb("#F7F4EA"),
                Stroke = Color.FromArgb("#758B68"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                WidthRequest = 150, // ancho 150
                HeightRequest = 150, // alto 150
                Padding = 0,
                VerticalOptions = LayoutOptions.Center,
                Content = new Image
                {
                    Source = receta.ImagenPlatillo, // ruta o url de la imagen
                    Aspect = Aspect.AspectFill, // llena el contenedor recortando si es necesario
                    WidthRequest = 150,
                    HeightRequest = 150
                }
            };

            // Columna vertical con los textos, badges y botones de la receta
            var stackInfo = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    badgeVerificacion,
                    new Label // titulo principal de la receta
                    {
                        Text = receta.Titulo,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#283B2D")
                    },
                    new Label // subtitulo con categoria, dieta, ubicacion y estacion
                    {
                        Text = $"{receta.CategoriaYDietaTexto}  |  {receta.UbicacionEstacionTexto}",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#5A6B5C")
                    },
                    new Label // metadatos de tiempo, dificultad y etiquetas
                    {
                        Text = $"{receta.MetaResumenTexto}  |  {receta.EtiquetasTexto}",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#758B68")
                    },
                    barraBotones // agrega la barra con los botones de accion
                }
            };

            gridContenido.Add(cuadroIcono, 0, 0); // coloca la imagen en la columna 0 fila 0
            gridContenido.Add(stackInfo, 1, 0); // coloca la informacion en la columna 1 fila 0

            // Tarjeta exterior con borde y fondo blanco
            var tarjeta = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = receta.DestacadaEnFeed() ? Color.FromArgb("#758B68") : Color.FromArgb("#D8D3C4"), // borde verde si es destacada
                StrokeThickness = receta.DestacadaEnFeed() ? 2 : 1, // grosor mayor si es destacada
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 },
                Padding = 16,
                Content = gridContenido
            };

            StackTarjetasFeed.Children.Add(tarjeta); // agrega la tarjeta al contenedor del feed
        }
    }


    // async metodo que ejecuta la segunda sobrecarga del metodo de busqueda filtrando unicamente por Estacion y Pais de Origen.
    private async void OnSobrecargaEstacionPaisClicked(object? sender, EventArgs e)
    {
        Estacion est = PickerEstacion.SelectedIndex > 0
            ? Enum.Parse<Estacion>(PickerEstacion.SelectedItem.ToString()!)
            : _service.UsuarioActivo.ConfiguracionPerfil().EstacionPreferida(); // toma la estacion seleccionada o la preferida del perfil.

        Origen pais = PickerOrigen.SelectedIndex > 0
            ? Enum.Parse<Origen>(PickerOrigen.SelectedItem.ToString()!)
            : _service.UsuarioActivo.ConfiguracionPerfil().Origen(); // toma el pais seleccionado o el preferido del perfil.

        // Invoca la segunda sobrecarga exacta del Diagrama de Clases: FiltrarFeedGlobal(Estacion, Origen)
        var filtradas = _service.FiltrarFeedGlobal(est, pais);
        RenderizarTarjetas(filtradas); // dibuja las recetas obtenidas por la sobrecarga

        LblEstadisticasFeed.Text =
            $"Sobrecarga FiltrarFeedGlobal({est}, {pais}) ejecutada: {filtradas.Count} recetas encontradas.";

        await DisplayAlertAsync("Sobrecarga de Método Ejecutada", // muestra alerta con los resultados de la sobrecarga
            $"Se invocó RecetaService.FiltrarFeedGlobal(Estacion.{est}, Origen.{pais}).\nResultados encontrados: {filtradas.Count}",
            "Aceptar");
    }


    // Metodo que reinicia todos los pickers y campos de busqueda a sus valores iniciales.
    private void OnLimpiarFiltrosClicked(object? sender, EventArgs e)
    {
        _inicializando = true; // bloquea ejecucion mientras se limpian los campos
        EntryBusqueda.Text = string.Empty; // limpia texto de busqueda
        PickerVegetal.SelectedIndex = 0; // resetea a "TODOS"
        PickerEstacion.SelectedIndex = 0; // resetea a "TODAS"
        PickerOrigen.SelectedIndex = 0; // resetea a "TODOS"
        PickerDificultad.SelectedIndex = 0; // resetea a "TODAS"
        ChkSoloChefs.IsChecked = false; // desmarca solo chefs
        ChkSoloFavoritas.IsChecked = false; // desmarca solo favoritas
        _inicializando = false; // habilita eventos nuevamente

        RefrescarVista(); // refresca la vista mostrando todas las recetas sin filtros
    }


    // async metodo para recargar manualmente el archivo JSON del catalogo publico global.
    private async void OnRecargarJsonGlobalClicked(object? sender, EventArgs e)
    {
        _service.CargarRespaldoGlobalManual(); // deserializa el JSON global de recetas
        await DisplayAlertAsync("Respaldo Global", // muestra alerta con la ruta del archivo deserializado
            $"Catálogo global deserializado correctamente desde:\n{_service.ObtenerRutaJsonPublico()}",
            "Aceptar");
    }
}
