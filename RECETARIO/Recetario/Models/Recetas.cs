using System.Text.Json.Serialization;

namespace Recetario.Models;

/// <summary>
/// Clase base abstracta para todas las recetas del sistema (Herencia 4).
/// Implementa IIdentificable e IValidable (UC04).
/// </summary>
public abstract class Receta : IIdentificable, IValidable
{
    [JsonInclude]
    protected string idReceta = Guid.NewGuid().ToString("N")[..8];

    [JsonInclude]
    protected string idUsuarioAutor = string.Empty;

    [JsonInclude]
    protected string titulo = string.Empty;

    [JsonInclude]
    protected string categoria = "Plato Principal";

    [JsonInclude]
    protected string tipoDieta = "Omnívora";

    [JsonInclude]
    protected List<string> etiquetas = new();

    [JsonInclude]
    protected string imagenPlatillo = "platillo_default.jpg";

    [JsonInclude]
    protected int tiempoPreparacion = 30;

    [JsonInclude]
    protected int porciones = 4;

    [JsonInclude]
    protected NivelDificultad dificultad = NivelDificultad.FACIL;

    [JsonInclude]
    protected Estacion estacion = Estacion.TODOELANHIO;

    [JsonInclude]
    protected DateTime fechaPublicacion = DateTime.Now;

    [JsonInclude]
    protected Origen origen = Origen.CHILE;

    [JsonInclude]
    protected string region = "Zona Central";

    [JsonInclude]
    protected List<IngredienteReceta> ingredientes = new();

    [JsonInclude]
    protected List<PasoPreparacion> pasos = new();

    protected Receta()
    {
    }

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

    public static string NormalizarRutaImagen(string? valor, string? tituloReceta = null)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return "platillo_default.jpg";

        string limpio = valor.Trim();
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
    public string ObtenerIdentificador() => idReceta;

    public bool EsValido() => ObtenerErroresValidacion().Count == 0;

    /// <summary>
    /// Caso de Uso UC04: Validar Datos de la Receta (Composición 1..* de Ingredientes y Pasos).
    /// </summary>
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

    public abstract bool EsPublicaEnFeed();
    public abstract string ObtenerResumenEntidad();
}

/// <summary>
/// Receta privada del usuario almacenada en el respaldo JSON local (UC01, UC14, UC15).
/// </summary>
public class RecetaPrivada : Receta
{
    [JsonInclude]
    private string notasPersonales = string.Empty;

    [JsonInclude]
    private bool soloDispositivoLocal = true;

    public RecetaPrivada() : base()
    {
    }

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

    public override bool EsPublicaEnFeed() => false;

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
public class RecetaPublica : Receta, ICalificable, IFiltrable
{
    [JsonInclude]
    private string especialidadChef = "Cocina Tradicional de Estación";

    [JsonInclude]
    private bool verificadaPorChef = false;

    [JsonInclude]
    private bool destacadaEnFeed = false;

    [JsonInclude]
    private List<Calificacion> calificaciones = new();

    public RecetaPublica() : base()
    {
    }

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
        //DateTime fechaPublicacion,
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

    public override bool EsPublicaEnFeed() => true;

    public string ObtenerVerificacion() =>
        verificadaPorChef
            ? $"✔ Verificada por Chef ({especialidadChef})"
            : "Receta de la Comunidad";

    public override string ObtenerResumenEntidad() =>
        $"[Pública 🌐] {titulo} — {ObtenerVerificacion()} | ★ {ObtenerPromedioCalificaciones():0.0} ({calificaciones.Count} reseñas)";

    public void AgregarCalificacion(Calificacion calificacion)
    {
        if (calificacion != null)
        {
            calificaciones.Insert(0, calificacion);
        }
    }

    public double ObtenerPromedioCalificaciones()
    {
        if (calificaciones == null || calificaciones.Count == 0)
            return 0.0;
        return Math.Round(calificaciones.Average(c => c.Puntaje()), 1);
    }

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
