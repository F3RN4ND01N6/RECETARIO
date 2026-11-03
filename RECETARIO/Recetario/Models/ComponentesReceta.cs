using System.Text.Json.Serialization;

namespace Recetario.Models;

/// <summary>
/// Paso secuencial dentro de la preparación de una receta (Composición 1..* en Receta).
/// </summary>
public class PasoPreparacion
{
    [JsonInclude]
    private int numeroPaso = 1;

    [JsonInclude]
    private string instruccion = string.Empty;

    [JsonInclude]
    private int tiempoEstimado = 5;

    [JsonInclude]
    private bool completado = false;

    public PasoPreparacion()
    {
    }

    public PasoPreparacion(int numeroPaso, string instruccion, int tiempoEstimado, bool completado = false)
    {
        this.numeroPaso = numeroPaso;
        this.instruccion = instruccion;
        this.tiempoEstimado = tiempoEstimado;
        this.completado = completado;
    }

    // Métodos exactos del Diagrama de Clases
    public int NumeroPaso() => numeroPaso;
    public string Instruccion() => instruccion;
    public int TiempoEstimado() => tiempoEstimado;
    public bool Completado() => completado;

    public void MarcarCompletado()
    {
        completado = !completado;
    }

    public string FormatearPaso() =>
        $"Paso {numeroPaso} ({this.tiempoEstimado} min): {instruccion}";

    // Propiedades de apoyo para enlace XAML
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
public class IngredienteTemporada
{
    [JsonInclude]
    private string nombreFrutaVerdura = string.Empty;

    [JsonInclude]
    private TipoVegetal tipo = TipoVegetal.VERDURA;

    [JsonInclude]
    private Estacion estacionPrincipal = Estacion.PRIMAVERA;

    [JsonInclude]
    private Origen paisOrigen = Origen.CHILE;

    [JsonInclude]
    private string zonaCultivo = string.Empty;

    public IngredienteTemporada()
    {
    }

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
    public string IconoVegetal => tipo == TipoVegetal.FRUTA ? "🍊" : "🍅";
}

/// <summary>
/// Ingrediente cuantificado que compone una receta .
/// </summary>
public class IngredienteReceta : IValidable
{
    [JsonInclude]
    private string nombre = string.Empty;

    [JsonInclude]
    private double cantidad = 1;

    [JsonInclude]
    private UnidadMedida unidad = UnidadMedida.UNIDADES;

    [JsonInclude]
    private Estacion estacion = Models.Estacion.TODOELANHIO;

    [JsonInclude]
    private bool esFrutaOVerduraEstacion = false;

    [JsonInclude]
    private TipoVegetal tipoVegetal = Models.TipoVegetal.NO_APLICA;

    [JsonInclude]
    private IngredienteTemporada? referenciaTemporada;

    public IngredienteReceta()
    {
    }

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





    public string FormatearIngrediente()
    {
        string sufijoVegetal = esFrutaOVerduraEstacion && tipoVegetal != Models.TipoVegetal.NO_APLICA
            ? $" [{tipoVegetal} de {estacion}]"
            : string.Empty;

        return unidad == UnidadMedida.ALGUSTO
            ? $"{nombre} (al gusto){sufijoVegetal}"
            : $"{cantidad:0.##} {unidad.ToString().ToLowerInvariant()} de {nombre}{sufijoVegetal}";
    }

    public bool EsValido() => ObtenerErroresValidacion().Count == 0;

    public List<string> ObtenerErroresValidacion()
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(nombre))
            errores.Add("El nombre del ingrediente no puede estar vacío.");
        if (unidad != UnidadMedida.ALGUSTO && cantidad <= 0)
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
public class Calificacion
{

    // PROPIEDADES XAML

    [JsonInclude]
    private string idCalificacion = Guid.NewGuid().ToString("N")[..8];

    [JsonInclude]
    private string idUsuario = string.Empty;

    [JsonInclude]
    private int puntaje = 5;

    [JsonInclude]
    private string comentario = string.Empty;

    [JsonInclude]
    private DateTime fecha = DateTime.Now;



    /// <summary>
    /// ELIMINACION OPORTUNA -> -> //
    /// </summary>
    public Calificacion()
    {
    }



    public Calificacion(string idUsuario, int puntaje, string comentario)
    {
        this.idCalificacion = Guid.NewGuid().ToString("N")[..8];
        this.idUsuario = idUsuario;
        this.puntaje = Math.Clamp(puntaje, 1, 5);
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
    public string EstrellasTexto => new string('★', Math.Clamp(puntaje, 1, 5)) + new string('☆', 5 - Math.Clamp(puntaje, 1, 5));

    [JsonIgnore]
    public string ComentarioBind => comentario;

    [JsonIgnore]
    public string AutorYFechaTexto => $"Por {idUsuario} • {fecha:dd/MM/yyyy}";
}






/// <summary>
/// Criterios de búsqueda y filtrado multicriterio de recetas (UC06).
/// </summary>
public class FiltroReceta
{
    [JsonInclude]
    private TipoVegetal? tipoFrutaOVerdura;

    [JsonInclude]
    private string nombreIngrediente = string.Empty;

    [JsonInclude]
    private Estacion? estacionTemporada;

    [JsonInclude]
    private Origen? pais;

    [JsonInclude]
    private string categoriaFiltro = string.Empty;

    [JsonInclude]
    private string dietaFiltro = string.Empty;

    [JsonInclude]
    private NivelDificultad? dificultad;

    [JsonInclude]
    private bool soloVerificadasPorChef;





    /// <summary>
    /// ELIMINACION OPORTUNA -> -> //
    /// </summary>
    public FiltroReceta()
    {
    }





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

    public bool CumpleCriterios(Receta receta)
    {
        if (receta == null)
            return false;

        if (!string.IsNullOrWhiteSpace(nombreIngrediente))
        {
            string termino = nombreIngrediente.Trim();
            bool coincideTitulo = receta.Titulo.Contains(termino, StringComparison.OrdinalIgnoreCase);
            bool coincideEtiqueta = receta.Etiquetas.Any(e => e.Contains(termino, StringComparison.OrdinalIgnoreCase));
            bool coincideIngrediente = receta.Ingredientes.Any(i => i.Nombre().Contains(termino, StringComparison.OrdinalIgnoreCase));

            if (!coincideTitulo && !coincideEtiqueta && !coincideIngrediente)
                return false;
        }

        if (tipoFrutaOVerdura.HasValue && tipoFrutaOVerdura.Value != TipoVegetal.NO_APLICA)
        {
            bool tieneTipo = receta.Ingredientes.Any(i => i.TipoVegetal() == tipoFrutaOVerdura.Value);
            if (!tieneTipo)
                return false;
        }

        if (estacionTemporada.HasValue && estacionTemporada.Value != Estacion.TODOELANHIO)
        {
            if (receta.Estacion != estacionTemporada.Value && receta.Estacion != Estacion.TODOELANHIO)
                return false;
        }

        if (pais.HasValue)
        {
            if (receta.Origen != pais.Value)
                return false;
        }

        if (!string.IsNullOrWhiteSpace(categoriaFiltro) && !categoriaFiltro.Equals("Todas", StringComparison.OrdinalIgnoreCase))
        {
            if (!receta.Categoria.Contains(categoriaFiltro.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(dietaFiltro) && !dietaFiltro.Equals("Todas", StringComparison.OrdinalIgnoreCase))
        {
            if (!receta.TipoDieta.Contains(dietaFiltro.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (dificultad.HasValue)
        {
            if (receta.Dificultad != dificultad.Value)
                return false;
        }

        if (soloVerificadasPorChef)
        {
            if (receta is not RecetaPublica pub || !pub.VerificadaPorChef())
                return false;
        }

        return true;
    }
}
