using System.Text.Json.Serialization;

namespace Recetario.Models;

/// <summary>
/// Clase base abstracta para todas las recetas del sistema (Herencia 4).
/// Implementa IIdentificable e IValidable (UC04).
/// </summary>
// abstract significa que esta clase no puede ser instanciada por sí sola, sirve como base para RecetaPrivada y RecetaPublica.
public abstract class Receta : IIdentificable, IValidable
{
    // protected significa que estos atributos son visibles dentro de esta clase y en sus clases derivadas.
    [JsonInclude]
    protected string idReceta = Guid.NewGuid().ToString("N")[..8]; // clave primaria única de la receta

    [JsonInclude]
    protected string idUsuarioAutor = string.Empty; // identificador del usuario que creó la receta

    [JsonInclude]
    protected string titulo = string.Empty; // nombre o título de la receta

    [JsonInclude]
    protected string categoria = "Plato Principal"; // categoría gastronómica (ej: Postre, Sopa)

    [JsonInclude]
    protected string tipoDieta = "Omnívora"; // clasificación de dieta (ej: Vegana, Vegetariana)

    [JsonInclude]
    protected List<string> etiquetas = new(); // lista de etiquetas o tags culinarios

    [JsonInclude]
    protected string imagenPlatillo = "platillo_default.jpg"; // nombre del archivo de imagen o ruta local

    [JsonInclude]
    protected int tiempoPreparacion = 30; // duración estimada en minutos

    [JsonInclude]
    protected int porciones = 4; // cantidad de comensales o porciones

    [JsonInclude]
    protected NivelDificultad dificultad = NivelDificultad.FACIL; // dificultad: FACIL, MEDIA o DIFICIL

    [JsonInclude]
    protected Estacion estacion = Estacion.TODOELANHIO; // temporada estacional de la receta

    [JsonInclude]
    protected DateTime fechaPublicacion = DateTime.Now; // fecha de creación o publicación

    [JsonInclude]
    protected Origen origen = Origen.CHILE; // país de origen culinario

    [JsonInclude]
    protected string region = "Zona Central"; // región geográfica de la receta

    // Composición 1..*: una receta se compone de al menos un ingrediente y un paso
    [JsonInclude]
    protected List<IngredienteReceta> ingredientes = new(); // lista de ingredientes que componen la receta

    [JsonInclude]
    protected List<PasoPreparacion> pasos = new(); // lista secuencial de pasos para cocinar la receta


    // Constructor protegido vacío necesario para la serialización y deserialización JSON.
    protected Receta()
    {
    }


    // Constructor protegido con parámetros para inicializar todos los atributos comunes de la receta.
    protected Receta(
        string idReceta,
        string idUsuarioAutor,
        string titulo,
        string categoria,
        string tipoDieta,
        List<string> etiquetas,
        string imagenPlatillo,
        int tiempoPreparacion,
        int porciones,
        NivelDificultad dificultad,
        Estacion estacion,
        Origen origen,
        string region,
        List<IngredienteReceta> ingredientes,
        List<PasoPreparacion> pasos,
        DateTime? fechaPublicacion = null)
    {
        this.idReceta = string.IsNullOrWhiteSpace(idReceta) ? Guid.NewGuid().ToString("N")[..8] : idReceta;
        this.idUsuarioAutor = idUsuarioAutor;
        this.titulo = titulo;
        this.categoria = categoria;
        this.tipoDieta = tipoDieta;
        this.etiquetas = etiquetas ?? new List<string>();
        this.imagenPlatillo = NormalizarRutaImagen(imagenPlatillo, titulo);
        this.tiempoPreparacion = tiempoPreparacion;
        this.porciones = porciones;
        this.dificultad = dificultad;
        this.estacion = estacion;
        this.fechaPublicacion = fechaPublicacion ?? DateTime.Now;
        this.origen = origen;
        this.region = region;
        this.ingredientes = ingredientes ?? new List<IngredienteReceta>();
        this.pasos = pasos ?? new List<PasoPreparacion>();
    }

    // Propiedades públicas respaldadas por los atributos protegidos del Diagrama de Clases
    [JsonIgnore]
    public string IdReceta
    {
        get => idReceta;
        set => idReceta = value;
    }

    [JsonIgnore]
    public string IdUsuarioAutor
    {
        get => idUsuarioAutor;
        set => idUsuarioAutor = value;
    }

    [JsonIgnore]
    public string Titulo
    {
        get => titulo;
        set => titulo = value;
    }

    [JsonIgnore]
    public string Categoria
    {
        get => categoria;
        set => categoria = value;
    }

    [JsonIgnore]
    public string TipoDieta
    {
        get => tipoDieta;
        set => tipoDieta = value;
    }

    [JsonIgnore]
    public List<string> Etiquetas
    {
        get => etiquetas;
        set => etiquetas = value;
    }

    [JsonIgnore]
    public string ImagenPlatillo
    {
        get => NormalizarRutaImagen(imagenPlatillo, titulo);
        set => imagenPlatillo = NormalizarRutaImagen(value, titulo);
    }


    // static metodo que resuelve la ruta o nombre de archivo de la fotografía a partir del nombre o emoji anterior.
    public static string NormalizarRutaImagen(string? valor, string? tituloReceta = null)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return "platillo_default.jpg";

        string limpio = valor.Trim();
        // Valida extensiones de imagen estándar o rutas de red/locales
        if (limpio.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            limpio.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            limpio.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            limpio.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
            limpio.Contains('\\') || limpio.Contains('/'))
        {
            return limpio;
        }

        // Mapeo automático de emojis anteriores a las fotografías descargadas en Resources/Images
        return limpio switch
        {
            "🌽" => "pastelera_choclo.jpg",
            "🎃" => "crema_zapallo.jpg",
            "🥦" => "salteado_brocoli.jpg",
            "🍓" => "tartaleta_frutilla.jpg",
            "🍑" => "tostadas_durazno.jpg",
            "🥭" => "ceviche_mango.jpg",
            _ when tituloReceta != null && tituloReceta.Contains("Choclo", StringComparison.OrdinalIgnoreCase) => "pastelera_choclo.jpg",
            _ when tituloReceta != null && tituloReceta.Contains("Zapallo", StringComparison.OrdinalIgnoreCase) => "crema_zapallo.jpg",
            _ when tituloReceta != null && tituloReceta.Contains("Brócoli", StringComparison.OrdinalIgnoreCase) => "salteado_brocoli.jpg",
            _ when tituloReceta != null && tituloReceta.Contains("Frutilla", StringComparison.OrdinalIgnoreCase) => "tartaleta_frutilla.jpg",
            _ when tituloReceta != null && tituloReceta.Contains("Durazno", StringComparison.OrdinalIgnoreCase) => "tostadas_durazno.jpg",
            _ when tituloReceta != null && tituloReceta.Contains("Mango", StringComparison.OrdinalIgnoreCase) => "ceviche_mango.jpg",
            _ => "platillo_default.jpg"
        };
    }

    [JsonIgnore]
    public int TiempoPreparacion
    {
        get => tiempoPreparacion;
        set => tiempoPreparacion = value;
    }

    [JsonIgnore]
    public int Porciones
    {
        get => porciones;
        set => porciones = value;
    }

    [JsonIgnore]
    public NivelDificultad Dificultad
    {
        get => dificultad;
        set => dificultad = value;
    }

    [JsonIgnore]
    public Estacion Estacion
    {
        get => estacion;
        set => estacion = value;
    }

    [JsonIgnore]
    public DateTime FechaPublicacion
    {
        get => fechaPublicacion;
        set => fechaPublicacion = value;
    }

    [JsonIgnore]
    public Origen Origen
    {
        get => origen;
        set => origen = value;
    }

    [JsonIgnore]
    public string Region
    {
        get => region;
        set => region = value;
    }

    [JsonIgnore]
    public List<IngredienteReceta> Ingredientes
    {
        get => ingredientes;
        set => ingredientes = value;
    }

    [JsonIgnore]
    public List<PasoPreparacion> Pasos
    {
        get => pasos;
        set => pasos = value;
    }

    // Propiedades formateadas para las tarjetas XAML
    [JsonIgnore]
    public string EtiquetasTexto => etiquetas.Count > 0 ? string.Join(" • #", new[] { "" }.Concat(etiquetas)).TrimStart(' ', '•') : "#Casera";

    [JsonIgnore]
    public string MetaResumenTexto => $"⏱ {tiempoPreparacion} min  |  🍽 {porciones} porc.  |  📊 {dificultad}";

    [JsonIgnore]
    public string UbicacionEstacionTexto => $"📍 {origen} ({region})  •  🌤 {estacion}";

    [JsonIgnore]
    public string CategoriaYDietaTexto => $"{categoria} • Dieta {tipoDieta}";

    // Métodos exactos del Diagrama de Clases
    public string ObtenerIdentificador() => idReceta; // devuelve la clave primaria de la receta


    // Implementación de IValidable: valida si la receta cumple todas las reglas de negocio.
    public bool EsValido() => ObtenerErroresValidacion().Count == 0;

    /// <summary>
    /// Caso de Uso UC04: Validar Datos de la Receta (Composición 1..* de Ingredientes y Pasos).
    /// </summary>
    // Valida que el título, categoría, dieta, tiempo, porciones, ingredientes y pasos cumplan con las reglas de negocio.
    public List<string> ObtenerErroresValidacion()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(titulo) || titulo.Trim().Length < 3)
            errores.Add("El título de la receta debe tener al menos 3 caracteres.");

        if (string.IsNullOrWhiteSpace(categoria))
            errores.Add("Debe asignar una categoría a la receta (UC03).");

        if (string.IsNullOrWhiteSpace(tipoDieta))
            errores.Add("Debe especificar el tipo de dieta (UC03).");

        if (tiempoPreparacion <= 0)
            errores.Add("El tiempo de preparación debe ser mayor a 0 minutos.");

        if (porciones <= 0)
            errores.Add("El número de porciones debe ser al menos 1.");

        // Regla de composición 1..*: debe contener al menos un ingrediente válido
        if (ingredientes == null || ingredientes.Count == 0)
        {
            errores.Add("La receta debe contener al menos 1 ingrediente (Composición 1..*).");
        }
        else
        {
            foreach (var ing in ingredientes)
            {
                if (!ing.EsValido())
                    errores.AddRange(ing.ObtenerErroresValidacion());
            }
        }

        // Regla de composición 1..*: debe contener al menos un paso con instrucción
        if (pasos == null || pasos.Count == 0)
        {
            errores.Add("La receta debe tener al menos 1 paso de preparación (Composición 1..*).");
        }
        else if (pasos.Any(p => string.IsNullOrWhiteSpace(p.Instruccion())))
        {
            errores.Add("Todos los pasos de preparación deben tener una instrucción escrita.");
        }

        return errores;
    }

    // Métodos abstractos que cada clase derivada implementa polimórficamente
    public abstract bool EsPublicaEnFeed();
    public abstract string ObtenerResumenEntidad();
}

/// <summary>
/// Receta privada del usuario almacenada en el respaldo JSON local (UC01, UC14, UC15).
/// </summary>
// RecetaPrivada hereda de Receta y representa las recetas guardadas de forma privada y local en el dispositivo del usuario.
public class RecetaPrivada : Receta
{
    [JsonInclude]
    private string notasPersonales = string.Empty; // anotaciones privadas exclusivas del usuario

    [JsonInclude]
    private bool soloDispositivoLocal = true; // bandera que indica que no se comparte en el feed público


    // Constructor vacío por defecto para la serialización JSON.
    public RecetaPrivada() : base()
    {
    }


    // Constructor con parámetros que invoca al constructor de la clase base con base(...).
    public RecetaPrivada(
        string idReceta,
        string idUsuarioAutor,
        string titulo,
        string categoria,
        string tipoDieta,
        List<string> etiquetas,
        string imagenPlatillo,
        int tiempoPreparacion,
        int porciones,
        NivelDificultad dificultad,
        Estacion estacion,
        Origen origen,
        string region,
        List<IngredienteReceta> ingredientes,
        List<PasoPreparacion> pasos,
        string notasPersonales = "",
        bool soloDispositivoLocal = true)
        :base(idReceta, idUsuarioAutor, titulo, categoria, tipoDieta, etiquetas, imagenPlatillo,
               tiempoPreparacion, porciones, dificultad, estacion, origen, region, ingredientes, pasos)
    {
        this.notasPersonales = notasPersonales ?? string.Empty;
        this.soloDispositivoLocal = soloDispositivoLocal;
    }

    // Métodos exactos del Diagrama de Clases
    public string NotasPersonales() => notasPersonales;
    public bool SoloDispositivoLocal() => soloDispositivoLocal;

    public void ActualizarNotasPrivadas(string notas, bool soloLocal)
    {
        notasPersonales = notas ?? string.Empty;
        soloDispositivoLocal = soloLocal;
    }

    // override indica que una receta privada nunca se lista directamente en el feed público global
    public override bool EsPublicaEnFeed() => false;


    // override devuelve el resumen descriptivo polimórfico de una receta privada
    public override string ObtenerResumenEntidad() =>
        $"[Privada 🔒] {titulo} ({categoria} - {estacion}) | {ingredientes.Count} ingr., {pasos.Count} pasos | Nota: {(string.IsNullOrWhiteSpace(notasPersonales) ? "Sin notas" : notasPersonales)}";

    [JsonIgnore]
    public string NotasPersonalesBind
    {
        get => notasPersonales;
        set => notasPersonales = value;
    }

    [JsonIgnore]
    public string BadgePrivacidadTexto => soloDispositivoLocal ? "🔒 Respaldo JSON Local" : "☁ Lista para publicar";
}

/// <summary>
/// Receta pública disponible en el Feed Global, con soporte de calificaciones y filtrado (UC05, UC06, UC09, UC16, UC17).
/// Implementa ICalificable e IFiltrable.
/// </summary>
// RecetaPublica hereda de Receta e implementa ICalificable e IFiltrable para permitir calificaciones y filtros en el Feed Global.
public class RecetaPublica : Receta, ICalificable, IFiltrable
{
    [JsonInclude]
    private string especialidadChef = "Cocina Tradicional de Estación"; // especialidad culinaria del autor

    [JsonInclude]
    private bool verificadaPorChef = false; // sello oficial otorgado por un creador especialista

    [JsonInclude]
    private bool destacadaEnFeed = false; // indica si la receta se resalta visualmente en el feed

    [JsonInclude]
    private List<Calificacion> calificaciones = new(); // lista de calificaciones y comentarios de los usuarios


    // Constructor vacío por defecto para serialización JSON.
    public RecetaPublica() : base()
    {
    }


    // Constructor con parámetros que invoca al constructor de la clase base con base(...).
    public RecetaPublica(
        string idReceta,
        string idUsuarioAutor,
        string titulo,
        string categoria,
        string tipoDieta,
        List<string> etiquetas,
        string imagenPlatillo,
        int tiempoPreparacion,
        int porciones,
        NivelDificultad dificultad,
        Estacion estacion,
        Origen origen,
        string region,
        List<IngredienteReceta> ingredientes,
        List<PasoPreparacion> pasos,
        string especialidadChef,
        bool verificadaPorChef = true,
        bool destacadaEnFeed = false,
        List<Calificacion>? calificaciones = null)
        : base(idReceta, idUsuarioAutor, titulo, categoria, tipoDieta, etiquetas, imagenPlatillo,
               tiempoPreparacion, porciones, dificultad, estacion, origen, region, ingredientes, pasos)
    {
        this.especialidadChef = especialidadChef;
        this.verificadaPorChef = verificadaPorChef;
        this.destacadaEnFeed = destacadaEnFeed;
        this.calificaciones = calificaciones ?? new List<Calificacion>();
    }

    // Métodos exactos del Diagrama de Clases
    public string EspecialidadChef() => especialidadChef;
    public bool VerificadaPorChef() => verificadaPorChef;
    public bool DestacadaEnFeed() => destacadaEnFeed;
    public IReadOnlyList<Calificacion> ObtenerCalificaciones() => calificaciones;

    // override indica que esta receta sí es pública y debe mostrarse en el Feed Global
    public override bool EsPublicaEnFeed() => true;


    // Devuelve el texto del badge según si fue verificada por un chef o es de la comunidad
    public string ObtenerVerificacion() =>
        verificadaPorChef
            ? $"✔ Verificada por Chef ({especialidadChef})"
            : "Receta de la Comunidad";


    // override devuelve el resumen polimórfico de la receta pública con su promedio de estrellas
    public override string ObtenerResumenEntidad() =>
        $"[Pública 🌐] {titulo} — {ObtenerVerificacion()} | ★ {ObtenerPromedioCalificaciones():0.0} ({calificaciones.Count} reseñas)";


    // Implementación de ICalificable: agrega una nueva calificación a la lista de reseñas
    public void AgregarCalificacion(Calificacion calificacion)
    {
        if (calificacion != null)
        {
            calificaciones.Insert(0, calificacion); // inserta al principio para ver la más reciente primero
        }
    }


    // Implementación de ICalificable: calcula el promedio aritmético de calificaciones redondeado a un decimal
    public double ObtenerPromedioCalificaciones()
    {
        if (calificaciones == null || calificaciones.Count == 0)
            return 0.0;
        return Math.Round(calificaciones.Average(c => c.Puntaje()), 1); // LINQ Average calcula el promedio
    }


    // Implementación de IFiltrable: evalúa si la receta cumple con los criterios del filtro
    public bool CumpleFiltro(FiltroReceta filtro)
    {
        if (filtro == null)
            return true;
        return filtro.CumpleCriterios(this);
    }

    // Propiedades de lectura para XAML
    [JsonIgnore]
    public string VerificacionBadgeTexto => ObtenerVerificacion();

    [JsonIgnore]
    public string PromedioEstrellasTexto =>
        calificaciones.Count > 0
            ? $"★ {ObtenerPromedioCalificaciones():0.0} ({calificaciones.Count})"
            : "☆ Sin calificar";

    [JsonIgnore]
    public bool EsDestacadaBind => destacadaEnFeed;
}
