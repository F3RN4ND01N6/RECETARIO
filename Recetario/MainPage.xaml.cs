namespace Recetario;


// Partial significa que la clase MainPage está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
// MainPage es la página predeterminada generada por la plantilla de .NET MAUI con un contador de ejemplo.
public partial class MainPage : ContentPage
{

    // Variable entera para almacenar el numero de veces que se ha hecho clic en el botón.
    int count = 0;


    // Constructor de la página MainPage, inicializa los componentes definidos en el archivo XAML.
    public MainPage()
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los componentes XAML asociados a esta clase.
    }


    // Metodo privado que se ejecuta cuando el usuario hace clic en el boton CounterBtn.
    private void OnCounterClicked(object? sender, EventArgs e)
    {
        count++; // incrementa en uno el valor del contador

        // Valida si el contador es 1 para mostrar el texto en singular o en plural
        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        // SemanticScreenReader permite anunciar el texto a herramientas de accesibilidad y lectores de pantalla.
        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
