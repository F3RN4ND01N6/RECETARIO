using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;

public partial class TemporadaPage : ContentPage
{
    private readonly RecetaService _service;
    private Estacion _estacionActual = Estacion.TODOELANHIO;

    public TemporadaPage(RecetaService service)
    {
        InitializeComponent();
        _service = service;
        CargarEstacion(Estacion.TODOELANHIO);
    }

    private void CargarEstacion(Estacion estacion)
    {
        _estacionActual = estacion;
        ActualizarBotonesEstacion(estacion);
        var lista = _service.ConsultarCalendarioTemporada(estacion);

        LblEstacionSeleccionada.Text =
            $"Estación seleccionada: {estacion} ({lista.Count} frutas y verduras registradas)";

        StackIngredientesTemporada.Children.Clear();

        foreach (var item in lista)
        {
            bool esFruta = item.Tipo() == TipoVegetal.FRUTA;

            var badgeTipo = new Border
            {
                BackgroundColor = esFruta ? Color.FromArgb("#F5E5DF") : Color.FromArgb("#E8EDE4"),
                Stroke = esFruta ? Color.FromArgb("#C8785A") : Color.FromArgb("#758B68"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(10, 4),
                VerticalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = $"{item.TipoTexto}  •  {item.EstacionPrincipal()}",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#283B2D")
                }
            };

            var fila = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 14
            };

            fila.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#E8EDE4"),
                Stroke = Color.FromArgb("#758B68"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                WidthRequest = 56,
                HeightRequest = 56,
                Content = new Label
                {
                    Text = item.IconoVegetal,
                    FontSize = 28,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            }, 0, 0);

            fila.Add(new VerticalStackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = item.NombreFrutaVerdura(),
                        FontSize = 17,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#283B2D")
                    },
                    new Label
                    {
                        Text = $"📍 País de origen: {item.PaisOrigen()}  •  Zona de cultivo: {item.ZonaCultivo()}",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#5A6B5C")
                    }
                }
            }, 1, 0);

            fila.Add(badgeTipo, 2, 0);

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

    private void ActualizarBotonesEstacion(Estacion activa)
    {
        PintarBotonEstacion(BtnEstacionTodo, activa == Estacion.TODOELANHIO);
        PintarBotonEstacion(BtnEstacionPrimavera, activa == Estacion.PRIMAVERA);
        PintarBotonEstacion(BtnEstacionVerano, activa == Estacion.VERANO);
        PintarBotonEstacion(BtnEstacionOtonio, activa == Estacion.OTONIO);
        PintarBotonEstacion(BtnEstacionInvierno, activa == Estacion.INVIERNO);
    }

    private static void PintarBotonEstacion(Button boton, bool activo)
    {
        if (boton == null) return;
        boton.BackgroundColor = activo ? Color.FromArgb("#758B68") : Color.FromArgb("#ECE8DC");
        boton.TextColor = activo ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#283B2D");
    }

    private void OnTodoElAnioClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.TODOELANHIO);
    private void OnPrimaveraClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.PRIMAVERA);
    private void OnVeranoClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.VERANO);
    private void OnOtonioClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.OTONIO);
    private void OnInviernoClicked(object? sender, EventArgs e) => CargarEstacion(Estacion.INVIERNO);

    private void OnFiltrarFeedPorEstacionActualClicked(object? sender, EventArgs e)
    {
        _service.DispararFiltroEstacionEnFeed(_estacionActual);
    }
}
