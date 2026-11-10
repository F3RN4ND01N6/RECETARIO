using Recetario.Models;
using Recetario.Services;

namespace Recetario.Views;


// Partial significa que la clase CrearEditarRecetaPage está definida en varios archivos, y el compilador los combinará en una sola clase durante la compilación.
// CrearEditarRecetaPage es la vista de formulario encargada de crear nuevas recetas o actualizar recetas privadas existentes con validaciones de negocio.
public partial class CrearEditarRecetaPage : ContentPage
{

    // readonly significa que estas variables solo pueden ser asignadas en el constructor de la clase y no pueden ser modificadas.
    private readonly RecetaService _service; // servicio central de recetas para guardar, actualizar y navegar entre pestañas.

    // Variable nullable (string?) que almacena el ID de la receta que se esta editando actualmente, o null si se esta creando una nueva.
    private string? _idRecetaEnEdicion;

    // Ruta absoluta o nombre de recurso de la imagen seleccionada por el usuario desde archivo o cámara.
    private string _rutaImagenPersonalizada = string.Empty;

    // Colecciones temporales para manipular en memoria los ingredientes y pasos que componen la receta antes de guardarla.
    private readonly List<IngredienteReceta> _ingredientesActuales = new();
    private readonly List<PasoPreparacion> _pasosActuales = new();


    // Constructor de la página CrearEditarRecetaPage, recibe el servicio inyectado y prepara el formulario.
    public CrearEditarRecetaPage(RecetaService service)
    {
        InitializeComponent(); // inicia interfaz de usuario y carga los XAML asociados a esta clase.
        _service = service; // asigna servicio de recetas a variable privada para usarlo en toda la clase.

        //   1. CARGA LAS OPCIONES DE TODOS LOS SELECTORES (PICKERS)
        InicializarSelectores();

        //   2. SUSCRIBE EL EVENTO PARA RECIBIR UNA RECETA CUANDO SE PULSA "EDITAR" (UC02)
        _service.SolicitudEditarReceta += CargarRecetaParaEdicion;

        //   3. PREPARA LOS VALORES INICIALES DEL FORMULARIO
        CargarDatosPorDefecto();
    }


    // Metodo privado que carga las listas de opciones en todos los Pickers del formulario desde listas y enumeradores.
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
        PickerIcono.SelectedIndex = 0; // imagen por defecto
        ImgVistaPrevia.Source = "platillo_default.jpg"; // muestra la imagen por defecto en la vista previa



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
        PickerDificultad.ItemsSource = Enum.GetNames<NivelDificultad>().ToList(); // obtiene los nombres del enum NivelDificultad
        PickerDificultad.SelectedIndex = 0; // FACIL

        PickerEstacion.ItemsSource = Enum.GetNames<Estacion>().ToList(); // obtiene los nombres del enum Estacion
        PickerEstacion.SelectedIndex = 1; // PRIMAVERA

        PickerOrigen.ItemsSource = Enum.GetNames<Origen>().ToList(); // obtiene los nombres del enum Origen
        PickerOrigen.SelectedIndex = 0; // CHILE

        PickerUnidadIngrediente.ItemsSource = Enum.GetNames<UnidadMedida>().ToList(); // obtiene unidades de medida del ingrediente
        PickerUnidadIngrediente.SelectedIndex = 6; // UNIDADES

        PickerTipoVegetalIng.ItemsSource = Enum.GetNames<TipoVegetal>().ToList(); // obtiene tipos de vegetales del ingrediente
        PickerTipoVegetalIng.SelectedIndex = 1; // VERDURA
    }


    // Metodo privado que limpia y restablece el formulario al modo "Crear Nueva Receta" con valores predeterminados.
    private void CargarDatosPorDefecto()
    {
        _idRecetaEnEdicion = null; // null indica que estamos en modo creacion y no edicion.
        _rutaImagenPersonalizada = string.Empty;

        PickerIcono.SelectedIndex = 0;
        ImgVistaPrevia.Source = "platillo_default.jpg";

        LblTituloModo.Text = "Crear Nueva Receta"; // titulo del encabezado
        LblSubtituloModo.Text = "Completa los datos generales, ingredientes y pasos. Todos los campos son verificados.";
        BtnGuardarPrivada.Text = "Guardar en Mi Recetario"; // texto del boton de guardar

        EntryTitulo.Text = string.Empty; // vacia campo de titulo
        EntryEtiquetas.Text = "Temporada, Casera";
        EntryTiempo.Text = "30";
        EntryPorciones.Text = "4";
        EntryRegion.Text = "Valle Central";
        EntryNotasPersonales.Text = string.Empty;

        _ingredientesActuales.Clear(); // limpia la lista temporal de ingredientes
        _ingredientesActuales.Add(new IngredienteReceta("Zapallo Camote en cubos", 500, UnidadMedida.GRAMO, Estacion.OTONIO, true, TipoVegetal.VERDURA)); // ingrediente de ejemplo

        _pasosActuales.Clear(); // limpia la lista temporal de pasos
        _pasosActuales.Add(new PasoPreparacion(1, "Lavar, pelar y cortar los vegetales de estación en trozos regulares.", 10)); // paso de ejemplo

        RefrescarListasFormulario(); // dibuja los ingredientes y pasos en la pantalla
    }


    // Metodo privado que se invoca cuando el usuario presiona "Editar" en MisRecetasPage para cargar una receta existente en el formulario.
    private void CargarRecetaParaEdicion(RecetaPrivada receta)
    {
        _idRecetaEnEdicion = receta.ObtenerIdentificador(); // guarda el ID de la receta existente para saber que se esta editando.

        LblTituloModo.Text = $"Editando Receta '{receta.Titulo}' (ID: {_idRecetaEnEdicion} ";
        LblSubtituloModo.Text = "Modifica cualquier campo, ingrediente o paso y presiona Actualizar.";
        BtnGuardarPrivada.Text = "Actualizar Receta Editada"; // cambia el texto del boton a modo edicion

        EntryTitulo.Text = receta.Titulo;
        _rutaImagenPersonalizada = receta.ImagenPlatillo;

        // Comprueba si la imagen de la receta coincide con las opciones del Picker para seleccionarla automaticamente.
        if (PickerIcono.ItemsSource is IList<string> items && items.Contains(receta.ImagenPlatillo))
        {
            PickerIcono.SelectedItem = receta.ImagenPlatillo;
            _rutaImagenPersonalizada = string.Empty;
        }

        ImgVistaPrevia.Source = receta.ImagenPlatillo;
        PickerCategoria.SelectedItem = receta.Categoria;
        PickerDieta.SelectedItem = receta.TipoDieta;
        EntryEtiquetas.Text = string.Join(", ", receta.Etiquetas); // une las etiquetas en una sola cadena separada por comas
        EntryTiempo.Text = receta.TiempoPreparacion.ToString();
        EntryPorciones.Text = receta.Porciones.ToString();
        PickerDificultad.SelectedItem = receta.Dificultad.ToString();
        PickerEstacion.SelectedItem = receta.Estacion.ToString();
        PickerOrigen.SelectedItem = receta.Origen.ToString();
        EntryRegion.Text = receta.Region;
        EntryNotasPersonales.Text = receta.NotasPersonales();

        _ingredientesActuales.Clear();
        _ingredientesActuales.AddRange(receta.Ingredientes); // clona los ingredientes de la receta en la lista temporal

        _pasosActuales.Clear();
        _pasosActuales.AddRange(receta.Pasos); // clona los pasos de la receta en la lista temporal

        RefrescarListasFormulario(); // actualiza la visualizacion de ingredientes y pasos en pantalla
    }


    // Metodo privado que redibuja las listas dinamicas de ingredientes y pasos con sus botones de eliminacion.
    private void RefrescarListasFormulario()
    {
        //   1. RECONSTRUYE LA LISTA VISUAL DE INGREDIENTES (COMPOSICION 1..*)
        StackListaIngredientes.Children.Clear();

        for (int i = 0; i < _ingredientesActuales.Count; i++)
        {
            int index = i; // captura el indice local para el closure del evento Clicked
            var ing = _ingredientesActuales[i];

            // Boton para quitar el ingrediente de la lista temporal
            var btnQuitar = new Button
            {
                Text = "✖",
                BackgroundColor = Color.FromArgb("#C8785A"), // terracota
                TextColor = Color.FromArgb("#FFFFFF"),
                CornerRadius = 8,
                Padding = new Thickness(10, 4),
                FontSize = 12
            };
            btnQuitar.Clicked += (_, _) =>
            {
                _ingredientesActuales.RemoveAt(index); // elimina el ingrediente de la coleccion por su indice
                RefrescarListasFormulario(); // redibuja la lista
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }, // texto del ingrediente
                    new ColumnDefinition { Width = GridLength.Auto } // boton de eliminar
                }
            };
            grid.Add(new Label
            {
                Text = $"• {ing.FormatearIngrediente()}", // texto con formato de cantidad y unidad
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
            paso.NumeroPasoBind = i + 1; // actualiza la numeracion correlativa del paso (1, 2, 3...)

            // Boton para quitar el paso de preparacion
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
                _pasosActuales.RemoveAt(index); // elimina el paso por indice
                RefrescarListasFormulario(); // redibuja la lista de pasos
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
                Text = paso.FormatearPaso(), // texto formateado con numero de paso y duracion
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


    // Metodo que se ejecuta al presionar "Agregar Ingrediente" para validar los datos e ingresarlo a la lista temporal.
    private void OnAgregarIngredienteClicked(object? sender, EventArgs e)
    {
        string nombre = EntryNombreIngrediente.Text?.Trim() ?? string.Empty; // obtiene el nombre del ingrediente sin espacios vacios
        if (string.IsNullOrWhiteSpace(nombre)) // si no tiene nombre no lo agrega
            return;

        double.TryParse(EntryCantidadIngrediente.Text, out double cantidad); // parsea la cantidad numerica de forma segura
        if (cantidad <= 0) cantidad = 1; // si es menor o igual a cero asigna 1 por defecto

        var unidad = Enum.Parse<UnidadMedida>(PickerUnidadIngrediente.SelectedItem?.ToString() ?? "UNIDADES");
        var tipoVeg = Enum.Parse<TipoVegetal>(PickerTipoVegetalIng.SelectedItem?.ToString() ?? "VERDURA");
        var estacion = Enum.Parse<Estacion>(PickerEstacion.SelectedItem?.ToString() ?? "PRIMAVERA");

        // Instancia un nuevo IngredienteReceta y lo agrega a la coleccion temporal en memoria
        _ingredientesActuales.Add(new IngredienteReceta(
            nombre: nombre,
            cantidad: cantidad,
            unidad: unidad,
            estacion: estacion,
            esFrutaOVerduraEstacion: tipoVeg is TipoVegetal.FRUTA or TipoVegetal.VERDURA,
            tipoVegetal: tipoVeg));

        EntryNombreIngrediente.Text = string.Empty; // limpia el campo de texto del nombre
        RefrescarListasFormulario(); // actualiza la interfaz
    }


    // Metodo que se ejecuta al presionar "Agregar Paso" para agregar una nueva instruccion de preparacion.
    private void OnAgregarPasoClicked(object? sender, EventArgs e)
    {
        string instruccion = EntryInstruccionPaso.Text?.Trim() ?? string.Empty; // texto de la instruccion
        if (string.IsNullOrWhiteSpace(instruccion))
            return;

        int.TryParse(EntryMinutosPaso.Text, out int minutos); // duracion en minutos
        if (minutos <= 0) minutos = 5; // minimo 5 minutos si no se especifica

        _pasosActuales.Add(new PasoPreparacion(_pasosActuales.Count + 1, instruccion, minutos)); // agrega el nuevo paso
        EntryInstruccionPaso.Text = string.Empty; // limpia el campo de texto de instruccion
        RefrescarListasFormulario(); // actualiza la vista de pasos
    }


    // Metodo privado que lee todos los campos de texto, pickers y listas temporales para crear una instancia completa de RecetaPrivada.
    private RecetaPrivada ConstruirRecetaDesdeFormulario()
    {
        int.TryParse(EntryTiempo.Text, out int tiempo); // convierte tiempo de preparacion a entero
        int.TryParse(EntryPorciones.Text, out int porciones); // convierte porciones a entero

        // Divide las etiquetas ingresadas separadas por comas en una lista de cadenas limpias sin espacios
        var etiquetas = (EntryEtiquetas.Text ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        // Determina la imagen a utilizar: personalizada de archivo/camara o la seleccionada en el Picker
        string imagenElegida = !string.IsNullOrWhiteSpace(_rutaImagenPersonalizada)
            ? _rutaImagenPersonalizada
            : (PickerIcono.SelectedItem?.ToString() ?? "platillo_default.jpg");

        // Retorna un nuevo objeto RecetaPrivada con todos sus atributos validados
        return new RecetaPrivada(
            idReceta: _idRecetaEnEdicion ?? $"PRIV-{Guid.NewGuid().ToString("N")[..5].ToUpperInvariant()}", // si es nueva genera ID con Guid
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
            ingredientes: new List<IngredienteReceta>(_ingredientesActuales), // clona ingredientes
            pasos: new List<PasoPreparacion>(_pasosActuales), // clona pasos
            notasPersonales: EntryNotasPersonales.Text?.Trim() ?? string.Empty,
            soloDispositivoLocal: true);
    }


    // Metodo que valida las reglas de negocio de la receta en construcción usando la interfaz IValidable y muestra el resultado en un banner.
    private void OnValidarRecetaClicked(object? sender, EventArgs e)
    {
        var borrador = ConstruirRecetaDesdeFormulario(); // construye la receta temporal
        if (borrador.EsValido()) // si cumple todas las reglas de IValidable
        {
            BannerValidacion.BackgroundColor = Color.FromArgb("#E8EDE4"); // verde suave
            BannerValidacion.Stroke = Color.FromArgb("#758B68");
            LblEstadoValidacion.Text = $"Validación Exitosa: '{borrador.Titulo}' cumple todas las reglas.";
        }
        else // si tiene errores de validacion
        {
            BannerValidacion.BackgroundColor = Color.FromArgb("#F5E5DF"); // rojo suave
            BannerValidacion.Stroke = Color.FromArgb("#C8785A");
            LblEstadoValidacion.Text = "Errores de Validación:\n• " + string.Join("\n• ", borrador.ObtenerErroresValidacion());
        }
    }


    // async metodo para guardar o actualizar la receta en el recetario privado local.
    private async void OnGuardarRecetaPrivadaClicked(object? sender, EventArgs e)
    {
        var receta = ConstruirRecetaDesdeFormulario();
        if (!receta.EsValido()) // si la receta no es valida muestra los errores y detiene la operacion
        {
            OnValidarRecetaClicked(sender, e);
            await DisplayAlertAsync("Validación de Receta",
                string.Join("\n• ", receta.ObtenerErroresValidacion()),
                "Corregir");
            return;
        }

        bool eraEdicion = !string.IsNullOrWhiteSpace(_idRecetaEnEdicion); // verifica si estabamos editando o creando
        if (eraEdicion)
        {
            _service.EditarRecetaPrivada(receta); // actualiza receta existente
        }
        else
        {
            _service.GuardarRecetaPrivada(receta); // registra nueva receta
        }

        await DisplayAlertAsync( // cuadro de dialogo de confirmacion
            eraEdicion ? "Receta Editada" : "Receta Creada",
            $"'{receta.Titulo}' fue validada y almacenada.",
            "Ir a Mi Recetario");

        CargarDatosPorDefecto(); // reinicia formulario a modo creacion
        _service.NavegarAPestana("MisRecetas"); // navega a la pestaña MisRecetas
    }


    // async metodo que guarda la receta privada y ademas la certifica y publica en el feed publico global.
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

        _service.GuardarRecetaPrivada(recetaPrivada); // guarda en el recetario privado

        // Obtiene el perfil de chef o el creador especialista certificado
        var chef = _service.UsuarioActivo as CreadorEspecialista ?? _service.PerfilCreadorEspecialista;
        var recetaPublica = chef.PublicarRecetaOficial(recetaPrivada); // certifica y convierte a receta publica
        _service.PublicarRecetaEnFeed(recetaPublica, chef); // serializa y publica en el feed

        await DisplayAlertAsync("Publicada en Feed Global",
            $"'{recetaPublica.Titulo}' se guardó y publicó en el Feed Global certificada por {chef.NombreUsuario()}.",
            "Ver en Feed Global");

        CargarDatosPorDefecto();
        _service.NavegarAPestana("Feed"); // navega al Feed para ver la receta publicada
    }


    // Metodo que reinicia todos los campos del formulario al presionar el boton de limpiar.
    private void OnLimpiarFormularioClicked(object? sender, EventArgs e)
    {
        CargarDatosPorDefecto();
    }


    // Metodo que actualiza la imagen de vista previa cuando el usuario cambia la seleccion en el desplegable de iconos.
    private void OnImagenSeleccionadaChanged(object? sender, EventArgs e)
    {
        if (PickerIcono.SelectedItem is string nombreArchivo)
        {
            _rutaImagenPersonalizada = string.Empty;
            ImgVistaPrevia.Source = nombreArchivo; // asigna el nombre del archivo de imagen
        }
    }


    // async metodo para seleccionar una imagen desde el almacenamiento del computador o telefono usando FilePicker.
    private async void OnExaminarImagenPcClicked(object? sender, EventArgs e)
    {
        var resultado = await FilePicker.Default.PickAsync(new PickOptions // abre el selector de archivos del sistema operativo
        {
            PickerTitle = "Selecciona una fotografía para el platillo",
            FileTypes = FilePickerFileType.Images // restringe la seleccion solo a imagenes
        });

        if (resultado != null && !string.IsNullOrWhiteSpace(resultado.FullPath))
        {
            _rutaImagenPersonalizada = resultado.FullPath; // guarda la ruta absoluta del archivo seleccionado
            ImgVistaPrevia.Source = _rutaImagenPersonalizada; // actualiza la vista previa con la imagen seleccionada
        }
    }


    // async metodo para capturar una fotografia directamente desde la camara del dispositivo usando MediaPicker.
    private async void OnTomarFotoCLicked(object? sender, EventArgs e)
    {
        if (!MediaPicker.Default.IsCaptureSupported) // valida si el dispositivo cuenta con soporte para camara
        {
            await DisplayAlertAsync("Cámara no disponible", "Este dispositivo no cuenta con cámara disponible.", "Aceptar");
            return;
        }

        FileResult? foto = await MediaPicker.Default.CapturePhotoAsync(); // abre la camara y espera a que el usuario tome la foto
        if (foto != null)
        {
            // Guarda una copia de la fotografia en el directorio local de datos de la aplicacion
            string rutaDestino = Path.Combine(FileSystem.Current.AppDataDirectory, $"foto_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
            using Stream streamOrigen = await foto.OpenReadAsync();
            using FileStream streamDestino = File.OpenWrite(rutaDestino);
            await streamOrigen.CopyToAsync(streamDestino); // copia los bytes de la foto al archivo local

            _rutaImagenPersonalizada = rutaDestino; // guarda la ruta de la foto capturada
            ImgVistaPrevia.Source = _rutaImagenPersonalizada; // muestra la foto tomada en la vista previa
        }
    }
}
