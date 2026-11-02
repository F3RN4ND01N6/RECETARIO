using Recetario.Models;

namespace Recetario.Services;

/// <summary>
/// Servicio de autenticación y registro de usuarios con segregación en archivos JSON:
/// - usuarios_normales.json: Usuarios estándar / cocineros
/// - administradores.json: Administradores con privilegios de moderación
/// - codigos_admin.json: Códigos únicos de un solo uso para habilitar el registro de administradores
/// </summary>
public class AuthService
{
    private readonly IPersistenciaJson<UsuarioNormal> _repoUsuariosNormales;
    private readonly IPersistenciaJson<Administrador> _repoAdministradores;
    private readonly IPersistenciaJson<CodigoConfirmacionAdmin> _repoCodigosAdmin;

    private List<UsuarioNormal> _cacheUsuariosNormales = new();
    private List<Administrador> _cacheAdministradores = new();
    private List<CodigoConfirmacionAdmin> _cacheCodigosAdmin = new();

    public AuthService()
    {
        _repoUsuariosNormales = new PersistenciaJsonService<UsuarioNormal>("usuarios_normales.json");
        _repoAdministradores = new PersistenciaJsonService<Administrador>("administradores.json");
        _repoCodigosAdmin = new PersistenciaJsonService<CodigoConfirmacionAdmin>("codigos_admin.json");

        CargarDatos();
    }

    private void CargarDatos()
    {
        // 1. Cargar o sembrar usuarios normales
        if (_repoUsuariosNormales.ExisteArchivoJson())
        {
            _cacheUsuariosNormales = _repoUsuariosNormales.CargarDesdeArchivo();
        }
        else
        {
            _cacheUsuariosNormales = new List<UsuarioNormal>
            {
                new UsuarioNormal(
                    idUsuario: "USR-01",
                    nombreUsuario: "Fernando Cocinero",
                    correo: "fernando@raccolta.cl",
                    biografia: "Amante de la cocina casera con frutas y verduras frescas de estación.",
                    configuracion: new ConfiguracionPerfil(Estacion.PRIMAVERA, Origen.CHILE, "Valle Central", true),
                    password: "123")
            };
            _repoUsuariosNormales.GuardarEnArchivos(_cacheUsuariosNormales);
        }

        // 2. Cargar o sembrar administradores
        if (_repoAdministradores.ExisteArchivoJson())
        {
            _cacheAdministradores = _repoAdministradores.CargarDesdeArchivo();
        }
        else
        {
            _cacheAdministradores = new List<Administrador>
            {
                new Administrador(
                    idUsuario: "ADM-01",
                    nombreUsuario: "Admin Moderador",
                    correo: "admin@raccolta.cl",
                    biografia: "Administrador general del catálogo y moderación del Feed Público.",
                    nivelAcceso: "ROOT_MODERADOR_GLOBAL",
                    configuracion: new ConfiguracionPerfil(Estacion.TODOELANHIO, Origen.CHILE, "Santiago", true),
                    password: "admin")
            };
            _repoAdministradores.GuardarEnArchivos(_cacheAdministradores);
        }

        // 3. Cargar o sembrar códigos de confirmación de administrador
        if (_repoCodigosAdmin.ExisteArchivoJson())
        {
            _cacheCodigosAdmin = _repoCodigosAdmin.CargarDesdeArchivo();
        }
        else
        {
            _cacheCodigosAdmin = new List<CodigoConfirmacionAdmin>
            {
                new("ADM-CHILE-2026"),
                new("ADM-BROCCOLI-9981"),
                new("ADM-MASTER-7734"),
                new("ADM-RACCOLTA-5512")
            };
            _repoCodigosAdmin.GuardarEnArchivos(_cacheCodigosAdmin);
        }
    }

    /// <summary>
    /// Intenta iniciar sesión buscando en el JSON correspondiente según si es administrador o usuario común.
    /// </summary>
    public (bool Exito, string Mensaje, Usuario? UsuarioLogueado) IniciarSesion(string identificador, string password, bool esModoAdmin)
    {
        if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Por favor, ingresa tu usuario/correo y contraseña.", null);
        }

        string idLimpio = identificador.Trim().ToLowerInvariant();

        if (esModoAdmin)
        {
            var admin = _cacheAdministradores.FirstOrDefault(a =>
                a.Correo().Trim().ToLowerInvariant() == idLimpio ||
                a.NombreUsuario().Trim().ToLowerInvariant() == idLimpio);

            if (admin == null)
            {
                return (false, "No se encontró ningún Administrador con ese correo o nombre de usuario.", null);
            }

            if (!admin.ValidarPassword(password))
            {
                return (false, "Contraseña incorrecta para la cuenta de Administrador.", null);
            }

            return (true, $"¡Bienvenido de vuelta, Administrador {admin.NombreUsuario()}!", admin);
        }
        else
        {
            var usuario = _cacheUsuariosNormales.FirstOrDefault(u =>
                u.Correo().Trim().ToLowerInvariant() == idLimpio ||
                u.NombreUsuario().Trim().ToLowerInvariant() == idLimpio);

            if (usuario == null)
            {
                return (false, "No se encontró ningún usuario con ese correo o nombre. ¿Deseas registrarte?", null);
            }

            if (!usuario.ValidarPassword(password))
            {
                return (false, "Contraseña incorrecta.", null);
            }

            return (true, $"¡Bienvenido {usuario.NombreUsuario()} a Raccolta di ricette!", usuario);
        }
    }

    /// <summary>
    /// Registra un nuevo usuario normal en usuarios_normales.json.
    /// </summary>
    public (bool Exito, string Mensaje, UsuarioNormal? NuevoUsuario) RegistrarUsuarioNormal(
        string nombre, string correo, string password, Estacion estacion = Estacion.PRIMAVERA, Origen origen = Origen.CHILE, string region = "Central")
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length < 3)
            return (false, "El nombre debe tener al menos 3 caracteres.", null);

        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
            return (false, "Ingresa un correo electrónico válido.", null);

        if (string.IsNullOrWhiteSpace(password) || password.Length < 3)
            return (false, "La contraseña debe tener al menos 3 caracteres.", null);

        string correoLimpio = correo.Trim().ToLowerInvariant();
        if (_cacheUsuariosNormales.Any(u => u.Correo().Trim().ToLowerInvariant() == correoLimpio))
        {
            return (false, "Ya existe un usuario registrado con este correo electrónico.", null);
        }

        string nuevoId = $"USR-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        var nuevoUsuario = new UsuarioNormal(
            idUsuario: nuevoId,
            nombreUsuario: nombre.Trim(),
            correo: correo.Trim(),
            biografia: "Cocinero entusiasta de recetas de temporada.",
            configuracion: new ConfiguracionPerfil(estacion, origen, region, true),
            password: password
        );

        _cacheUsuariosNormales.Add(nuevoUsuario);
        _repoUsuariosNormales.GuardarEnArchivos(_cacheUsuariosNormales);

        return (true, "¡Cuenta creada exitosamente! Ya puedes iniciar sesión.", nuevoUsuario);
    }

    /// <summary>
    /// Registra un nuevo administrador previa validación de código de confirmación de un solo uso en codigos_admin.json.
    /// </summary>
    public (bool Exito, string Mensaje, Administrador? NuevoAdmin) RegistrarAdministrador(
        string nombre, string correo, string password, string codigoConfirmacion)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length < 3)
            return (false, "El nombre debe tener al menos 3 caracteres.", null);

        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
            return (false, "Ingresa un correo electrónico válido.", null);

        if (string.IsNullOrWhiteSpace(password) || password.Length < 3)
            return (false, "La contraseña debe tener al menos 3 caracteres.", null);

        if (string.IsNullOrWhiteSpace(codigoConfirmacion))
            return (false, "El código de confirmación de administrador es obligatorio.", null);

        string codLimpio = codigoConfirmacion.Trim().ToUpperInvariant();
        var codigoItem = _cacheCodigosAdmin.FirstOrDefault(c => c.Codigo.Trim().ToUpperInvariant() == codLimpio);

        if (codigoItem == null)
        {
            return (false, "El código de confirmación ingresado no es válido.", null);
        }

        if (codigoItem.Utilizado)
        {
            return (false, $"Este código ya fue utilizado anteriormente por '{codigoItem.UsadoPorCorreo}'. Solicita uno nuevo.", null);
        }

        string correoLimpio = correo.Trim().ToLowerInvariant();
        if (_cacheAdministradores.Any(a => a.Correo().Trim().ToLowerInvariant() == correoLimpio))
        {
            return (false, "Ya existe un administrador registrado con este correo electrónico.", null);
        }

        // Marcar código como consumido
        codigoItem.Utilizado = true;
        codigoItem.UsadoPorCorreo = correo.Trim();
        codigoItem.FechaUso = DateTime.Now;
        _repoCodigosAdmin.GuardarEnArchivos(_cacheCodigosAdmin);

        // Crear administrador
        string nuevoId = $"ADM-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        var nuevoAdmin = new Administrador(
            idUsuario: nuevoId,
            nombreUsuario: nombre.Trim(),
            correo: correo.Trim(),
            biografia: "Administrador y moderador de recetas de la comunidad.",
            nivelAcceso: "ADMINISTRADOR_VERIFICADO",
            configuracion: new ConfiguracionPerfil(Estacion.TODOELANHIO, Origen.CHILE, "Chile", true),
            password: password
        );

        _cacheAdministradores.Add(nuevoAdmin);
        _repoAdministradores.GuardarEnArchivos(_cacheAdministradores);

        return (true, "¡Cuenta de Administrador activada con éxito!", nuevoAdmin);
    }

    public List<CodigoConfirmacionAdmin> ObtenerCodigosDisponibles() =>
        _cacheCodigosAdmin.Where(c => !c.Utilizado).ToList();
}
