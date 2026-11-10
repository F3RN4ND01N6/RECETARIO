using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;

// partial es para que la clase se pueda dividir en varios archivos, en este caso el archivo xaml y el archivo xaml.cs
public partial class MisRecetasPage : ContentPage
{

    // readonly es para que la variable no pueda ser modificada después de ser inicializada en el constructor
    private readonly RecetaService _service;


    // Constructor de la página, recibe el servicio de recetas como parámetro
    public MisRecetasPage(RecetaService service)
    {
        InitializeComponent(); // Inicializa los componentes de la página (definidos en el archivo XAML)
        _service = service; // Asigna el servicio de recetas a la variable privada _service
        _service.DatosActualizados += RefrescarLista; // Suscribe el método RefrescarLista al evento DatosActualizados del servicio de recetas
        RefrescarLista(); // Llama al método RefrescarLista para actualizar la lista de recetas privadas al cargar la página
    }


    // Sobrescribe el método OnAppearing para refrescar la lista de recetas privadas cada vez que la página aparece
    protected override void OnAppearing()
    {
        base.OnAppearing(); // Llama al método base OnAppearing para mantener el comportamiento predeterminado de la página
        RefrescarLista(); // Llama al método RefrescarLista para actualizar la lista de recetas privadas cada vez que la página aparece
    }


    // Método que se ejecuta cuando cambia el texto en el Entry de búsqueda de recetas privadas
    private void OnBusquedaPrivadaChanged(object? sender, TextChangedEventArgs e)
    {
        RefrescarLista(); // Llama al método RefrescarLista para actualizar la lista de recetas privadas según el texto ingresado en el Entry de búsqueda
    }



    // Método que refresca la lista de recetas privadas según el filtro de búsqueda
    private void RefrescarLista() 
    {

        // Crea un filtro de recetas privadas usando el texto ingresado en el Entry de búsqueda, o una cadena vacía si no hay texto
        var filtro = new FiltroReceta(nombreIngrediente: EntryBuscarPrivada.Text ?? string.Empty);

        var privadas = _service.FiltrarRecetasPrivadas(_service.UsuarioActivo.ObtenerIdentificador(), filtro); // Obtiene la lista de recetas privadas filtradas según
                                                                                                               // el filtro creado

        LblContadorPrivadas.Text = $"Total en recetario privado: {privadas.Count} recetas  •  Actor activo: {_service.UsuarioActivo.ObtenerTipoPerfil()}"; // Actualiza el texto
                                                                                                                                                           // del Label que muestra el contador de recetas privadas y el tipo de perfil del usuario activo


        // Limpia el StackLayout que contiene las recetas privadas antes de agregar las nuevas recetas filtradas
        StackRecetasPrivadas.Children.Clear();

        if (privadas.Count == 0) // Si no hay recetas privadas que coincidan con el filtro, muestra un mensaje indicando que no hay recetas privadas
        {
            StackRecetasPrivadas.Children.Add(new Border // Agrega un Border al StackLayout que contiene las recetas privadas
            {
                BackgroundColor = Color.FromArgb("#FFFFFF"), // Color de fondo blanco
                Stroke = Color.FromArgb("#D8D3C4"), // Color del borde
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 }, // Forma del borde con esquinas redondeadas
                Padding = 22, // Espaciado interno del Border
                Content = new Label // Agrega un Label al Border que contiene el mensaje indicando que no hay recetas privadas
                {
                    // Texto del Label que indica que no hay recetas privadas
                    Text = "No hay recetas privadas que coincidan. Usa el botón 'Nueva Receta' o la pestaña Crear para agregar una.",
                    HorizontalTextAlignment = TextAlignment.Center, // Alineación horizontal del texto centrada, TextAlignment es alinear texto en este caso horizontalmente
                    TextColor = Color.FromArgb("#283B2D") // Color del texto
                }
            });
            return;
        }

        
        // Recorre la lista de recetas privadas y crea una tarjeta visual completa para cada una con sus botones de acción.
        foreach (var receta in privadas)
        {
            // Boton para abrir la vista modal de detalle de la receta seleccionada.
            var btnVer = new Button
            {
                Text = "Ver Detalle",
                BackgroundColor = Color.FromArgb("#C8785A"), // --> color de fondo terracota
                TextColor = Color.FromArgb("#FFFFFF"), // --> color de texto blanco
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12, // esquinas redondeadas del boton
                Padding = new Thickness(12, 6),
                FontSize = 12,
                Margin = new Thickness(0, 0, 5, 5) // margen exterior derecho e inferior
            };
            btnVer.Clicked += async (_, _) => // evento asincrono al hacer clic para abrir la pagina modal de detalle.
            {
                await Navigation.PushModalAsync(new DetalleRecetaPage(receta, _service)); // PushModalAsync() muestra DetalleRecetaPage de forma modal superpuesta.
            };

            // Boton para cargar la receta seleccionada en el formulario de edicion.
            var btnEditar = new Button
            {
                Text = "Editar Receta",
                BackgroundColor = Color.FromArgb("#758B68"), // --> color de fondo verde olivo
                TextColor = Color.FromArgb("#FFFFFF"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                FontSize = 12,
                Margin = new Thickness(0, 0, 5, 5)
            };
            btnEditar.Clicked += (_, _) => // evento al hacer clic para disparar la edicion de la receta.
            {
                _service.DispararEdicionReceta(receta); // notifica al servicio para cargar la receta en CrearEditarRecetaPage y cambiar de pestaña.
            };

            // Boton para publicar la receta privada en el feed global como receta oficial certificada.
            var btnPublicarChef = new Button
            {
                Text = "Publicar en Feed Global",
                BackgroundColor = Color.FromArgb("#E8EDE4"), // --> color de fondo claro suave
                TextColor = Color.FromArgb("#283B2D"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                FontSize = 12,
                Margin = new Thickness(0, 0, 5, 5)
            };
            btnPublicarChef.Clicked += async (_, _) => // evento asincrono para convertir y publicar la receta en el feed global.
            {
                // Convierte el usuario activo a CreadorEspecialista o usa el perfil certificado por defecto mediante operador null-coalescing ??.
                var chef = _service.UsuarioActivo as CreadorEspecialista ?? _service.PerfilCreadorEspecialista;
                var recetaPublica = chef.PublicarRecetaOficial(receta); // transforma la receta privada en una instancia de RecetaPublica.
                _service.PublicarRecetaEnFeed(recetaPublica, chef); // guarda y serializa la receta en el feed publico global.

                await DisplayAlertAsync("Receta Publicada en Feed Global", // muestra alerta informativa del exito de la publicacion.
                    $"'{recetaPublica.Titulo}' fue certificada por {chef.NombreUsuario()} y serializada en el archivo Global.",
                    "Ver en Feed");

                _service.NavegarAPestana("Feed"); // navega automaticamente a la pestaña del Feed para ver la receta publicada.
            };

            // Boton para eliminar la receta privada del almacenamiento local.
            var btnEliminar = new Button
            {
                Text = "Eliminar",
                BackgroundColor = Color.FromArgb("#F5E5DF"), // --> color de fondo rojizo suave
                TextColor = Color.FromArgb("#A85E42"), // --> color de texto rojo terracota
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                FontSize = 12,
                Margin = new Thickness(0,0,5,5)
            };
            btnEliminar.Clicked += async (_, _) => // evento asincrono para confirmar y eliminar la receta.
            {
                bool confirmar = await DisplayAlertAsync( // solicita confirmacion al usuario mediante un cuadro de dialogo si/no.
                    "Eliminar Receta Privada",
                    $"¿Deseas eliminar '{receta.Titulo}' de tu recetario local?",
                    "Eliminar", "Cancelar");
                if (confirmar) // si el usuario presiona "Eliminar" se ejecuta la eliminación.
                {
                    _service.EliminarRecetaPrivada(receta.ObtenerIdentificador()); // elimina la receta del servicio y del JSON local.
                }
            };

            // Card principal que envuelve toda la informacion de la receta privada.
            var card = new Border
            {
                BackgroundColor = Colors.White, // fondo blanco de la tarjeta
                Stroke = Color.FromArgb("#D8D3C4"), // borde gris suave
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 }, // esquinas redondeadas con radio de 18
                Padding = 16, // espaciado interior de 16
                Content = new VerticalStackLayout // distribucion vertical de los elementos internos
                {
                    Spacing = 6, // espaciado entre elementos
                    Children =
                    {
                        new HorizontalStackLayout // fila horizontal con imagen y datos principales
                        {
                            Spacing = 14,
                            Children =
                            {
                                new Border // contenedor para la imagen de la receta
                                {
                                    BackgroundColor = Color.FromArgb("#F7F4EA"),
                                    Stroke = Color.FromArgb("#758B68"),
                                    StrokeThickness = 1.5,
                                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                                    WidthRequest = 96, // ancho fijo de 96
                                    HeightRequest = 96, // alto fijo de 96
                                    Padding = 0,
                                    VerticalOptions = LayoutOptions.Center,
                                    Content = new Image // control de imagen que muestra la foto de la receta
                                    {
                                        Source = receta.ImagenPlatillo,
                                        Aspect = Aspect.AspectFill, // AspectFill hace que la imagen llene el contenedor recortando si es necesario
                                        WidthRequest = 96,
                                        HeightRequest = 96
                                    }
                                },
                                new VerticalStackLayout // columna con el titulo y etiquetas de la receta
                                {
                                    VerticalOptions = LayoutOptions.Center,
                                    Children =
                                    {
                                        new Label
                                        {
                                            Text = receta.Titulo, // titulo de la receta
                                            FontSize = 18,
                                            FontAttributes = FontAttributes.Bold,
                                            TextColor = Color.FromArgb("#283B2D")
                                        },
                                        new Label
                                        {
                                            Text = $"{receta.BadgePrivacidadTexto}  •  {receta.CategoriaYDietaTexto}  •  {receta.UbicacionEstacionTexto}",
                                            FontSize = 12,
                                            TextColor = Color.FromArgb("#5A6B5C")
                                        }
                                    }
                                }
                            }
                        },
                        new Label // muestra las notas personales de la receta si existen
                        {
                            Text = $"Notas personales: {(string.IsNullOrWhiteSpace(receta.NotasPersonales()) ? "Sin notas adicionales" : receta.NotasPersonales())}",
                            FontSize = 13,
                            TextColor = Color.FromArgb("#283B2D"),
                            BackgroundColor = Color.FromArgb("#F7F4EA"),
                            Padding = new Thickness(10, 6)
                        },
                        new Label // muestra el resumen de la entidad (polimorfismo IResumible)
                        {
                            Text = receta.ObtenerResumenEntidad(),
                            FontSize = 11,
                            TextColor = Color.FromArgb("#5A6B5C")
                        },
                        new FlexLayout // contenedor flexible para los botones de accion que permite saltos de linea automaticos
                        {
                            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, // Wrap permite que los botones se acomoden en multiples lineas si no caben
                            JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start,
                            Children = { btnVer, btnEditar, btnPublicarChef, btnEliminar } // agrega los botones de accion a la barra flexible
                        }
                    }
                }
            };

            StackRecetasPrivadas.Children.Add(card); // agrega la tarjeta completa al contenedor de la pagina
        }
    }


    // Metodo que navega a la pestaña de Crear/Editar Receta al presionar el boton correspondiente.
    private void OnIrACrearClicked(object? sender, EventArgs e)
    {
        _service.NavegarAPestana("Crear"); // solicita al servicio cambiar a la pestaña CrearEditar
    }


    // async metodo para guardar manualmente un respaldo en formato JSON en el almacenamiento local.
    private async void OnGuardarJsonLocalClicked(object? sender, EventArgs e)
    {
        _service.GuardarRespaldoPrivadoManual(); // ejecuta la serializacion a JSON del recetario privado
        await DisplayAlertAsync("Respaldo Local Guardado", // muestra alerta con la ruta fisica del archivo generado
            $"Recetario privado serializado exitosamente en:\n{_service.ObtenerRutaJsonPrivado()}",
            "Aceptar");
    }


    // async metodo para recargar manualmente los datos desde el archivo JSON local.
    private async void OnCargarJsonLocalClicked(object? sender, EventArgs e)
    {
        _service.CargarRespaldoPrivadoManual(); // ejecuta la deserializacion desde el archivo JSON
        await DisplayAlertAsync("Respaldo Local Cargado", // muestra alerta confirmando la carga del archivo
            $"Recetario privado deserializado desde:\n{_service.ObtenerRutaJsonPrivado()}",
            "Aceptar");
    }
}
