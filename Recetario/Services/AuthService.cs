using Recetario.Models;

namespace Recetario.Services;

/// <summary>
/// Servicio de autenticación y registro de usuarios con segregación en archivos JSON:
/// - usuarios_normales.json: Usuarios estándar / cocineros
/// - administradores.json: Administradores con privilegios de moderación
/// - codigos_admin.json: Códigos únicos de un solo uso para habilitar el registro de administradores
/// </summary>
// AuthService gestiona el inicio de sesión, validación de contraseñas y registro de usuarios estándar y administradores.
public class AuthService
{

    // readonly significa que estas referencias a los repositorios de persistencia solo se asignan en el constructor.
    private readonly IPersistenciaJson<UsuarioNormal> _repoUsuariosNormales; // repositorio para el archivo usuarios_normales.json
    private readonly IPersistenciaJson<Administrador> _repoAdministradores; // repositorio para el archivo administradores.json
    private readonly IPersistenciaJson<CodigoConfirmacionAdmin> _repoCodigosAdmin; // repositorio para el archivo codigos_admin.json

    // Listas en memoria (caché) para consultar y manipular rápidamente los usuarios sin leer el disco en cada operación.
    private List<UsuarioNormal> _cacheUsuariosNormales = new();
    private List<Administrador> _cacheAdministradores = new();
    private List<CodigoConfirmacionAdmin> _cacheCodigosAdmin = new();


    // AuthService sirve para inicializar los repositorios JSON correspondientes y cargar los datos a memoria.
    public AuthService()
    {
        _repoUsuariosNormales = new PersistenciaJsonService<UsuarioNormal>("usuarios_normales.json"); // inicializa servicio para usuarios normales
        _repoAdministradores = new PersistenciaJsonService<Administrador>("administradores.json"); // inicializa servicio para administradores
        _repoCodigosAdmin = new PersistenciaJsonService<CodigoConfirmacionAdmin>("codigos_admin.json"); // inicializa servicio para codigos de activacion

        CargarDatos(); // carga los datos desde los archivos JSON o siembra valores iniciales si no existen
    }


    // Metodo privado que carga los datos desde los archivos JSON o crea usuarios iniciales por defecto si los archivos no existen.
    private void CargarDatos()
    {
        // 1. Cargar o sembrar usuarios normales
        if (_repoUsuariosNormales.ExisteArchivoJson()) // verifica si el archivo JSON ya existe en disco
        {
            _cacheUsuariosNormales = _repoUsuariosNormales.CargarDesdeArchivo(); // deserializa la lista existente
        }
        else // si es la primera vez que se ejecuta la aplicacion crea un usuario normal inicial
        {
            _cacheUsuariosNormales = new List<UsuarioNormal>
            {
                new UsuarioNormal(
                    idUsuario: "USR-01",
                    nombreUsuario: "Fernando Cocinero",
                    correo: "fernando@raccolta.cl",
                    biografia: "Amante de la cocina casera con frutas y verduras frescas de estación.",
                    configuracion: new ConfiguracionPerfil(Estacion.PRIMAVERA, Origen.CHILE, "Valle Central", true),
                    password: "123") // contraseña inicial para pruebas
            };
            _repoUsuariosNormales.GuardarEnArchivos(_cacheUsuariosNormales); // guarda el usuario inicial en el archivo JSON
        }

        // 2. Cargar o sembrar administradores
        if (_repoAdministradores.ExisteArchivoJson())
        {
            _cacheAdministradores = _repoAdministradores.CargarDesdeArchivo();
        }
        else // crea el administrador inicial por defecto
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
                    password: "admin") // contraseña inicial de administrador
            };
            _repoAdministradores.GuardarEnArchivos(_cacheAdministradores); // guarda el administrador en el archivo JSON
        }

        // 3. Cargar o sembrar códigos de confirmación de administrador
        if (_repoCodigosAdmin.ExisteArchivoJson())
        {
            _cacheCodigosAdmin = _repoCodigosAdmin.CargarDesdeArchivo();
        }
        else // siembra 4 codigos unicos de un solo uso para poder registrar nuevos administradores
        {
            _cacheCodigosAdmin = new List<CodigoConfirmacionAdmin>
            {
                new("ADM-CHILE-2026"),
                new("ADM-BROCCOLI-9981"),
                new("ADM-MASTER-7734"),
                new("ADM-RACCOLTA-5512")
            };
            _repoCodigosAdmin.GuardarEnArchivos(_cacheCodigosAdmin); // guarda los codigos en el archivo JSON
        }
    }

    /// <summary>
    /// Intenta iniciar sesión buscando en el JSON correspondiente según si es administrador o usuario común.
    /// </summary>
    // Metodo que valida credenciales y retorna una tupla (Exito, Mensaje, UsuarioLogueado).
    public (bool Exito, string Mensaje, Usuario? UsuarioLogueado) IniciarSesion(string identificador, string password, bool esModoAdmin)
    {
        // Valida que el identificador (correo o nombre) y la contrasena no esten en blanco
        if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Por favor, ingresa tu usuario/correo y contraseña.", null);
        }

        string idLimpio = identificador.Trim().ToLowerInvariant(); // limpia espacios y convierte a minusculas para comparar

        if (esModoAdmin) // si el switch de administrador esta encendido busca en la lista de administradores
        {
            // FirstOrDefault busca el primer administrador que coincida en correo o nombre de usuario
            var admin = _cacheAdministradores.FirstOrDefault(a =>
                a.Correo().Trim().ToLowerInvariant() == idLimpio ||
                a.NombreUsuario().Trim().ToLowerInvariant() == idLimpio);

            if (admin == null) // si no encontro ninguna coincidencia
            {
                return (false, "No se encontró ningún Administrador con ese correo o nombre de usuario.", null);
            }

            if (!admin.ValidarPassword(password)) // valida si la contraseña coincide con la almacenada
            {
                return (false, "Contraseña incorrecta para la cuenta de Administrador.", null);
            }

            return (true, $"¡Bienvenido de vuelta, Administrador {admin.NombreUsuario()}!", admin); // login de admin exitoso
        }
        else // si es modo usuario regular / cocinero
        {
            // FirstOrDefault busca el primer usuario normal que coincida en correo o nombre
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

            return (true, $"¡Bienvenido {usuario.NombreUsuario()} a Raccolta di ricette!", usuario); // login exitoso
        }
    }

    /// <summary>
    /// Registra un nuevo usuario normal en usuarios_normales.json.
    /// </summary>
    // Metodo que valida datos e inserta un nuevo UsuarioNormal en el archivo JSON.
    public (bool Exito, string Mensaje, UsuarioNormal? NuevoUsuario) RegistrarUsuarioNormal(
        string nombre, string correo, string password, Estacion estacion = Estacion.PRIMAVERA, Origen origen = Origen.CHILE, string region = "Central")
    {
        // Validacion de longitud minima del nombre
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length < 3)
            return (false, "El nombre debe tener al menos 3 caracteres.", null);

        // Validacion de formato de correo electronico
        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
            return (false, "Ingresa un correo electrónico válido.", null);

        // Validacion de longitud minima de contraseña
        if (string.IsNullOrWhiteSpace(password) || password.Length < 3)
            return (false, "La contraseña debe tener al menos 3 caracteres.", null);

        string correoLimpio = correo.Trim().ToLowerInvariant();
        // Any verifica si ya existe un usuario registrado con el mismo correo electronico
        if (_cacheUsuariosNormales.Any(u => u.Correo().Trim().ToLowerInvariant() == correoLimpio))
        {
            return (false, "Ya existe un usuario registrado con este correo electrónico.", null);
        }

        string nuevoId = $"USR-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"; // genera un ID unico con formato USR-XXXXXX
        var nuevoUsuario = new UsuarioNormal(
            idUsuario: nuevoId,
            nombreUsuario: nombre.Trim(),
            correo: correo.Trim(),
            biografia: "Cocinero entusiasta de recetas de temporada.",
            configuracion: new ConfiguracionPerfil(estacion, origen, region, true),
            password: password
        );

        _cacheUsuariosNormales.Add(nuevoUsuario); // agrega a la lista en memoria
        _repoUsuariosNormales.GuardarEnArchivos(_cacheUsuariosNormales); // persiste de inmediato en el archivo JSON

        return (true, "¡Cuenta creada exitosamente! Ya puedes iniciar sesión.", nuevoUsuario);
    }

    /// <summary>
    /// Registra un nuevo administrador previa validación de código de confirmación de un solo uso en codigos_admin.json.
    /// </summary>
    // Metodo que valida el codigo de seguridad y registra una nueva cuenta de tipo Administrador.
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
        // Busca el codigo ingresado en la lista de codigos de confirmacion
        var codigoItem = _cacheCodigosAdmin.FirstOrDefault(c => c.Codigo.Trim().ToUpperInvariant() == codLimpio);

        if (codigoItem == null) // si no existe el codigo
        {
            return (false, "El código de confirmación ingresado no es válido.", null);
        }

        if (codigoItem.Utilizado) // si el codigo ya fue canjeado por otra persona
        {
            return (false, $"Este código ya fue utilizado anteriormente por '{codigoItem.UsadoPorCorreo}'. Solicita uno nuevo.", null);
        }

        string correoLimpio = correo.Trim().ToLowerInvariant();
        if (_cacheAdministradores.Any(a => a.Correo().Trim().ToLowerInvariant() == correoLimpio))
        {
            return (false, "Ya existe un administrador registrado con este correo electrónico.", null);
        }

        // Marcar código como consumido para que nadie mas lo pueda utilizar
        codigoItem.Utilizado = true;
        codigoItem.UsadoPorCorreo = correo.Trim();
        codigoItem.FechaUso = DateTime.Now;
        _repoCodigosAdmin.GuardarEnArchivos(_cacheCodigosAdmin); // actualiza el estado del codigo en el JSON

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

        _cacheAdministradores.Add(nuevoAdmin); // agrega a la lista de administradores en memoria
        _repoAdministradores.GuardarEnArchivos(_cacheAdministradores); // persiste en el archivo administradores.json

        return (true, "¡Cuenta de Administrador activada con éxito!", nuevoAdmin);
    }


    // Devuelve la lista de codigos de confirmacion que todavia no han sido utilizados filtrados con LINQ Where.
    public List<CodigoConfirmacionAdmin> ObtenerCodigosDisponibles() =>
        _cacheCodigosAdmin.Where(c => !c.Utilizado).ToList();
}
