using System.Text.Encodings.Web;
using System.Text.Json;

namespace Recetario.Services;

/// <summary>
/// Implementación genérica de persistencia en Memoria Secundaria JSON (UC14, UC15, UC16, UC17).
/// Guarda y actualiza directamente en la carpeta 'BaseDatos_RecetasApp' dentro del código fuente del proyecto.
/// </summary>
public class PersistenciaJsonService<T> : IPersistenciaJson<T>
{
    private readonly string rutaArchivo;

    // Configuración con WriteIndented = true para que el JSON quede perfectamente formateado en el código
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public PersistenciaJsonService(string nombreArchivo)
    {
        // 1. Obtener la ruta de la carpeta BaseDatos_RecetasApp en el mismo proyecto/código
        string carpetaBase = ResolverCarpetaBaseDatos();

        // 2. Asegurar que la carpeta exista
        if (!Directory.Exists(carpetaBase))
        {
            Directory.CreateDirectory(carpetaBase);
        }

        // 3. Fijar la ruta absoluta del archivo JSON correspondiente
        rutaArchivo = Path.Combine(carpetaBase, nombreArchivo);
    }

    /// <summary>
    /// Localiza dinámicamente la carpeta 'BaseDatos_RecetasApp' en el proyecto (junto al código fuente),
    /// navegando hacia arriba desde el directorio de compilación (bin/Debug/...) hacia la raíz de la solución.
    /// </summary>
    private static string ResolverCarpetaBaseDatos()
    {
        // 1. Buscar hacia arriba desde el directorio del ejecutable (bin\Debug\...)
        string? directorioActual = AppDomain.CurrentDomain.BaseDirectory;
        while (!string.IsNullOrEmpty(directorioActual))
        {
            string candidatoDirecto = Path.Combine(directorioActual, "BaseDatos_RecetasApp");
            if (Directory.Exists(candidatoDirecto))
            {
                return candidatoDirecto;
            }

            string candidatoEnRecetario = Path.Combine(directorioActual, "Recetario", "BaseDatos_RecetasApp");
            if (Directory.Exists(candidatoEnRecetario))
            {
                return candidatoEnRecetario;
            }

            DirectoryInfo? padre = Directory.GetParent(directorioActual);
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

    public void GuardarEnArchivos(List<T> datos)
    {
        string contenidoJson = JsonSerializer.Serialize(datos ?? new List<T>(), OpcionesJson);
        File.WriteAllText(rutaArchivo, contenidoJson);
    }

    public List<T> CargarDesdeArchivo()
    {
        if (!ExisteArchivoJson())
            return new List<T>();

        try
        {
            string contenidoJson = File.ReadAllText(rutaArchivo);
            if (string.IsNullOrWhiteSpace(contenidoJson))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(contenidoJson, OpcionesJson) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    public bool ExisteArchivoJson() => File.Exists(rutaArchivo);

    public string ObtenerRutaArchivo() => rutaArchivo;
}
