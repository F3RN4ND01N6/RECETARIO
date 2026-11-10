using System.Text.Encodings.Web;
using System.Text.Json;

namespace Recetario.Services;

/// <summary>
/// Implementación genérica de persistencia en Memoria Secundaria JSON (UC14, UC15, UC16, UC17).
/// Guarda y actualiza directamente en la carpeta 'BaseDatos_RecetasApp' dentro del código fuente del proyecto.
/// </summary>
// PersistenciaJsonService implementa la interfaz genérica IPersistenciaJson<T> para persistir listas de objetos en formato JSON.
public class PersistenciaJsonService<T> : IPersistenciaJson<T>
{

    // readonly significa que esta variable de ruta solo se asigna en el constructor y no puede ser modificada.
    private readonly string rutaArchivo; // almacena la ruta absoluta hacia el archivo JSON.

    // Configuración con WriteIndented = true para que el JSON quede perfectamente formateado en el código
    // static readonly crea una única instancia de opciones compartida y segura para hilos.
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = true, // formatea el texto JSON con sangría y saltos de línea para que sea legible
        IncludeFields = true, // incluye campos públicos y privados durante la serialización
        PropertyNameCaseInsensitive = true, // ignora mayúsculas y minúsculas al leer propiedades
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // permite tildes, ñ y caracteres especiales sin escapar a unicode (\u00f1)
    };


    // Constructor del servicio de persistencia, recibe el nombre del archivo JSON (ej: 'RecetasPrivadas.json') y resuelve su ruta.
    public PersistenciaJsonService(string nombreArchivo)
    {
        // 1. Obtener la ruta de la carpeta BaseDatos_RecetasApp en el mismo proyecto/código
        string carpetaBase = ResolverCarpetaBaseDatos();

        // 2. Asegurar que la carpeta exista, si no existe la crea en el disco duro
        if (!Directory.Exists(carpetaBase))
        {
            Directory.CreateDirectory(carpetaBase);
        }

        // 3. Fijar la ruta absoluta del archivo JSON correspondiente combinando la carpeta con el nombre
        rutaArchivo = Path.Combine(carpetaBase, nombreArchivo);
    }

    /// <summary>
    /// Localiza dinámicamente la carpeta 'BaseDatos_RecetasApp' en el proyecto (junto al código fuente),
    /// navegando hacia arriba desde el directorio de compilación (bin/Debug/...) hacia la raíz de la solución.
    /// </summary>
    // static metodo que no requiere una instancia para ejecutarse y localiza la carpeta física en disco.
    private static string ResolverCarpetaBaseDatos()
    {
        // 1. Buscar hacia arriba desde el directorio del ejecutable (bin\Debug\...)
        string? directorioActual = AppDomain.CurrentDomain.BaseDirectory;
        while (!string.IsNullOrEmpty(directorioActual))
        {
            string candidatoDirecto = Path.Combine(directorioActual, "BaseDatos_RecetasApp");
            if (Directory.Exists(candidatoDirecto))
            {
                return candidatoDirecto; // retorna la carpeta encontrada directamente
            }

            string candidatoEnRecetario = Path.Combine(directorioActual, "Recetario", "BaseDatos_RecetasApp");
            if (Directory.Exists(candidatoEnRecetario))
            {
                return candidatoEnRecetario; // retorna la carpeta encontrada dentro del subdirectorio Recetario
            }

            DirectoryInfo? padre = Directory.GetParent(directorioActual); // sube un nivel en la jerarquía de carpetas
            directorioActual = padre?.FullName;
        }

        // 2. Buscar desde el directorio de trabajo actual por si se lanza desde Visual Studio o CLI
        string dirTrabajo = Environment.CurrentDirectory;
        string candidatoTrabajo = Path.Combine(dirTrabajo, "BaseDatos_RecetasApp");
        if (Directory.Exists(candidatoTrabajo))
        {
            return candidatoTrabajo;
        }

        string candidatoTrabajoSub = Path.Combine(dirTrabajo, "Recetario", "BaseDatos_RecetasApp");
        if (Directory.Exists(candidatoTrabajoSub))
        {
            return candidatoTrabajoSub;
        }

        // 3. Fallback: subir 4 niveles desde bin/Debug/net10.0-windows... a la carpeta del proyecto
        string rutaFallback = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "BaseDatos_RecetasApp"));
        return rutaFallback;
    }


    // Guarda y serializa la lista de datos en el archivo físico en disco en formato JSON.
    public void GuardarEnArchivos(List<T> datos)
    {
        // JsonSerializer.Serialize convierte la lista de objetos de tipo T en una cadena de texto JSON.
        string contenidoJson = JsonSerializer.Serialize(datos ?? new List<T>(), OpcionesJson);
        File.WriteAllText(rutaArchivo, contenidoJson); // escribe todo el texto en el archivo sobrescribiendo el contenido anterior
    }


    // Carga y deserializa los datos desde el archivo físico JSON a una lista de objetos en memoria.
    public List<T> CargarDesdeArchivo()
    {
        if (!ExisteArchivoJson()) // si el archivo físico no existe retorna una lista vacía
            return new List<T>();

        try
        {
            string contenidoJson = File.ReadAllText(rutaArchivo); // lee todo el texto del archivo JSON
            if (string.IsNullOrWhiteSpace(contenidoJson)) // si el archivo esta vacio retorna lista vacia
                return new List<T>();

            // JsonSerializer.Deserialize convierte el texto JSON a una lista de objetos de tipo T
            return JsonSerializer.Deserialize<List<T>>(contenidoJson, OpcionesJson) ?? new List<T>();
        }
        catch // si ocurre algún error de formato o lectura captura la excepción y retorna una lista vacía
        {
            return new List<T>();
        }
    }


    // Comprueba si el archivo físico existe en el almacenamiento secundario.
    public bool ExisteArchivoJson() => File.Exists(rutaArchivo);


    // Devuelve la ruta absoluta del archivo en disco.
    public string ObtenerRutaArchivo() => rutaArchivo;
}
