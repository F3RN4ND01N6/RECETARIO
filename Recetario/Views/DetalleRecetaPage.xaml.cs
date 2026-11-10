using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;


// Partial significa que la clase DetalleRecetaPage está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
// DetalleRecetaPage muestra toda la información detallada de una receta específica: ingredientes, pasos, calificaciones y estado de favorita.
public partial class DetalleRecetaPage : ContentPage
{

    // readonly significa que estas variables solo pueden ser asignadas en el constructor de la clase y no pueden ser modificadas.
    private readonly Receta _receta; // objeto de tipo Receta que contiene los datos de la receta seleccionada.
    private readonly RecetaService _service; // servicio central de recetas para consultar y modificar datos.


    // DetalleRecetaPage sirve para inicializar los componentes y recibir la receta a mostrar y el servicio de recetas.
    public DetalleRecetaPage(Receta receta, RecetaService service)
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los XAML asociados a esta clase.
        _receta = receta; // asigna la receta seleccionada a la variable privada para usarla en toda la clase.
        _service = service; // asigna servicio de recetas a variable privada para usarlo en toda la clase.

        // ItemsSource asigna la lista de opciones de puntuacion que se mostraran en el desplegable (Picker).
        PickerPuntaje.ItemsSource = new List<string>
        {
            "5 ★ - Excelente",
            "4 ★ - Muy Buena",
            "3 ★ - Buena",
            "2 ★ - Regular",
            "1 ★ - Mejorable"
        };
        PickerPuntaje.SelectedIndex = 0; // selecciona por defecto la primera opcion (5 estrellas).

        CargarVista(); // llama al metodo privado para cargar y renderizar toda la informacion en la pantalla.
    }


    // Metodo privado que distribuye y muestra todos los datos de la receta en los controles de la interfaz.
    private void CargarVista()
    {
        ImgPlatillo.Source = _receta.ImagenPlatillo; // asigna la URL o ruta de la imagen del platillo al control Image.
        LblTitulo.Text = _receta.Titulo; // muestra el titulo de la receta.
        LblSubtitulo.Text = $"{_receta.CategoriaYDietaTexto}  •  {_receta.UbicacionEstacionTexto}"; // muestra categoria, dieta, ubicacion y estacion.
        LblMeta.Text = $"{_receta.MetaResumenTexto}  •  {_receta.EtiquetasTexto}"; // muestra tiempo, dificultad, porciones y etiquetas.
        LblResumenEntidad.Text = _receta.ObtenerResumenEntidad(); // muestra resumen polimórfico de la entidad receta.

        ActualizarBotonFavorita(); // actualiza el texto y color del boton de favoritos segun el estado actual.
        RenderizarIngredientes(); // dibuja dinámicamente la lista de ingredientes en la pantalla.
        RenderizarPasos(); // dibuja dinámicamente la lista de pasos interactivos de preparación.
        RenderizarCalificaciones(); // dibuja la sección y comentarios de calificaciones si la receta es publica.
    }


    // Metodo privado que cambia el estilo y texto del boton de favoritos segun si la receta ya esta guardada como favorita o no.
    private void ActualizarBotonFavorita()
    {
        bool esFav = _service.EsRecetaFavorita(_receta.ObtenerIdentificador()); // verifica con el servicio si la receta esta en la lista de favoritos.
        BtnFavorita.Text = esFav ? "♥ En tus Favoritas" : "♡ Marcar como Favorita"; // operador ternario para alternar el texto del boton.
        BtnFavorita.BackgroundColor = esFav ? Color.FromArgb("#C8785A") : Color.FromArgb("#F5E5DF"); // --> color de fondo del boton segun estado.
        BtnFavorita.TextColor = esFav ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#C8785A"); // --> color del texto del boton segun estado.
    }


    // Metodo privado que recorre la coleccion de ingredientes y crea dinámicamente una tarjeta visual para cada uno.
    private void RenderizarIngredientes()
    {
        StackIngredientes.Children.Clear(); // limpia los elementos del contenedor para evitar duplicados al recargar.
        foreach (var ing in _receta.Ingredientes) // recorre cada ingrediente registrado en la receta.
        {
            var fila = new Border // crea un contenedor visual con bordes redondeados para el ingrediente.
            {
                BackgroundColor = Color.FromArgb("#F7F4EA"), // --> color de fondo claro
                Stroke = Color.FromArgb("#D8D3C4"), // --> color de borde
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 }, // borde redondeado con radio de 10.
                Padding = new Thickness(12, 8), // espaciado interno: 12 horizontal y 8 vertical.
                Content = new Label // contenido interno del borde: etiqueta con el texto formateado del ingrediente.
                {
                    Text = $"• {ing.FormatearIngrediente()}", // FormatearIngrediente() devuelve la cantidad, unidad y nombre del ingrediente.
                    TextColor = Color.FromArgb("#283B2D"), // --> color de texto verde oscuro
                    FontSize = 14 // tamaño de la fuente
                }
            };
            StackIngredientes.Children.Add(fila); // agrega la tarjeta del ingrediente al StackLayout vertical.
        }
    }


    // Modificar el Tick de completado interrumpe el flujo de la funcion RenderizarPasos()
    // Renderiza la lista interactiva de pasos donde cada uno es un botón que se puede marcar o desmarcar.
    private void RenderizarPasos()
    {
        StackPasos.Children.Clear(); // limpia los pasos anteriores antes de volver a generarlos.
        foreach (var paso in _receta.Pasos) // recorre cada paso de preparación de la receta.
        {
            var btnPaso = new Button // crea un boton para representar el paso con su estado de completitud.
            {
                Text = $"{(paso.Completado() ? "✅" : "⬜")} {paso.FormatearPaso()}", // muestra un check si esta completado o un cuadro vacio si esta pendiente.
                BackgroundColor = paso.Completado() ? Color.FromArgb("#758B68") : Color.FromArgb("#F7F4EA"), // color verde si esta completado, claro si no.
                TextColor = paso.Completado() ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#283B2D"), // color blanco de texto si esta completado.
                CornerRadius = 10, // redondeo de las esquinas del boton.
                HorizontalOptions = LayoutOptions.Fill // expande el boton horizontalmente para ocupar todo el ancho disponible.
            };

            // Evento Clicked que se dispara al tocar el paso para alternar su estado de completado.
            btnPaso.Clicked += (_, _) =>
            {
                paso.MarcarCompletado(); // alterna el valor booleano de completado del paso.
                RenderizarPasos(); // vuelve a dibujar la lista de pasos para actualizar el diseño visual.
            };

            StackPasos.Children.Add(btnPaso); // agrega el boton del paso al contenedor vertical de pasos.
        }
    }


    // Metodo privado que muestra las calificaciones y reseñas unicamente si la receta es una instancia de RecetaPublica.
    private void RenderizarCalificaciones()
    {
        if (_receta is not RecetaPublica publica) // pattern matching para verificar si la receta NO es publica; si es privada oculta el panel.
        {
            PanelCalificaciones.IsVisible = false; // IsVisible en false oculta completamente el panel en la interfaz.
            return;
        }

        PanelCalificaciones.IsVisible = true; // IsVisible en true muestra el panel de calificaciones.
        StackResenas.Children.Clear(); // limpia las reseñas existentes en pantalla.

        var calificaciones = publica.ObtenerCalificaciones(); // obtiene la lista de objetos Calificacion de la receta publica.
        if (calificaciones.Count == 0) // valida si todavia no hay ninguna calificacion registrada.
        {
            StackResenas.Children.Add(new Label // muestra un mensaje indicando que no hay comentarios aun.
            {
                Text = "Aún no hay calificaciones. ¡Sé el primero en calificar!",
                TextColor = Color.FromArgb("#5A6B5C"),
                FontAttributes = FontAttributes.Italic // texto en cursiva
            });
            return;
        }

        foreach (var cal in calificaciones) // recorre cada calificacion dejada por los usuarios.
        {
            var card = new Border // crea una tarjeta visual para cada calificacion.
            {
                BackgroundColor = Colors.White, // fondo blanco
                Stroke = Color.FromArgb("#D8D3C4"), // borde gris
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(12, 8),
                Content = new VerticalStackLayout // contenedor vertical para apilar las estrellas y el comentario.
                {
                    Spacing = 3,
                    Children =
                    {
                        new Label // muestra las estrellas y el autor con la fecha.
                        {
                            Text = $"{cal.EstrellasTexto}  —  {cal.AutorYFechaTexto}",
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#C8785A"),
                            FontSize = 13
                        },
                        new Label // muestra el texto del comentario dejado por el usuario.
                        {
                            Text = cal.Comentario(),
                            TextColor = Color.FromArgb("#283B2D"),
                            FontSize = 13
                        }
                    }
                }
            };
            StackResenas.Children.Add(card); // agrega la tarjeta de calificacion a la lista.
        }
    }


    // Metodo que se ejecuta cuando el usuario hace clic en el boton de marcar/desmarcar favorita.
    private void OnAlternarFavoritaClicked(object? sender, EventArgs e)
    {
        _service.AlternarFavorita(_receta.ObtenerIdentificador()); // llama al servicio para agregar o quitar la receta de favoritas.
        ActualizarBotonFavorita(); // refresca el diseño del boton de favoritos.
    }


    // async metodo que se ejecuta al presionar "Enviar Calificación" para guardar una nueva calificacion en la receta publica.
    private async void OnEnviarCalificacionClicked(object? sender, EventArgs e)
    {
        if (_receta is not RecetaPublica publica) // si la receta no es publica no se puede calificar.
            return;

        // switch expression para convertir el indice seleccionado del picker a un valor numerico de 1 a 5.
        int puntaje = PickerPuntaje.SelectedIndex switch
        {
            0 => 5, // 5 estrellas
            1 => 4, // 4 estrellas
            2 => 3, // 3 estrellas
            3 => 2, // 2 estrellas
            4 => 1, // 1 estrella
            _ => 5  // valor por defecto si no coincide
        };

        // string.IsNullOrWhiteSpace valida si el texto del comentario esta vacio o contiene solo espacios.
        string comentario = string.IsNullOrWhiteSpace(EntryComentario.Text)
            ? "¡Deliciosa receta de temporada!" // comentario predeterminado si el usuario no escribio nada.
            : EntryComentario.Text.Trim(); // Trim() elimina espacios en blanco al inicio y al final.

        // Crea una nueva instancia del modelo Calificacion con el autor activo, puntaje y comentario.
        var nuevaCalificacion = new Calificacion(
            idUsuario: _service.UsuarioActivo.NombreUsuario(),
            puntaje: puntaje,
            comentario: comentario);

        _service.CalificarRecetaPublica(publica.ObtenerIdentificador(), nuevaCalificacion); // envia la calificacion al servicio de recetas.
        EntryComentario.Text = string.Empty; // limpia el campo de texto del comentario.
        LblResumenEntidad.Text = _receta.ObtenerResumenEntidad(); // actualiza el resumen en pantalla con el nuevo promedio.
        RenderizarCalificaciones(); // vuelve a dibujar las reseñas para mostrar la nueva calificacion agregada.

        await DisplayAlertAsync("Calificación Registrada (UC09)", // muestra alerta de confirmacion con el nuevo promedio.
            $"Nuevo promedio de la receta: ★ {publica.ObtenerPromedioCalificaciones():0.0}",
            "Aceptar");
    }


    // async metodo que se ejecuta al presionar "Cerrar" para cerrar la pagina modal de detalle.
    private async void OnCerrarClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // PopModalAsync() cierra y retira la ventana modal actual volviendo a la pantalla anterior.
    }
}
