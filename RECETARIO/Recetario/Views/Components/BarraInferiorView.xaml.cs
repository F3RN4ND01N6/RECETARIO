namespace Recetario.Views.Components;

public partial class BarraInferiorView : ContentView
{
    public static readonly BindableProperty PestanaActivaProperty =
        BindableProperty.Create(
            nameof(PestanaActiva),
            typeof(string),
            typeof(BarraInferiorView),
            "Feed",
            propertyChanged: OnPestanaActivaChanged);

    public string PestanaActiva
    {
        get => (string)GetValue(PestanaActivaProperty);
        set => SetValue(PestanaActivaProperty, value);
    }

    public BarraInferiorView()
    {
        InitializeComponent();
        ActualizarEstilos();
    }

    private static void OnPestanaActivaChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BarraInferiorView barra)
        {
            barra.ActualizarEstilos();
        }
    }

    private void ActualizarEstilos()
    {
        AplicarEstiloBoton(BtnTabFeed, PestanaActiva == "Feed", "#758B68");
        AplicarEstiloBoton(BtnTabMisRecetas, PestanaActiva == "MisRecetas", "#758B68");
        AplicarEstiloBoton(BtnTabCrearEditar, PestanaActiva == "CrearEditar", "#C8785A");
        AplicarEstiloBoton(BtnTabTemporada, PestanaActiva == "Temporada", "#758B68");
        AplicarEstiloBoton(BtnTabPerfil, PestanaActiva == "Perfil", "#C8785A");
    }

    private static void AplicarEstiloBoton(Button boton, bool activo, string colorActivoHex)
    {
        if (boton == null) return;

        if (activo)
        {
            boton.BackgroundColor = Color.FromArgb(colorActivoHex);
            boton.TextColor = Color.FromArgb("#FFFFFF");
            boton.BorderColor = Color.FromArgb("#283B2D");
            boton.BorderWidth = 1.5;
        }
        else
        {
            boton.BackgroundColor = Color.FromArgb("#F7F4EA");
            boton.TextColor = Color.FromArgb("#283B2D");
            boton.BorderColor = Color.FromArgb("#D8D3C4");
            boton.BorderWidth = 1;
        }
    }

    private async void OnFeedClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//FeedPage");

    private async void OnMisRecetasClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MisRecetasPage");

    private async void OnCrearEditarClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//CrearEditarRecetaPage");

    private async void OnTemporadaClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//TemporadaPage");

    private async void OnPerfilClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//PerfilPage");
}
