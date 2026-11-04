using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;

public partial class CrearEditarRecetaPage : ContentPage
{

    private readonly RecetaService _service;
    private string? _idRecetaEnEdicion;
    private string _rutaImagenPersonalizada = string.Empty;

    private readonly List<IngredienteReceta> _ingredientesActuales = new();
    private readonly List<PasoPreparacion> _pasosActuales = new();


    public CrearEditarRecetaPage(RecetaService service)
    {
        InitializeComponent();
        _service = service;

        //   1. CARGA LAS OPCIONES DE TODOS LOS SELECTORES (PICKERS)
        InicializarSelectores();

        //   2. SUSCRIBE EL EVENTO PARA RECIBIR UNA RECETA CUANDO SE PULSA "EDITAR" (UC02)
        _service.SolicitudEditarReceta += CargarRecetaParaEdicion;

        //   3. PREPARA LOS VALORES INICIALES DEL FORMULARIO
        CargarDatosPorDefecto();
    }


    private void InicializarSelectores()
    {
        //   SELECTOR DE FOTOGRAFIAS PRECARGADAS EN RESOURCES/IMAGES
        PickerIcono.ItemsSource = new List<string>
        {
            "platillo_default.jpg",
            "paste_choclo.png",
            "ceviche_mango.jpg",
            "pastelera_choclo.jpg",
            "crema_zapallo.jpg",
            "salteado_brocoli.jpg",
            "tartaleta_frutilla.jpg",
            "tostadas_durazno.jpg"
        };
        PickerIcono.SelectedIndex = 0;
        ImgVistaPrevia.Source = "platillo_default.jpg";



        //   SELECTOR DE CATEGORIA GASTRONOMICA (UC03)
        PickerCategoria.ItemsSource = new List<string>
        {
            "Plato Principal",
            "Sopas y Cremas",
            "Ensalada Tibia",
            "Entrada Fresca",
            "Desayuno / Brunch",
            "Postre de Estación"
        };
        PickerCategoria.SelectedIndex = 0;


        //   SELECTOR DE TIPO DE DIETA (UC03)
        PickerDieta.ItemsSource = new List<string>
        {
            "Vegetariana",
            "Vegana",
            "Omnívora",
            "Sin Gluten",
            "Keto / Baja en Carbohidratos"
        };
        PickerDieta.SelectedIndex = 0;


        //   SELECTORES CONECTADOS DIRECTAMENTE A LOS ENUMERADORES DEL DIAGRAMA DE CLASES
        PickerDificultad.ItemsSource = Enum.GetNames<NivelDificultad>().ToList();
        PickerDificultad.SelectedIndex = 0;

        PickerEstacion.ItemsSource = Enum.GetNames<Estacion>().ToList();
        PickerEstacion.SelectedIndex = 1;

        PickerOrigen.ItemsSource = Enum.GetNames<Origen>().ToList();
        PickerOrigen.SelectedIndex = 0;

        PickerUnidadIngrediente.ItemsSource = Enum.GetNames<UnidadMedida>().ToList();
        PickerUnidadIngrediente.SelectedIndex = 6; // UNIDADES

        PickerTipoVegetalIng.ItemsSource = Enum.GetNames<TipoVegetal>().ToList();
        PickerTipoVegetalIng.SelectedIndex = 1; // VERDURA
    }


    private void CargarDatosPorDefecto()
    {
        _idRecetaEnEdicion = null;
        _rutaImagenPersonalizada = string.Empty;

        PickerIcono.SelectedIndex = 0;
        ImgVistaPrevia.Source = "platillo_default.jpg";

        LblTituloModo.Text = "Crear Nueva Receta";
        LblSubtituloModo.Text = "Completa los datos generales, ingredientes y pasos. Todos los campos son verificados.";
        BtnGuardarPrivada.Text = "Guardar en Mi Recetario";

        EntryTitulo.Text = string.Empty;
        EntryEtiquetas.Text = "Temporada, Casera";
        EntryTiempo.Text = "30";
        EntryPorciones.Text = "4";
        EntryRegion.Text = "Valle Central";
        EntryNotasPersonales.Text = string.Empty;

        _ingredientesActuales.Clear();
        _ingredientesActuales.Add(new IngredienteReceta("Zapallo Camote en cubos", 500, UnidadMedida.GRAMO, Estacion.OTONIO, true, TipoVegetal.VERDURA));

        _pasosActuales.Clear();
        _pasosActuales.Add(new PasoPreparacion(1, "Lavar, pelar y cortar los vegetales de estación en trozos regulares.", 10));

        RefrescarListasFormulario();
    }


    private void CargarRecetaParaEdicion(RecetaPrivada receta)
    {
        _idRecetaEnEdicion = receta.ObtenerIdentificador();

        LblTituloModo.Text = $"Editando Receta '{receta.Titulo}' (ID: {_idRecetaEnEdicion} ";
        LblSubtituloModo.Text = "Modifica cualquier campo, ingrediente o paso y presiona Actualizar.";
        BtnGuardarPrivada.Text = "Actualizar Receta Editada";

        EntryTitulo.Text = receta.Titulo;
        _rutaImagenPersonalizada = receta.ImagenPlatillo;

        if (PickerIcono.ItemsSource is IList<string> items && items.Contains(receta.ImagenPlatillo))
        {
            PickerIcono.SelectedItem = receta.ImagenPlatillo;
            _rutaImagenPersonalizada = string.Empty;
        }

        ImgVistaPrevia.Source = receta.ImagenPlatillo;
        PickerCategoria.SelectedItem = receta.Categoria;
        PickerDieta.SelectedItem = receta.TipoDieta;
        EntryEtiquetas.Text = string.Join(", ", receta.Etiquetas);
        EntryTiempo.Text = receta.TiempoPreparacion.ToString();
        EntryPorciones.Text = receta.Porciones.ToString();
        PickerDificultad.SelectedItem = receta.Dificultad.ToString();
        PickerEstacion.SelectedItem = receta.Estacion.ToString();
        PickerOrigen.SelectedItem = receta.Origen.ToString();
        EntryRegion.Text = receta.Region;
        EntryNotasPersonales.Text = receta.NotasPersonales();

        _ingredientesActuales.Clear();
        _ingredientesActuales.AddRange(receta.Ingredientes);

        _pasosActuales.Clear();
        _pasosActuales.AddRange(receta.Pasos);

        RefrescarListasFormulario();
    }


    private void RefrescarListasFormulario()
    {
        //   1. RECONSTRUYE LA LISTA VISUAL DE INGREDIENTES (COMPOSICION 1..*)
        StackListaIngredientes.Children.Clear();

        for (int i = 0; i < _ingredientesActuales.Count; i++)
        {
            int index = i;
            var ing = _ingredientesActuales[i];

            var btnQuitar = new Button
            {
                Text = "✖",
                BackgroundColor = Color.FromArgb("#C8785A"),
                TextColor = Color.FromArgb("#FFFFFF"),
                CornerRadius = 8,
                Padding = new Thickness(10, 4),
                FontSize = 12
            };
            btnQuitar.Clicked += (_, _) =>
            {
                _ingredientesActuales.RemoveAt(index);
                RefrescarListasFormulario();
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            grid.Add(new Label
            {
                Text = $"• {ing.FormatearIngrediente()}",
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb("#283B2D")
            }, 0, 0);
            grid.Add(btnQuitar, 1, 0);

            StackListaIngredientes.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#FFFFFF"),
                Stroke = Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(10, 6),
                Content = grid
            });
        }


        //   2. RECONSTRUYE LA LISTA VISUAL DE PASOS DE PREPARACION (COMPOSICION 1..*)
        StackListaPasos.Children.Clear();

        for (int i = 0; i < _pasosActuales.Count; i++)
        {
            int index = i;
            var paso = _pasosActuales[i];
            paso.NumeroPasoBind = i + 1;

            var btnQuitarPaso = new Button
            {
                Text = "✖",
                BackgroundColor = Color.FromArgb("#C8785A"),
                TextColor = Color.FromArgb("#FFFFFF"),
                CornerRadius = 8,
                Padding = new Thickness(10, 4),
                FontSize = 12
            };
            btnQuitarPaso.Clicked += (_, _) =>
            {
                _pasosActuales.RemoveAt(index);
                RefrescarListasFormulario();
            };

            var gridPaso = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            gridPaso.Add(new Label
            {
                Text = paso.FormatearPaso(),
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb("#283B2D")
            }, 0, 0);
            gridPaso.Add(btnQuitarPaso, 1, 0);

            StackListaPasos.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#FFFFFF"),
                Stroke = Color.FromArgb("#D8D3C4"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(10, 6),
                Content = gridPaso
            });
        }
    }

    private void OnAgregarIngredienteClicked(object? sender, EventArgs e)
    {
        string nombre = EntryNombreIngrediente.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nombre))
            return;

        double.TryParse(EntryCantidadIngrediente.Text, out double cantidad);
        if (cantidad <= 0) cantidad = 1;

        var unidad = Enum.Parse<UnidadMedida>(PickerUnidadIngrediente.SelectedItem?.ToString() ?? "UNIDADES");
        var tipoVeg = Enum.Parse<TipoVegetal>(PickerTipoVegetalIng.SelectedItem?.ToString() ?? "VERDURA");
        var estacion = Enum.Parse<Estacion>(PickerEstacion.SelectedItem?.ToString() ?? "PRIMAVERA");

        _ingredientesActuales.Add(new IngredienteReceta(
            nombre: nombre,
            cantidad: cantidad,
            unidad: unidad,
            estacion: estacion,
            esFrutaOVerduraEstacion: tipoVeg is TipoVegetal.FRUTA or TipoVegetal.VERDURA,
            tipoVegetal: tipoVeg));

        EntryNombreIngrediente.Text = string.Empty;
        RefrescarListasFormulario();
    }

    private void OnAgregarPasoClicked(object? sender, EventArgs e)
    {
        string instruccion = EntryInstruccionPaso.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(instruccion))
            return;

        int.TryParse(EntryMinutosPaso.Text, out int minutos);
        if (minutos <= 0) minutos = 5;

        _pasosActuales.Add(new PasoPreparacion(_pasosActuales.Count + 1, instruccion, minutos));
        EntryInstruccionPaso.Text = string.Empty;
        RefrescarListasFormulario();
    }


    private RecetaPrivada ConstruirRecetaDesdeFormulario()
    {
        int.TryParse(EntryTiempo.Text, out int tiempo);
        int.TryParse(EntryPorciones.Text, out int porciones);

        var etiquetas = (EntryEtiquetas.Text ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        string imagenElegida = !string.IsNullOrWhiteSpace(_rutaImagenPersonalizada)
            ? _rutaImagenPersonalizada
            : (PickerIcono.SelectedItem?.ToString() ?? "platillo_default.jpg");

        return new RecetaPrivada(
            idReceta: _idRecetaEnEdicion ?? $"PRIV-{Guid.NewGuid().ToString("N")[..5].ToUpperInvariant()}",
            idUsuarioAutor: _service.UsuarioActivo.ObtenerIdentificador(),
            titulo: EntryTitulo.Text?.Trim() ?? string.Empty,
            categoria: PickerCategoria.SelectedItem?.ToString() ?? "Plato Principal",
            tipoDieta: PickerDieta.SelectedItem?.ToString() ?? "Vegetariana",
            etiquetas: etiquetas,
            imagenPlatillo: imagenElegida,
            tiempoPreparacion: tiempo,
            porciones: porciones,
            dificultad: Enum.Parse<NivelDificultad>(PickerDificultad.SelectedItem?.ToString() ?? "FACIL"),
            estacion: Enum.Parse<Estacion>(PickerEstacion.SelectedItem?.ToString() ?? "PRIMAVERA"),
            origen: Enum.Parse<Origen>(PickerOrigen.SelectedItem?.ToString() ?? "CHILE"),
            region: EntryRegion.Text?.Trim() ?? "Zona Central",
            ingredientes: new List<IngredienteReceta>(_ingredientesActuales),
            pasos: new List<PasoPreparacion>(_pasosActuales),
            notasPersonales: EntryNotasPersonales.Text?.Trim() ?? string.Empty,
            soloDispositivoLocal: true);
    }

    private void OnValidarRecetaClicked(object? sender, EventArgs e)
    {
        var borrador = ConstruirRecetaDesdeFormulario();
        if (borrador.EsValido())
        {
            BannerValidacion.BackgroundColor = Color.FromArgb("#E8EDE4");
            BannerValidacion.Stroke = Color.FromArgb("#758B68");
            LblEstadoValidacion.Text = $"Validación Exitosa: '{borrador.Titulo}' cumple todas las reglas.";
        }
        else
        {
            BannerValidacion.BackgroundColor = Color.FromArgb("#F5E5DF");
            BannerValidacion.Stroke = Color.FromArgb("#C8785A");
            LblEstadoValidacion.Text = "Errores de Validación:\n• " + string.Join("\n• ", borrador.ObtenerErroresValidacion());
        }
    }

    private async void OnGuardarRecetaPrivadaClicked(object? sender, EventArgs e)
    {
        var receta = ConstruirRecetaDesdeFormulario();
        if (!receta.EsValido())
        {
            OnValidarRecetaClicked(sender, e);
            await DisplayAlertAsync("Validación de Receta",
                string.Join("\n• ", receta.ObtenerErroresValidacion()),
                "Corregir");
            return;
        }

        bool eraEdicion = !string.IsNullOrWhiteSpace(_idRecetaEnEdicion);
        if (eraEdicion)
        {
            _service.EditarRecetaPrivada(receta);
        }
        else
        {
            _service.GuardarRecetaPrivada(receta);
        }

        await DisplayAlertAsync(
            eraEdicion ? "Receta Editada (UC02)" : "Receta Creada",
            $"'{receta.Titulo}' fue validada (UC04) y almacenada.",
            "Ir a Mi Recetario");

        CargarDatosPorDefecto();
        _service.NavegarAPestana("MisRecetas");
    }

    private async void OnGuardarYPublicarFeedClicked(object? sender, EventArgs e)
    {
        var recetaPrivada = ConstruirRecetaDesdeFormulario();
        if (!recetaPrivada.EsValido())
        {
            OnValidarRecetaClicked(sender, e);
            await DisplayAlertAsync("Validación de Receta",
                string.Join("\n• ", recetaPrivada.ObtenerErroresValidacion()),
                "Corregir");
            return;
        }

        _service.GuardarRecetaPrivada(recetaPrivada);

        var chef = _service.UsuarioActivo as CreadorEspecialista ?? _service.PerfilCreadorEspecialista;
        var recetaPublica = chef.PublicarRecetaOficial(recetaPrivada);
        _service.PublicarRecetaEnFeed(recetaPublica, chef);

        await DisplayAlertAsync("Publicada en Feed Global",
            $"'{recetaPublica.Titulo}' se guardó y publicó en el Feed Global certificada por {chef.NombreUsuario()}.",
            "Ver en Feed Global");

        CargarDatosPorDefecto();
        _service.NavegarAPestana("Feed");
    }

    private void OnLimpiarFormularioClicked(object? sender, EventArgs e)
    {
        CargarDatosPorDefecto();
    }

    private void OnImagenSeleccionadaChanged(object? sender, EventArgs e)
    {
        if (PickerIcono.SelectedItem is string nombreArchivo)
        {
            _rutaImagenPersonalizada = string.Empty;
            ImgVistaPrevia.Source = nombreArchivo;
        }
    }

    private async void OnExaminarImagenPcClicked(object? sender, EventArgs e)
    {
        var resultado = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona una fotografía para el platillo",
            FileTypes = FilePickerFileType.Images
        });

        if (resultado != null && !string.IsNullOrWhiteSpace(resultado.FullPath))
        {
            _rutaImagenPersonalizada = resultado.FullPath;
            ImgVistaPrevia.Source = _rutaImagenPersonalizada;
        }
    }

    private async void OnTomarFotoCLicked(object? sender, EventArgs e)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            await DisplayAlertAsync("Cámara no disponible", "Este dispositivo no cuenta con cámara disponible.", "Aceptar");
            return;
        }

        FileResult? foto = await MediaPicker.Default.CapturePhotoAsync();
        if (foto != null)
        {
            string rutaDestino = Path.Combine(FileSystem.Current.AppDataDirectory, $"foto_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
            using Stream streamOrigen = await foto.OpenReadAsync();
            using FileStream streamDestino = File.OpenWrite(rutaDestino);
            await streamOrigen.CopyToAsync(streamDestino);

            _rutaImagenPersonalizada = rutaDestino;
            ImgVistaPrevia.Source = _rutaImagenPersonalizada;
        }
    }
}
