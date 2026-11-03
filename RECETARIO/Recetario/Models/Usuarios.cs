using System.Text.Json.Serialization;

namespace Recetario.Models;

/// <summary>
/// Configuración de preferencias de temporada y origen del usuario (Agregación 1 con Usuario).
/// </summary>
public class ConfiguracionPerfil
{
    [JsonInclude]
    private Estacion estacionPreferida = Estacion.PRIMAVERA;

    [JsonInclude]
    private Origen origen = Models.Origen.CHILE;

    [JsonInclude]
    private string regionPreferida = "Zona Central";

    [JsonInclude]
    private bool ingredientesEstacion = true;

    public ConfiguracionPerfil()
    {
    }

    public ConfiguracionPerfil(Estacion estacionPreferida, Origen origen, string regionPreferida, bool ingredientesEstacion)
    {
        this.estacionPreferida = estacionPreferida;
        this.origen = origen;
        this.regionPreferida = regionPreferida;
        this.ingredientesEstacion = ingredientesEstacion;
    }

    // Métodos exactos del Diagrama de Clases
    public Estacion EstacionPreferida() => estacionPreferida;
    public Origen Origen() => origen;
    public string RegionPreferida() => regionPreferida;
    public bool IngredientesEstacion() => ingredientesEstacion;

    // Setters / Propiedades auxiliares para enlace de datos en la interfaz MAUI
    public void ActualizarConfiguracion(Estacion estacion, Origen paisOrigen, string region, bool priorizarEstacion)
    {
        estacionPreferida = estacion;
        origen = paisOrigen;
        regionPreferida = string.IsNullOrWhiteSpace(region) ? "General" : region.Trim();
        ingredientesEstacion = priorizarEstacion;
    }
}

/// <summary>
/// Clase base abstracta para todos los actores del sistema (Herencias 1, 2 y 3).
/// Implementa IIdentificable e IValidable.
/// </summary>
public abstract class Usuario : IIdentificable, IValidable
{
    [JsonInclude]
    protected string idUsuario = Guid.NewGuid().ToString("N")[..8];

    [JsonInclude]
    protected string nombreUsuario = string.Empty;

    [JsonInclude]
    protected string correo = string.Empty;

    [JsonInclude]
    protected string password = string.Empty;

    [JsonInclude]
    protected string biografia = string.Empty;

    [JsonInclude]
    protected DateTime fechaRegistro = DateTime.Now;

    [JsonInclude]
    protected TipoPerfilUsuario tipoPerfil = TipoPerfilUsuario.USUARIO_NORMAL;

    [JsonInclude]
    protected ConfiguracionPerfil configuracionPerfil = new();

    protected Usuario()
    {
    }

    protected Usuario(string idUsuario, string nombreUsuario, string correo, string biografia, TipoPerfilUsuario tipoPerfil, ConfiguracionPerfil? configuracion = null, string password = "")
    {
        this.idUsuario = idUsuario;
        this.nombreUsuario = nombreUsuario;
        this.correo = correo;
        this.password = password;
        this.biografia = biografia;
        this.fechaRegistro = DateTime.Now;
        this.tipoPerfil = tipoPerfil;
        this.configuracionPerfil = configuracion ?? new ConfiguracionPerfil();
    }

    // Métodos exactos del Diagrama de Clases
    public string IdUsuario() => idUsuario;
    public string NombreUsuario() => nombreUsuario;
    public string Correo() => correo;
    public string Password() => password;
    public void EstablecerPassword(string nuevaPassword) => password = nuevaPassword;
    public bool ValidarPassword(string passwordAValidar) => !string.IsNullOrEmpty(password) && password == passwordAValidar;
    public string Biografia() => biografia;
    public DateTime FechaRegistro() => fechaRegistro;
    public ConfiguracionPerfil ConfiguracionPerfil() => configuracionPerfil;
    public TipoPerfilUsuario ObtenerTipoPerfil() => tipoPerfil;

    public void ActualizarDatosBasicos(string nuevoNombre, string nuevoCorreo, string nuevaBiografia)
    {
        nombreUsuario = nuevoNombre.Trim();
        correo = nuevoCorreo.Trim();
        biografia = nuevaBiografia.Trim();
    }

    public abstract void EjecutarAccionRol();

    public string ObtenerIdentificador() => idUsuario;

    public virtual string ObtenerResumenEntidad() =>
        $"{nombreUsuario} ({tipoPerfil}) - Origen: {configuracionPerfil.Origen()} ({configuracionPerfil.RegionPreferida()})";

    public bool EsValido() => ObtenerErroresValidacion().Count == 0;

    public List<string> ObtenerErroresValidacion()
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(idUsuario))
            errores.Add("El identificador de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombreUsuario) || nombreUsuario.Trim().Length < 3)
            errores.Add("El nombre de usuario debe tener al menos 3 caracteres.");
        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
            errores.Add("El correo electrónico no tiene un formato válido.");
        return errores;
    }

    [JsonIgnore]
    public string UltimaAccionRolMensaje { get; protected set; } = string.Empty;
}

/// <summary>
/// Usuario o Cocinero estándar con gestión de recetas favoritas (UC08).
/// </summary>
public class UsuarioNormal : Usuario
{
    [JsonInclude]
    private List<string> idsRecetasFavoritas = new();

    public UsuarioNormal() : base()
    {
        tipoPerfil = TipoPerfilUsuario.USUARIO_NORMAL;
    }

    public UsuarioNormal(string idUsuario, string nombreUsuario, string correo, string biografia, ConfiguracionPerfil? configuracion = null, string password = "")
        : base(idUsuario, nombreUsuario, correo, biografia, TipoPerfilUsuario.USUARIO_NORMAL, configuracion, password)
    {
    }

    public IReadOnlyList<string> ObtenerIdsFavoritas() => idsRecetasFavoritas;

    public bool EsFavorita(string idReceta) => idsRecetasFavoritas.Contains(idReceta);

    public override void EjecutarAccionRol()
    {
        UltimaAccionRolMensaje = $"Cocinero '{nombreUsuario}': explorando recetario de temporada ({configuracionPerfil.EstacionPreferida()}) con {idsRecetasFavoritas.Count} recetas favoritas guardadas.";
    }

    public void MarcarFavorita(string idReceta)
    {
        if (!string.IsNullOrWhiteSpace(idReceta) && !idsRecetasFavoritas.Contains(idReceta))
        {
            idsRecetasFavoritas.Add(idReceta);
        }
    }

    public void DesmarcarFavorita(string idReceta)
    {
        idsRecetasFavoritas.Remove(idReceta);
    }
}

/// <summary>
/// Creador Especialista (Chef / Youtuber / Nutricionista) capaz de publicar recetas verificadas en el Feed Global (UC16).
/// </summary>
public class CreadorEspecialista : Usuario
{
    [JsonInclude]
    private string credencialesChef = "Chef Gastronómico & Especialista en Cocina de Temporada";

    public CreadorEspecialista() : base()
    {
        tipoPerfil = TipoPerfilUsuario.CHEF_VERIFICADO;
    }

    public CreadorEspecialista(string idUsuario, string nombreUsuario, string correo, string biografia, string credencialesChef, ConfiguracionPerfil? configuracion = null)
        : base(idUsuario, nombreUsuario, correo, biografia, TipoPerfilUsuario.CHEF_VERIFICADO, configuracion)
    {
        this.credencialesChef = credencialesChef;
    }

    public string CredencialesChef() => credencialesChef;

    public void ActualizarCredenciales(string nuevasCredenciales) => credencialesChef = nuevasCredenciales.Trim();

    public override void EjecutarAccionRol()
    {
        UltimaAccionRolMensaje = $"Chef Verificado '{nombreUsuario}' ({credencialesChef}): habilitado para certificar y publicar recetas oficiales en el Feed Global (UC16).";
    }

    /// <summary>
    /// Convierte y certifica una RecetaPrivada como RecetaPublica oficial verificada para el Feed Global.
    /// </summary>
    public RecetaPublica PublicarRecetaOficial(RecetaPrivada privada)
    {
        var publica = new RecetaPublica(
            idReceta: string.IsNullOrWhiteSpace(privada.IdReceta) ? Guid.NewGuid().ToString("N")[..8] : privada.IdReceta,
            idUsuarioAutor: idUsuario,
            titulo: privada.Titulo,
            categoria: privada.Categoria,
            tipoDieta: privada.TipoDieta,
            etiquetas: new List<string>(privada.Etiquetas),
            imagenPlatillo: privada.ImagenPlatillo,
            tiempoPreparacion: privada.TiempoPreparacion,
            porciones: privada.Porciones,
            dificultad: privada.Dificultad,
            estacion: privada.Estacion,
            origen: privada.Origen,
            region: privada.Region,
            ingredientes: new List<IngredienteReceta>(privada.Ingredientes),
            pasos: new List<PasoPreparacion>(privada.Pasos),
            especialidadChef: credencialesChef,
            verificadaPorChef: true,
            destacadaEnFeed: true
        );

        EjecutarAccionRol();
        return publica;
    }
}

/// <summary>
/// Administrador de la plataforma con permisos de moderación y gestión del catálogo global (UC02, UC17).
/// </summary>
public class Administrador : Usuario
{
    [JsonInclude]
    private string nivelAcceso = "SUPER_ADMIN_MODERADOR";

    public Administrador() : base()
    {
        tipoPerfil = TipoPerfilUsuario.ADMINISTRADOR;
    }

    public Administrador(string idUsuario, string nombreUsuario, string correo, string biografia, string nivelAcceso, ConfiguracionPerfil? configuracion = null, string password = "")
        : base(idUsuario, nombreUsuario, correo, biografia, TipoPerfilUsuario.ADMINISTRADOR, configuracion, password)
    {
        this.nivelAcceso = nivelAcceso;
    }

    public string NivelAcceso() => nivelAcceso;

    public override void EjecutarAccionRol()
    {
        UltimaAccionRolMensaje = $"Administrador '{nombreUsuario}' [Nivel: {nivelAcceso}]: auditoría activa sobre el Feed Público y sincronización JSON Global (UC02 / UC17).";
    }

    public void ForzarEliminacionReceta(string idReceta)
    {
        UltimaAccionRolMensaje = $"Administrador '{nombreUsuario}' ejecutó eliminación forzada de la receta ID '{idReceta}'.";
    }
}
