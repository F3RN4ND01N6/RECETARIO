using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;

public partial class FeedPage : ContentPage
{
    private readonly RecetaService _service;
    private bool _inicializando = true;

    public FeedPage(RecetaService service)
    {
        InitializeComponent();
        _service = service;

        ConfigurarSelectores();
        _service.DatosActualizados += RefrescarVista;
        _service.SolicitudFiltrarFeedPorEstacion += AplicarFiltroDesdeCalendario;

        _inicializando = false;
        RefrescarVista();
    }

    /// <summary>
    /// LLAMADO DE ENUMERADORES 
    /// </summary>
    private void ConfigurarSelectores()
    {
        PickerVegetal.ItemsSource = new List<string> { "TODOS", "FRUTA", "VERDURA", "OTRO" };
        PickerVegetal.SelectedIndex = 0;

        PickerEstacion.ItemsSource = new List<string> { "TODAS", "PRIMAVERA", "VERANO", "OTOÑO", "INVIERNO", "TODO EL AÑO" };
        PickerEstacion.SelectedIndex = 0;

        PickerOrigen.ItemsSource = new List<string> { "TODOS", "CHILE", "ARGENTINA", "PERU", "COLOMBIA", "OTRO" };
        PickerOrigen.SelectedIndex = 0;

        PickerDificultad.ItemsSource = new List<string> { "TODAS", "FACIL", "MEDIA", "DIFICIL" };
        PickerDificultad.SelectedIndex = 0;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefrescarVista();
    }

    private void AplicarFiltroDesdeCalendario(Estacion estacion)
    {
        _inicializando = true;
        PickerEstacion.SelectedItem = estacion.ToString();
        _inicializando = false;
        RefrescarVista();
    }

    private void OnFiltroCambiado(object? sender, EventArgs e)
    {
        if (_inicializando)
            return;
        RefrescarVista();
    }

    private void RefrescarVista()
    {
        if (_inicializando)
            return;

        LblRolActivo.Text = $"Actor: {_service.UsuarioActivo.ObtenerTipoPerfil()}";

        TipoVegetal? tipoVeg = PickerVegetal.SelectedIndex > 0
            ? Enum.Parse<TipoVegetal>(PickerVegetal.SelectedItem.ToString()!)
            : null;

        Estacion? estacion = PickerEstacion.SelectedIndex > 0
            ? Enum.Parse<Estacion>(PickerEstacion.SelectedItem.ToString()!)
            : null;

        Origen? origen = PickerOrigen.SelectedIndex > 0
            ? Enum.Parse<Origen>(PickerOrigen.SelectedItem.ToString()!)
            : null;

        NivelDificultad? dificultad = PickerDificultad.SelectedIndex > 0
            ? Enum.Parse<NivelDificultad>(PickerDificultad.SelectedItem.ToString()!)
            : null;

        var filtro = new FiltroReceta(
            tipoFrutaOVerdura: tipoVeg,
            nombreIngrediente: EntryBusqueda.Text ?? string.Empty,
            estacionTemporada: estacion,
            pais: origen,
            categoriaFiltro: string.Empty,
            dietaFiltro: string.Empty,
            dificultad: dificultad,
            soloVerificadasPorChef: ChkSoloChefs.IsChecked);

        var resultados = _service.FiltrarFeedGlobal(filtro);

        if (ChkSoloFavoritas.IsChecked)
        {
            resultados = resultados.Where(r => _service.EsRecetaFavorita(r.ObtenerIdentificador())).ToList();
        }

        LblEstadisticasFeed.Text =
            $"Mostrando {resultados.Count} de {_service.TotalRecetasPublicas} recetas públicas  •  ♥ {_service.TotalFavoritas} favoritas  •  Rol activo: {_service.UsuarioActivo.NombreUsuario()}";

        RenderizarTarjetas(resultados);
    }

    private void RenderizarTarjetas(List<RecetaPublica> recetas)
    {
        StackTarjetasFeed.Children.Clear();

        if (recetas.Count == 0)
        {
            StackTarjetasFeed.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#FFFFFF"),
                Stroke = Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Padding = 24,
                Content = new Label
                {
                    Text = "No se encontraron recetas con los filtros seleccionados. Prueba limpiando los filtros.",
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromArgb("#283B2D"),
                    FontSize = 15
                }
            });
            return;
        }

        bool esAdmin = _service.UsuarioActivo is Administrador;

        foreach (var receta in recetas)
        {
            bool esFav = _service.EsRecetaFavorita(receta.ObtenerIdentificador());

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
                Text = esFav ? "♥ Favorita" : "♡ Favorita",
                BackgroundColor = esFav ? Color.FromArgb("#C8785A") : Color.FromArgb("#F5E5DF"),
                TextColor = esFav ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#C8785A"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 14,
                Padding = new Thickness(14, 6),
                FontSize = 12
            };
            btnFav.Clicked += (_, _) =>
            {
                _service.AlternarFavorita(receta.ObtenerIdentificador());
            };

            // Botón Ver Receta en Terracota (#C8785A) o Verde Salvia (#758B68) con texto blanco
            var btnDetalle = new Button
            {
                Text = "📖 Ver Receta y Calificar (UC09)",
                BackgroundColor = Color.FromArgb("#C8785A"),
                TextColor = Color.FromArgb("#FFFFFF"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 14,
                Padding = new Thickness(16, 6),
                FontSize = 12
            };
            btnDetalle.Clicked += async (_, _) =>
            {
                await Navigation.PushModalAsync(new DetalleRecetaPage(receta, _service));
            };

            var barraBotones = new HorizontalStackLayout
            {
                Spacing = 8,
                Margin = new Thickness(0, 6, 0, 0),
                Children = { btnDetalle, btnFav }
            };

            if (esAdmin)
            {
                var btnAdminEliminar = new Button
                {
                    Text = "🗑 Moderar / Eliminar (Admin UC02)",
                    BackgroundColor = Color.FromArgb("#283B2D"),
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 14,
                    Padding = new Thickness(12, 6),
                    FontSize = 12
                };
                btnAdminEliminar.Clicked += async (_, _) =>
                {
                    bool confirmar = await DisplayAlertAsync(
                        "Moderación de Administrador (UC02)",
                        $"¿Forzar eliminación de '{receta.Titulo}' del Feed Público Global?",
                        "Eliminar", "Cancelar");
                    if (confirmar)
                    {
                        _service.EliminarRecetaPublicaComoAdmin(receta.ObtenerIdentificador());
                    }
                };
                barraBotones.Children.Add(btnAdminEliminar);
            }

            var gridContenido = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 16
            };

            var cuadroIcono = new Border
            {
                BackgroundColor = Color.FromArgb("#F7F4EA"),
                Stroke = Color.FromArgb("#758B68"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                WidthRequest = 150,
                HeightRequest = 150,
                Padding = 0,
                VerticalOptions = LayoutOptions.Center,
                Content = new Image
                {
                    Source = receta.ImagenPlatillo,
                    Aspect = Aspect.AspectFill,
                    WidthRequest = 150,
                    HeightRequest = 150
                }
            };

            var stackInfo = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    badgeVerificacion,
                    new Label
                    {
                        Text = receta.Titulo,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#283B2D")
                    },
                    new Label
                    {
                        Text = $"{receta.CategoriaYDietaTexto}  |  {receta.UbicacionEstacionTexto}",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#5A6B5C")
                    },
                    new Label
                    {
                        Text = $"{receta.MetaResumenTexto}  |  {receta.EtiquetasTexto}",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#758B68")
                    },
                    barraBotones
                }
            };

            gridContenido.Add(cuadroIcono, 0, 0);
            gridContenido.Add(stackInfo, 1, 0);

            var tarjeta = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = receta.DestacadaEnFeed() ? Color.FromArgb("#758B68") : Color.FromArgb("#D8D3C4"),
                StrokeThickness = receta.DestacadaEnFeed() ? 2 : 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 },
                Padding = 16,
                Content = gridContenido
            };

            StackTarjetasFeed.Children.Add(tarjeta);
        }
    }

    private async void OnSobrecargaEstacionPaisClicked(object? sender, EventArgs e)
    {
        Estacion est = PickerEstacion.SelectedIndex > 0
            ? Enum.Parse<Estacion>(PickerEstacion.SelectedItem.ToString()!)
            : _service.UsuarioActivo.ConfiguracionPerfil().EstacionPreferida();

        Origen pais = PickerOrigen.SelectedIndex > 0
            ? Enum.Parse<Origen>(PickerOrigen.SelectedItem.ToString()!)
            : _service.UsuarioActivo.ConfiguracionPerfil().Origen();

        // Invoca la segunda sobrecarga exacta del Diagrama de Clases: FiltrarFeedGlobal(Estacion, Origen)
        var filtradas = _service.FiltrarFeedGlobal(est, pais);
        RenderizarTarjetas(filtradas);

        LblEstadisticasFeed.Text =
            $"Sobrecarga FiltrarFeedGlobal({est}, {pais}) ejecutada: {filtradas.Count} recetas encontradas.";

        await DisplayAlertAsync("Sobrecarga de Método Ejecutada",
            $"Se invocó RecetaService.FiltrarFeedGlobal(Estacion.{est}, Origen.{pais}).\nResultados encontrados: {filtradas.Count}",
            "Aceptar");
    }

    private void OnLimpiarFiltrosClicked(object? sender, EventArgs e)
    {
        _inicializando = true;
        EntryBusqueda.Text = string.Empty;
        PickerVegetal.SelectedIndex = 0;
        PickerEstacion.SelectedIndex = 0;
        PickerOrigen.SelectedIndex = 0;
        PickerDificultad.SelectedIndex = 0;
        ChkSoloChefs.IsChecked = false;
        ChkSoloFavoritas.IsChecked = false;
        _inicializando = false;

        RefrescarVista();
    }

    private async void OnRecargarJsonGlobalClicked(object? sender, EventArgs e)
    {
        _service.CargarRespaldoGlobalManual();
        await DisplayAlertAsync("Respaldo Global JSON (UC17)",
            $"Catálogo global deserializado correctamente desde:\n{_service.ObtenerRutaJsonPublico()}",
            "Aceptar");
    }
}
