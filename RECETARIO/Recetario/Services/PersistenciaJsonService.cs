using System.Text.Encodings.Web;
using System.Text.Json;

namespace Recetario.Services;

/// <summary>
/// Implementación genérica de persistencia en Memoria Secundaria JSON (UC14, UC15, UC16, UC17).
/// </summary>
public class PersistenciaJsonService<T> : IPersistenciaJson<T>
{
    private string rutaArchivo;

    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public PersistenciaJsonService(string nombreArchivo)
    {
        string carpetaBase = FileSystem.Current.AppDataDirectory;
        if (!Directory.Exists(carpetaBase))
        {
            Directory.CreateDirectory(carpetaBase);
        }
        rutaArchivo = Path.Combine(carpetaBase, nombreArchivo);
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
