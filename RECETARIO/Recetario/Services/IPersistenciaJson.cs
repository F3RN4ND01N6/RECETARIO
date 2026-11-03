namespace Recetario.Services;

/// <summary>
/// Contrato genérico para serialización y deserialización en archivos JSON (UC14, UC15, UC16, UC17).
/// </summary>
public interface IPersistenciaJson<T>
{
    void GuardarEnArchivos(List<T> datos);
    List<T> CargarDesdeArchivo();
    bool ExisteArchivoJson();
    string ObtenerRutaArchivo();
}
