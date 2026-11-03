using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;

public partial class MisRecetasPage : ContentPage
{
    private readonly RecetaService _service;

    public MisRecetasPage(RecetaService service)
    {
        InitializeComponent();
        _service = service;
        _service.DatosActualizados += RefrescarLista;
        RefrescarLista();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefrescarLista();
    }

    private void OnBusquedaPrivadaChanged(object? sender, TextChangedEventArgs e)
    {
        RefrescarLista();
    }

    private void RefrescarLista()
    {
        LblRutaJsonLocal.Text = $"📁 Archivo JSON Local: {_service.ObtenerRutaJsonPrivado()}";

        var filtro = new FiltroReceta(nombreIngrediente: EntryBuscarPrivada.Text ?? string.Empty);
        var privadas = _service.FiltrarRecetasPrivadas(_service.UsuarioActivo.ObtenerIdentificador(), filtro);

        LblContadorPrivadas.Text = $"Total en recetario privado: {privadas.Count} recetas  •  Actor activo: {_service.UsuarioActivo.ObtenerTipoPerfil()}";

        StackRecetasPrivadas.Children.Clear();

        if (privadas.Count == 0)
        {
            StackRecetasPrivadas.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#FFFFFF"),
                Stroke = Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Padding = 22,
                Content = new Label
                {
                    Text = "No hay recetas privadas que coincidan. Usa el botón '➕ Nueva Receta' o la pestaña Crear/Editar para agregar una.",
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromArgb("#283B2D")
                }
            });
            return;
        }

        foreach (var receta in privadas)
        {
            var btnVer = new Button
            {
                Text = "📖 Ver Detalle",
                BackgroundColor = Color.FromArgb("#C8785A"),
                TextColor = Color.FromArgb("#FFFFFF"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                FontSize = 12
            };
            btnVer.Clicked += async (_, _) =>
            {
                await Navigation.PushModalAsync(new DetalleRecetaPage(receta, _service));
            };

            var btnEditar = new Button
            {
                Text = "✏ Editar Receta (UC02)",
                BackgroundColor = Color.FromArgb("#758B68"),
                TextColor = Color.FromArgb("#FFFFFF"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                FontSize = 12
            };
            btnEditar.Clicked += (_, _) =>
            {
                _service.DispararEdicionReceta(receta);
            };

            var btnPublicarChef = new Button
            {
                Text = "🌐 Publicar en Feed Global (Chef UC16)",
                BackgroundColor = Color.FromArgb("#E8EDE4"),
                TextColor = Color.FromArgb("#283B2D"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                FontSize = 12
            };
            btnPublicarChef.Clicked += async (_, _) =>
            {
                var chef = _service.UsuarioActivo as CreadorEspecialista ?? _service.PerfilCreadorEspecialista;
                var recetaPublica = chef.PublicarRecetaOficial(receta);
                _service.PublicarRecetaEnFeed(recetaPublica, chef);

                await DisplayAlertAsync("Receta Publicada en Feed Global (UC16)",
                    $"'{recetaPublica.Titulo}' fue certificada por {chef.NombreUsuario()} y serializada en el archivo JSON Global.",
                    "Ver en Feed");

                _service.NavegarAPestana("Feed");
            };

            var btnEliminar = new Button
            {
                Text = "🗑 Eliminar (UC02)",
                BackgroundColor = Color.FromArgb("#F5E5DF"),
                TextColor = Color.FromArgb("#A85E42"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                FontSize = 12
            };
            btnEliminar.Clicked += async (_, _) =>
            {
                bool confirmar = await DisplayAlertAsync(
                    "Eliminar Receta Privada (UC02)",
                    $"¿Deseas eliminar '{receta.Titulo}' de tu recetario local?",
                    "Eliminar", "Cancelar");
                if (confirmar)
                {
                    _service.EliminarRecetaPrivada(receta.ObtenerIdentificador());
                }
            };

            var card = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 },
                Padding = 16,
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new HorizontalStackLayout
                        {
                            Spacing = 14,
                            Children =
                            {
                                new Border
                                {
                                    BackgroundColor = Color.FromArgb("#F7F4EA"),
                                    Stroke = Color.FromArgb("#758B68"),
                                    StrokeThickness = 1.5,
                                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                                    WidthRequest = 96,
                                    HeightRequest = 96,
                                    Padding = 0,
                                    VerticalOptions = LayoutOptions.Center,
                                    Content = new Image
                                    {
                                        Source = receta.ImagenPlatillo,
                                        Aspect = Aspect.AspectFill,
                                        WidthRequest = 96,
                                        HeightRequest = 96
                                    }
                                },
                                new VerticalStackLayout
                                {
                                    VerticalOptions = LayoutOptions.Center,
                                    Children =
                                    {
                                        new Label
                                        {
                                            Text = receta.Titulo,
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
                        new Label
                        {
                            Text = $"📝 Notas personales: {(string.IsNullOrWhiteSpace(receta.NotasPersonales()) ? "Sin notas adicionales" : receta.NotasPersonales())}",
                            FontSize = 13,
                            TextColor = Color.FromArgb("#283B2D"),
                            BackgroundColor = Color.FromArgb("#F7F4EA"),
                            Padding = new Thickness(10, 6)
                        },
                        new Label
                        {
                            Text = receta.ObtenerResumenEntidad(),
                            FontSize = 11,
                            TextColor = Color.FromArgb("#5A6B5C")
                        },
                        new FlexLayout
                        {
                            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
                            JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start,
                            Children = { btnVer, btnEditar, btnPublicarChef, btnEliminar }
                        }
                    }
                }
            };

            StackRecetasPrivadas.Children.Add(card);
        }
    }

    private void OnIrACrearClicked(object? sender, EventArgs e)
    {
        _service.NavegarAPestana("CrearEditar");
    }

    private async void OnGuardarJsonLocalClicked(object? sender, EventArgs e)
    {
        _service.GuardarRespaldoPrivadoManual();
        await DisplayAlertAsync("Respaldo Local Guardado (UC14)",
            $"Recetario privado serializado exitosamente en:\n{_service.ObtenerRutaJsonPrivado()}",
            "Aceptar");
    }

    private async void OnCargarJsonLocalClicked(object? sender, EventArgs e)
    {
        _service.CargarRespaldoPrivadoManual();
        await DisplayAlertAsync("Respaldo Local Cargado (UC15)",
            $"Recetario privado deserializado desde:\n{_service.ObtenerRutaJsonPrivado()}",
            "Aceptar");
    }
}
