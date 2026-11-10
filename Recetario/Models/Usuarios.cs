using System.Text.Json.Serialization;

namespace Recetario.Models;

/// <summary>
/// Configuración de preferencias de temporada y origen del usuario (Agregación 1 con Usuario).
/// </summary>
// ConfiguracionPerfil almacena los gustos culinarios del usuario: estación preferida, país de origen y si prefiere ingredientes estacionales.
public class ConfiguracionPerfil
{
    [JsonInclude]
    private Estacion estacionPreferida = Estacion.PRIMAVERA; // estación favorita para filtrar recetas

    [JsonInclude]
    private Origen origen = Models.Origen.CHILE; // país de origen preferido

    [JsonInclude]
    private string regionPreferida = "Zona Central"; // región, valle o departamento

    [JsonInclude]
    private bool ingredientesEstacion = true; // si prefiere priorizar vegetales de temporada


    // Constructor vacío por defecto necesario para la deserialización desde JSON.
    public ConfiguracionPerfil()
    {
    }


    // Constructor con parámetros para inicializar las preferencias del perfil.
    public ConfiguracionPerfil(Estacion estacionPreferida, Origen origen, string regionPreferida, bool ingredientesEstacion)
    {
        this.estacionPreferida = estacionPreferida;
        this.origen = origen;
        this.regionPreferida = regionPreferida;
        this.ingredientesEstacion = ingredientesEstacion;
    }

    // Métodos exactos del Diagrama de Clases
    public Estacion EstacionPreferida() => estacionPreferida; // devuelve la estación preferida
    public Origen Origen() => origen; // devuelve el país de origen
    public string RegionPreferida() => regionPreferida; // devuelve la región
    public bool IngredientesEstacion() => ingredientesEstacion; // devuelve si prioriza temporada


    // Actualiza los valores de configuración cuando el usuario modifica su perfil en PerfilPage.
    public void ActualizarConfiguracion(Estacion estacion, Origen paisOrigen, string region, bool priorizarEstacion)
    {
        estacionPreferida = estacion;
        origen = paisOrigen;
        regionPreferida = string.IsNullOrWhiteSpace(region) ? "General" : region.Trim(); // Trim() limpia espacios y asigna "General" si es nulo
        ingredientesEstacion = priorizarEstacion;
    }
}

/// <summary>
/// Clase base abstracta para todos los actores del sistema (Herencias 1, 2 y 3).
/// Implementa IIdentificable e IValidable.
/// </summary>
// abstract significa que esta clase no se puede instanciar directamente, sino que sirve como plantilla base para UsuarioNormal, CreadorEspecialista y Administrador.
public abstract class Usuario : IIdentificable, IValidable
{
    // protected significa que estos atributos son accesibles dentro de esta clase y en todas sus clases derivadas.
    [JsonInclude]
    protected string idUsuario = Guid.NewGuid().ToString("N")[..8]; // clave primaria única del usuario

    [JsonInclude]
    protected string nombreUsuario = string.Empty; // nombre visible del usuario

    [JsonInclude]
    protected string correo = string.Empty; // correo electrónico para autenticación

    [JsonInclude]
    protected string password = string.Empty; // contraseña en texto plano para validación local

    [JsonInclude]
    protected string biografia = string.Empty; // descripción o reseña personal del perfil

    [JsonInclude]
    protected DateTime fechaRegistro = DateTime.Now; // fecha en que se registró la cuenta

    [JsonInclude]
    protected TipoPerfilUsuario tipoPerfil = TipoPerfilUsuario.USUARIO_NORMAL; // rol actual asignado

    [JsonInclude]
    protected ConfiguracionPerfil configuracionPerfil = new(); // agregación con las preferencias de perfil


    // Constructor protegido vacío para serialización JSON.
    protected Usuario()
    {
    }


    // Constructor protegido con parámetros para inicializar los atributos de la clase base.
    protected Usuario(string idUsuario, string nombreUsuario, string correo, string biografia, TipoPerfilUsuario tipoPerfil, ConfiguracionPerfil? configuracion = null, string password = "")
    {
        this.idUsuario = idUsuario;
        this.nombreUsuario = nombreUsuario;
        this.correo = correo;
        this.password = password;
        this.biografia = biografia;
        this.fechaRegistro = DateTime.Now;
        this.tipoPerfil = tipoPerfil;
        this.configuracionPerfil = configuracion ?? new ConfiguracionPerfil(); // si no se pasa configuración crea una por defecto con ??
    }

    // Métodos exactos del Diagrama de Clases
    public string IdUsuario() => idUsuario;
    public string NombreUsuario() => nombreUsuario;
    public string Correo() => correo;
    public string Password() => password;
    public void EstablecerPassword(string nuevaPassword) => password = nuevaPassword; // actualiza contraseña
    public bool ValidarPassword(string passwordAValidar) => !string.IsNullOrEmpty(password) && password == passwordAValidar; // valida coincidencia
    public string Biografia() => biografia;
    public DateTime FechaRegistro() => fechaRegistro;
    public ConfiguracionPerfil ConfiguracionPerfil() => configuracionPerfil;
    public TipoPerfilUsuario ObtenerTipoPerfil() => tipoPerfil;


    // Actualiza el nombre, correo y biografía ingresados en el formulario de perfil.
    public void ActualizarDatosBasicos(string nuevoNombre, string nuevoCorreo, string nuevaBiografia)
    {
        nombreUsuario = nuevoNombre.Trim();
        correo = nuevoCorreo.Trim();
        biografia = nuevaBiografia.Trim();
    }


    // abstract método que no tiene cuerpo aquí; cada clase hija debe obligatoriamente implementarlo según su rol (Polimorfismo).
    public abstract void EjecutarAccionRol();

    // Implementación de IIdentificable: devuelve el identificador de usuario.
    public string ObtenerIdentificador() => idUsuario;


    // virtual método que puede ser sobrescrito (override) por las clases hijas si desean personalizar el resumen.
    public virtual string ObtenerResumenEntidad() =>
        $"{nombreUsuario} ({tipoPerfil}) - Origen: {configuracionPerfil.Origen()} ({configuracionPerfil.RegionPreferida()})";


    // Implementación de IValidable: valida que no existan errores de validación en los datos del usuario.
    public bool EsValido() => ObtenerErroresValidacion().Count == 0;


    // Comprueba las reglas de negocio del usuario: ID obligatorio, nombre mínimo de 3 letras y correo con arroba.
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
    public string UltimaAccionRolMensaje { get; protected set; } = string.Empty; // mensaje descriptivo de la última acción del rol
}

/// <summary>
/// Usuario o Cocinero estándar con gestión de recetas favoritas (UC08).
/// </summary>
// UsuarioNormal hereda de Usuario y representa a los cocineros aficionados con lista de recetas favoritas.
public class UsuarioNormal : Usuario
{
    [JsonInclude]
    private List<string> idsRecetasFavoritas = new(); // lista de IDs de recetas guardadas como favoritas


    // Constructor por defecto, llama al constructor base y fija el rol en USUARIO_NORMAL.
    public UsuarioNormal() : base()
    {
        tipoPerfil = TipoPerfilUsuario.USUARIO_NORMAL;
    }


    // Constructor con parámetros que invoca al constructor de la clase base con base(...).
    public UsuarioNormal(string idUsuario, string nombreUsuario, string correo, string biografia, ConfiguracionPerfil? configuracion = null, string password = "")
        : base(idUsuario, nombreUsuario, correo, biografia, TipoPerfilUsuario.USUARIO_NORMAL, configuracion, password)
    {
    }


    // Devuelve una colección de solo lectura con los identificadores de recetas favoritas.
    public IReadOnlyList<string> ObtenerIdsFavoritas() => idsRecetasFavoritas;


    // Comprueba si una receta específica está en la lista de favoritas.
    public bool EsFavorita(string idReceta) => idsRecetasFavoritas.Contains(idReceta);


    // override implementa la acción polimórfica específica de un cocinero normal explorando recetas.
    public override void EjecutarAccionRol()
    {
        UltimaAccionRolMensaje = $"Cocinero '{nombreUsuario}': explorando recetario de temporada ({configuracionPerfil.EstacionPreferida()}) con {idsRecetasFavoritas.Count} recetas favoritas guardadas.";
    }


    // Agrega el ID de una receta a la lista de favoritas si no estaba agregada previamente.
    public void MarcarFavorita(string idReceta)
    {
        if (!string.IsNullOrWhiteSpace(idReceta) && !idsRecetasFavoritas.Contains(idReceta))
        {
            idsRecetasFavoritas.Add(idReceta);
        }
    }


    // Quita el ID de una receta de la lista de favoritas.
    public void DesmarcarFavorita(string idReceta)
    {
        idsRecetasFavoritas.Remove(idReceta);
    }
}

/// <summary>
/// Creador Especialista (Chef / Youtuber / Nutricionista) capaz de publicar recetas verificadas en el Feed Global (UC16).
/// </summary>
// CreadorEspecialista hereda de Usuario y representa a un Chef profesional facultado para certificar recetas oficiales.
public class CreadorEspecialista : Usuario
{
    [JsonInclude]
    private string credencialesChef = "Chef Gastronómico & Especialista en Cocina de Temporada"; // título o certificación profesional


    // Constructor por defecto fijando el rol en CHEF_VERIFICADO.
    public CreadorEspecialista() : base()
    {
        tipoPerfil = TipoPerfilUsuario.CHEF_VERIFICADO;
    }


    // Constructor con parámetros que inicializa credenciales y llama al constructor base.
    public CreadorEspecialista(string idUsuario, string nombreUsuario, string correo, string biografia, string credencialesChef, ConfiguracionPerfil? configuracion = null)
        : base(idUsuario, nombreUsuario, correo, biografia, TipoPerfilUsuario.CHEF_VERIFICADO, configuracion)
    {
        this.credencialesChef = credencialesChef;
    }

    public string CredencialesChef() => credencialesChef;
    public void ActualizarCredenciales(string nuevasCredenciales) => credencialesChef = nuevasCredenciales.Trim();


    // override implementa la acción polimórfica indicando los permisos de certificación de recetas oficiales.
    public override void EjecutarAccionRol()
    {
        UltimaAccionRolMensaje = $"Chef Verificado '{nombreUsuario}' ({credencialesChef}): habilitado para certificar y publicar recetas oficiales en el Feed Global (UC16).";
    }

    /// <summary>
    /// Convierte y certifica una RecetaPrivada como RecetaPublica oficial verificada para el Feed Global.
    /// </summary>
    // Transforma una receta privada en pública, certificándola con su especialidad y sello de verificación.
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
            ingredientes: new List<IngredienteReceta>(privada.Ingredientes), // clona ingredientes
            pasos: new List<PasoPreparacion>(privada.Pasos), // clona pasos
            especialidadChef: credencialesChef, // certifica con las credenciales del chef
            verificadaPorChef: true, // otorga el sello de verificación oficial
            destacadaEnFeed: true // resalta la receta en el feed global
        );

        EjecutarAccionRol();
        return publica;
    }
}

/// <summary>
/// Administrador de la plataforma con permisos de moderación y gestión del catálogo global (UC02, UC17).
/// </summary>
// Administrador hereda de Usuario y cuenta con privilegios elevados de auditoría y moderación forzada.
public class Administrador : Usuario
{
    [JsonInclude]
    private string nivelAcceso = "SUPER_ADMIN_MODERADOR"; // nivel de acceso del administrador


    // Constructor por defecto fijando el rol en ADMINISTRADOR.
    public Administrador() : base()
    {
        tipoPerfil = TipoPerfilUsuario.ADMINISTRADOR;
    }


    // Constructor con parámetros que inicializa el nivel de acceso y llama al constructor base.
    public Administrador(string idUsuario, string nombreUsuario, string correo, string biografia, string nivelAcceso, ConfiguracionPerfil? configuracion = null, string password = "")
        : base(idUsuario, nombreUsuario, correo, biografia, TipoPerfilUsuario.ADMINISTRADOR, configuracion, password)
    {
        this.nivelAcceso = nivelAcceso;
    }

    public string NivelAcceso() => nivelAcceso;


    // override implementa la acción polimórfica indicando los privilegios de moderación activa sobre el feed.
    public override void EjecutarAccionRol()
    {
        UltimaAccionRolMensaje = $"Administrador '{nombreUsuario}' [Nivel: {nivelAcceso}]: auditoría activa sobre el Feed Público y sincronización JSON Global (UC02 / UC17).";
    }


    // Registra la moderación forzada y eliminación de una receta pública inapropiada.
    public void ForzarEliminacionReceta(string idReceta)
    {
        UltimaAccionRolMensaje = $"Administrador '{nombreUsuario}' ejecutó eliminación forzada de la receta ID '{idReceta}'.";
    }
}
