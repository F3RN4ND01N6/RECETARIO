using System.Text.Json.Serialization;

namespace Recetario.Models;

/// <summary>
/// Paso secuencial dentro de la preparación de una receta (Composición 1..* en Receta).
/// </summary>
// PasoPreparacion representa cada una de las instrucciones ordenadas necesarias para cocinar una receta.
public class PasoPreparacion
{

    // [JsonInclude] permite que los campos privados sean serializados y guardados en los archivos JSON.
    [JsonInclude]
    private int numeroPaso = 1; // orden correlativo del paso (1, 2, 3...)

    [JsonInclude]
    private string instruccion = string.Empty; // texto explicativo de lo que se debe hacer en este paso

    [JsonInclude]
    private int tiempoEstimado = 5; // duración estimada en minutos para completar este paso

    [JsonInclude]
    private bool completado = false; // estado interactivo que indica si el cocinero ya realizó el paso


    // Constructor vacío por defecto necesario para la deserialización JSON.
    public PasoPreparacion()
    {
    }


    // Constructor con parámetros para inicializar un nuevo paso de preparación.
    public PasoPreparacion(int numeroPaso, string instruccion, int tiempoEstimado, bool completado = false)
    {
        this.numeroPaso = numeroPaso; // asigna el número del paso
        this.instruccion = instruccion; // asigna el texto de la instrucción
        this.tiempoEstimado = tiempoEstimado; // asigna los minutos de duración
        this.completado = completado; // asigna si inicia completado o pendiente
    }

    // Métodos exactos del Diagrama de Clases
    public int NumeroPaso() => numeroPaso; // devuelve el número de paso
    public string Instruccion() => instruccion; // devuelve la instrucción
    public int TiempoEstimado() => tiempoEstimado; // devuelve el tiempo estimado
    public bool Completado() => completado; // devuelve el estado de completado


    // Alterna el valor booleano de completado al presionar el botón del paso en la vista DetalleRecetaPage.
    public void MarcarCompletado()
    {
        completado = !completado; // niega el valor actual (si era false pasa a true y viceversa)
    }


    // Formatea el texto del paso para mostrarlo en listas y vistas de detalle.
    public string FormatearPaso() =>
        $"Paso {numeroPaso} ({this.tiempoEstimado} min): {instruccion}";

    // Propiedades de apoyo para enlace XAML
    // [JsonIgnore] omite estas propiedades durante la serialización para no duplicar datos en el JSON.
    [JsonIgnore]
    public int NumeroPasoBind
    {
        get => numeroPaso;
        set => numeroPaso = value;
    }

    [JsonIgnore]
    public string InstruccionBind
    {
        get => instruccion;
        set => instruccion = value;
    }

    [JsonIgnore]
    public int TiempoEstimadoBind
    {
        get => this.tiempoEstimado;
        set => this.tiempoEstimado = value;
    }

    [JsonIgnore]
    public bool CompletadoBind
    {
        get => completado;
        set => completado = value;
    }

    [JsonIgnore]
    public string TextoFormateado => FormatearPaso();
}

/// <summary>
/// Ingrediente o cultivo de estación del Calendario de Temporada (UC07).
/// </summary>
// IngredienteTemporada representa una fruta o verdura en el catálogo estacional con su zona de cultivo.
public class IngredienteTemporada
{
    [JsonInclude]
    private string nombreFrutaVerdura = string.Empty; // nombre del fruto o vegetal

    [JsonInclude]
    private TipoVegetal tipo = TipoVegetal.VERDURA; // si es FRUTA, VERDURA u OTRO

    [JsonInclude]
    private Estacion estacionPrincipal = Estacion.PRIMAVERA; // estación en la que se cosecha naturalmente

    [JsonInclude]
    private Origen paisOrigen = Origen.CHILE; // país de producción principal

    [JsonInclude]
    private string zonaCultivo = string.Empty; // valle, región o localidad de cultivo


    // Constructor vacío por defecto para la serialización JSON.
    public IngredienteTemporada()
    {
    }


    // Constructor con parámetros para inicializar el vegetal o fruta de temporada.
    public IngredienteTemporada(string nombreFrutaVerdura, TipoVegetal tipo, Estacion estacionPrincipal, Origen paisOrigen, string zonaCultivo)
    {
        this.nombreFrutaVerdura = nombreFrutaVerdura;
        this.tipo = tipo;
        this.estacionPrincipal = estacionPrincipal;
        this.paisOrigen = paisOrigen;
        this.zonaCultivo = zonaCultivo;
    }

    // Métodos exactos del Diagrama de Clases
    public string NombreFrutaVerdura() => nombreFrutaVerdura;
    public TipoVegetal Tipo() => tipo;
    public Estacion EstacionPrincipal() => estacionPrincipal;
    public Origen PaisOrigen() => paisOrigen;
    public string ZonaCultivo() => zonaCultivo;

    // Propiedades de lectura para XAML
    [JsonIgnore]
    public string NombreBind => nombreFrutaVerdura;

    [JsonIgnore]
    public string TipoTexto => tipo == TipoVegetal.FRUTA ? "Fruta de Estación" : "Verdura / Hortaliza";

    [JsonIgnore]
    public string EstacionTexto => $"Estación: {estacionPrincipal}";

    [JsonIgnore]
    public string OrigenZonaTexto => $"{paisOrigen} • {zonaCultivo}";

    [JsonIgnore]
    public string IconoVegetal => tipo == TipoVegetal.FRUTA ? "🍊" : "🍅"; // emoji o icono representativo
}

/// <summary>
/// Ingrediente cuantificado que compone una receta .
/// </summary>
// IngredienteReceta representa un ingrediente específico dentro de una receta con cantidad y unidad de medida.
public class IngredienteReceta : IValidable
{
    [JsonInclude]
    private string nombre = string.Empty; // nombre del ingrediente (ej: Harina, Choclo)

    [JsonInclude]
    private double cantidad = 1; // cantidad numérica requerida

    [JsonInclude]
    private UnidadMedida unidad = UnidadMedida.UNIDADES; // gramos, tazas, unidades, etc.

    [JsonInclude]
    private Estacion estacion = Models.Estacion.TODOELANHIO; // temporada a la que pertenece el ingrediente

    [JsonInclude]
    private bool esFrutaOVerduraEstacion = false; // indica si es un producto de temporada verificado

    [JsonInclude]
    private TipoVegetal tipoVegetal = Models.TipoVegetal.NO_APLICA; // clasificación botánica

    [JsonInclude]
    private IngredienteTemporada? referenciaTemporada; // enlace opcional al catálogo de temporada


    // Constructor vacío por defecto para la serialización JSON.
    public IngredienteReceta()
    {
    }


    // Constructor con parámetros para construir el ingrediente de la receta.
    public IngredienteReceta(
        string nombre,
        double cantidad,
        UnidadMedida unidad,
        Estacion estacion = Models.Estacion.TODOELANHIO,
        bool esFrutaOVerduraEstacion = false,
        TipoVegetal tipoVegetal = Models.TipoVegetal.NO_APLICA,
        IngredienteTemporada? referenciaTemporada = null)
    {
        this.nombre = nombre;
        this.cantidad = cantidad;
        this.unidad = unidad;
        this.estacion = estacion;
        this.esFrutaOVerduraEstacion = esFrutaOVerduraEstacion;
        this.tipoVegetal = tipoVegetal;
        this.referenciaTemporada = referenciaTemporada;
    }

    // Métodos exactos del Diagrama de Clases
    public string Nombre() => nombre;
    public double Cantidad() => cantidad;
    public UnidadMedida Unidad() => unidad;
    public Estacion Estacion() => estacion;
    public bool EsFrutaOVerduraEstacion() => esFrutaOVerduraEstacion;
    public TipoVegetal TipoVegetal() => tipoVegetal;
    public IngredienteTemporada? ReferenciaTemporada() => referenciaTemporada;


    // Formatea el ingrediente mostrando su cantidad, unidad de medida y sufijo de temporada si aplica.
    public string FormatearIngrediente()
    {
        string sufijoVegetal = esFrutaOVerduraEstacion && tipoVegetal != Models.TipoVegetal.NO_APLICA
            ? $" [{tipoVegetal} de {estacion}]"
            : string.Empty;

        return unidad == UnidadMedida.ALGUSTO
            ? $"{nombre} (al gusto){sufijoVegetal}" // formato sin cantidad numérica fija
            : $"{cantidad:0.##} {unidad.ToString().ToLowerInvariant()} de {nombre}{sufijoVegetal}";
    }


    // Implementación de IValidable: es válido si no posee errores de validación.
    public bool EsValido() => ObtenerErroresValidacion().Count == 0;


    // Devuelve los errores encontrados en los campos del ingrediente.
    public List<string> ObtenerErroresValidacion()
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(nombre)) // valida que el nombre no esté vacío
            errores.Add("El nombre del ingrediente no puede estar vacío.");
        if (unidad != UnidadMedida.ALGUSTO && cantidad <= 0) // valida que la cantidad sea positiva
            errores.Add($"La cantidad de '{nombre}' debe ser mayor que 0.");
        return errores;
    }

    // ELIMINACION OPORTUNA -> -> //
    [JsonIgnore]
    public string TextoFormateado => FormatearIngrediente();
}

/// <summary>
/// RECEÑA PUNTAJE DE RECETAS (UC09).
/// </summary>
// Calificacion representa una valoración numérica con comentario dejada por un usuario a una receta pública.
public class Calificacion
{

    // PROPIEDADES XAML
    [JsonInclude]
    private string idCalificacion = Guid.NewGuid().ToString("N")[..8]; // identificador único de la calificación

    [JsonInclude]
    private string idUsuario = string.Empty; // nombre o identificador del usuario que calificó

    [JsonInclude]
    private int puntaje = 5; // cantidad de estrellas de 1 a 5

    [JsonInclude]
    private string comentario = string.Empty; // texto de la reseña

    [JsonInclude]
    private DateTime fecha = DateTime.Now; // fecha y hora en la que se publicó la reseña

    /// <summary>
    /// ELIMINACION OPORTUNA -> -> //
    /// </summary>
    public Calificacion()
    {
    }

    // Constructor con parámetros para inicializar una nueva reseña o calificación.
    public Calificacion(string idUsuario, int puntaje, string comentario)
    {
        this.idCalificacion = Guid.NewGuid().ToString("N")[..8];
        this.idUsuario = idUsuario;
        this.puntaje = Math.Clamp(puntaje, 1, 5); // Math.Clamp asegura que el puntaje siempre esté entre 1 y 5
        this.comentario = comentario;
        this.fecha = DateTime.Now;
    }

    // Métodos exactos del Diagrama de Clases
    public string IdCalificacion() => idCalificacion;
    public string IdUsuario() => idUsuario;
    public int Puntaje() => puntaje;
    public string Comentario() => comentario;
    public DateTime Fecha() => fecha;

    // Propiedades de lectura para XAML
    [JsonIgnore]
    // Genera una cadena con estrellas rellenas y vacías según el puntaje (ej: ★★★★☆)
    public string EstrellasTexto => new string('★', Math.Clamp(puntaje, 1, 5)) + new string('☆', 5 - Math.Clamp(puntaje, 1, 5));

    [JsonIgnore]
    public string ComentarioBind => comentario;

    [JsonIgnore]
    public string AutorYFechaTexto => $"Por {idUsuario} • {fecha:dd/MM/yyyy}";
}

/// <summary>
/// Criterios de búsqueda y filtrado multicriterio de recetas (UC06).
/// </summary>
// FiltroReceta encapsula los parámetros de búsqueda para filtrar el feed público o el recetario privado.
public class FiltroReceta
{
    [JsonInclude]
    private TipoVegetal? tipoFrutaOVerdura; // filtro por fruta o verdura

    [JsonInclude]
    private string nombreIngrediente = string.Empty; // texto de búsqueda por ingrediente o título

    [JsonInclude]
    private Estacion? estacionTemporada; // filtro por estación

    [JsonInclude]
    private Origen? pais; // filtro por país de origen

    [JsonInclude]
    private string categoriaFiltro = string.Empty; // filtro por categoría culinaria

    [JsonInclude]
    private string dietaFiltro = string.Empty; // filtro por tipo de dieta

    [JsonInclude]
    private NivelDificultad? dificultad; // filtro por nivel de dificultad

    [JsonInclude]
    private bool soloVerificadasPorChef; // filtro para ver únicamente recetas certificadas por chefs

    /// <summary>
    /// ELIMINACION OPORTUNA -> -> //
    /// </summary>
    public FiltroReceta()
    {
    }

    // Constructor con parámetros opcionales para configurar los criterios deseados de búsqueda.
    public FiltroReceta(
        TipoVegetal? tipoFrutaOVerdura = null,
        string nombreIngrediente = "",
        Estacion? estacionTemporada = null,
        Origen? pais = null,
        string categoriaFiltro = "",
        string dietaFiltro = "",
        NivelDificultad? dificultad = null,
        bool soloVerificadasPorChef = false)
    {
        this.tipoFrutaOVerdura = tipoFrutaOVerdura;
        this.nombreIngrediente = nombreIngrediente ?? string.Empty;
        this.estacionTemporada = estacionTemporada;
        this.pais = pais;
        this.categoriaFiltro = categoriaFiltro ?? string.Empty;
        this.dietaFiltro = dietaFiltro ?? string.Empty;
        this.dificultad = dificultad;
        this.soloVerificadasPorChef = soloVerificadasPorChef;
    }

    // Métodos exactos del Diagrama de Clases
    public TipoVegetal? TipoFrutaOVerdura() => tipoFrutaOVerdura;
    public string CategoriaFiltro() => categoriaFiltro;
    public string NombreIngrediente() => nombreIngrediente;
    public Estacion? EstacionTemporada() => estacionTemporada;
    public Origen? Pais() => pais;
    public string DietaFiltro() => dietaFiltro;
    public NivelDificultad? Dificultad() => dificultad;
    public bool SoloVerificadasPorChef() => soloVerificadasPorChef;

    /// <summary>
    /// RESTRICCIONES PARA LA CREACION DE RECETAS (EXCEPCIONES)
    /// </summary>
    /// <param name="receta"></param>
    /// <returns></returns>
    // Evalúa si una receta cumple con todos los filtros activos configurados.
    public bool CumpleCriterios(Receta receta)
    {
        if (receta == null)
            return false;

        // Comprueba si el texto coincide en el título, etiquetas o ingredientes de la receta
        if (!string.IsNullOrWhiteSpace(nombreIngrediente))
        {
            string termino = nombreIngrediente.Trim();
            bool coincideTitulo = receta.Titulo.Contains(termino, StringComparison.OrdinalIgnoreCase);
            bool coincideEtiqueta = receta.Etiquetas.Any(e => e.Contains(termino, StringComparison.OrdinalIgnoreCase));
            bool coincideIngrediente = receta.Ingredientes.Any(i => i.Nombre().Contains(termino, StringComparison.OrdinalIgnoreCase));

            if (!coincideTitulo && !coincideEtiqueta && !coincideIngrediente)
                return false;
        }

        // Comprueba si contiene el tipo vegetal solicitado
        if (tipoFrutaOVerdura.HasValue && tipoFrutaOVerdura.Value != TipoVegetal.NO_APLICA)
        {
            bool tieneTipo = receta.Ingredientes.Any(i => i.TipoVegetal() == tipoFrutaOVerdura.Value);
            if (!tieneTipo)
                return false;
        }

        // Comprueba si coincide con la estación
        if (estacionTemporada.HasValue && estacionTemporada.Value != Estacion.TODOELANHIO)
        {
            if (receta.Estacion != estacionTemporada.Value && receta.Estacion != Estacion.TODOELANHIO)
                return false;
        }

        // Comprueba si coincide con el país de origen
        if (pais.HasValue)
        {
            if (receta.Origen != pais.Value)
                return false;
        }

        // Comprueba si coincide con la categoría
        if (!string.IsNullOrWhiteSpace(categoriaFiltro) && !categoriaFiltro.Equals("Todas", StringComparison.OrdinalIgnoreCase))
        {
            if (!receta.Categoria.Contains(categoriaFiltro.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Comprueba si coincide con la dieta
        if (!string.IsNullOrWhiteSpace(dietaFiltro) && !dietaFiltro.Equals("Todas", StringComparison.OrdinalIgnoreCase))
        {
            if (!receta.TipoDieta.Contains(dietaFiltro.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Comprueba si coincide con la dificultad
        if (dificultad.HasValue)
        {
            if (receta.Dificultad != dificultad.Value)
                return false;
        }

        // Comprueba si se solicitaron únicamente recetas certificadas por chefs
        if (soloVerificadasPorChef)
        {
            if (receta is not RecetaPublica pub || !pub.VerificadaPorChef())
                return false;
        }

        return true; // cumple con todos los filtros
    }
}
