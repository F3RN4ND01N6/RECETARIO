using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;

public partial class DetalleRecetaPage : ContentPage
{
    private readonly Receta _receta;
    private readonly RecetaService _service;

    public DetalleRecetaPage(Receta receta, RecetaService service)
    {
        InitializeComponent();
        _receta = receta;
        _service = service;

        PickerPuntaje.ItemsSource = new List<string>
        {
            "5 ★ - Excelente",
            "4 ★ - Muy Buena",
            "3 ★ - Buena",
            "2 ★ - Regular",
            "1 ★ - Mejorable"
        };
        PickerPuntaje.SelectedIndex = 0;

        CargarVista();
    }

    private void CargarVista()
    {
        ImgPlatillo.Source = _receta.ImagenPlatillo;
        LblTitulo.Text = _receta.Titulo;
        LblSubtitulo.Text = $"{_receta.CategoriaYDietaTexto}  •  {_receta.UbicacionEstacionTexto}";
        LblMeta.Text = $"{_receta.MetaResumenTexto}  •  {_receta.EtiquetasTexto}";
        LblResumenEntidad.Text = _receta.ObtenerResumenEntidad();

        ActualizarBotonFavorita();
        RenderizarIngredientes();
        RenderizarPasos();
        RenderizarCalificaciones();
    }

    private void ActualizarBotonFavorita()
    {
        bool esFav = _service.EsRecetaFavorita(_receta.ObtenerIdentificador());
        BtnFavorita.Text = esFav ? "♥ En tus Favoritas (Quitar UC08)" : "♡ Marcar como Favorita (UC08)";
        BtnFavorita.BackgroundColor = esFav ? Color.FromArgb("#C8785A") : Color.FromArgb("#F5E5DF");
        BtnFavorita.TextColor = esFav ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#C8785A");
    }

    private void RenderizarIngredientes()
    {
        StackIngredientes.Children.Clear();
        foreach (var ing in _receta.Ingredientes)
        {
            var fila = new Border
            {
                BackgroundColor = Color.FromArgb("#F7F4EA"),
                Stroke = Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(12, 8),
                Content = new Label
                {
                    Text = $"• {ing.FormatearIngrediente()}",
                    TextColor = Color.FromArgb("#283B2D"),
                    FontSize = 14
                }
            };
            StackIngredientes.Children.Add(fila);
        }
    }

    private void RenderizarPasos()
    {
        StackPasos.Children.Clear();
        foreach (var paso in _receta.Pasos)
        {
            var btnPaso = new Button
            {
                Text = $"{(paso.Completado() ? "✅" : "⬜")} {paso.FormatearPaso()}",
                BackgroundColor = paso.Completado() ? Color.FromArgb("#758B68") : Color.FromArgb("#F7F4EA"),
                TextColor = paso.Completado() ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#283B2D"),
                CornerRadius = 10,
                HorizontalOptions = LayoutOptions.Fill
            };

            btnPaso.Clicked += (_, _) =>
            {
                paso.MarcarCompletado();
                RenderizarPasos();
            };

            StackPasos.Children.Add(btnPaso);
        }
    }

    private void RenderizarCalificaciones()
    {
        if (_receta is not RecetaPublica publica)
        {
            PanelCalificaciones.IsVisible = false;
            return;
        }

        PanelCalificaciones.IsVisible = true;
        StackResenas.Children.Clear();

        var calificaciones = publica.ObtenerCalificaciones();
        if (calificaciones.Count == 0)
        {
            StackResenas.Children.Add(new Label
            {
                Text = "Aún no hay calificaciones. ¡Sé el primero en calificar!",
                TextColor = Color.FromArgb("#5A6B5C"),
                FontAttributes = FontAttributes.Italic
            });
            return;
        }

        foreach (var cal in calificaciones)
        {
            var card = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(12, 8),
                Content = new VerticalStackLayout
                {
                    Spacing = 3,
                    Children =
                    {
                        new Label
                        {
                            Text = $"{cal.EstrellasTexto}  —  {cal.AutorYFechaTexto}",
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#C8785A"),
                            FontSize = 13
                        },
                        new Label
                        {
                            Text = cal.Comentario(),
                            TextColor = Color.FromArgb("#283B2D"),
                            FontSize = 13
                        }
                    }
                }
            };
            StackResenas.Children.Add(card);
        }
    }

    private void OnAlternarFavoritaClicked(object? sender, EventArgs e)
    {
        _service.AlternarFavorita(_receta.ObtenerIdentificador());
        ActualizarBotonFavorita();
    }

    private async void OnEnviarCalificacionClicked(object? sender, EventArgs e)
    {
        if (_receta is not RecetaPublica publica)
            return;

        int puntaje = PickerPuntaje.SelectedIndex switch
        {
            0 => 5,
            1 => 4,
            2 => 3,
            3 => 2,
            4 => 1,
            _ => 5
        };

        string comentario = string.IsNullOrWhiteSpace(EntryComentario.Text)
            ? "¡Deliciosa receta de temporada!"
            : EntryComentario.Text.Trim();

        var nuevaCalificacion = new Calificacion(
            idUsuario: _service.UsuarioActivo.NombreUsuario(),
            puntaje: puntaje,
            comentario: comentario);

        _service.CalificarRecetaPublica(publica.ObtenerIdentificador(), nuevaCalificacion);
        EntryComentario.Text = string.Empty;
        LblResumenEntidad.Text = _receta.ObtenerResumenEntidad();
        RenderizarCalificaciones();

        await DisplayAlertAsync("Calificación Registrada (UC09)",
            $"Nuevo promedio de la receta: ★ {publica.ObtenerPromedioCalificaciones():0.0}",
            "Aceptar");
    }

    private async void OnCerrarClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
