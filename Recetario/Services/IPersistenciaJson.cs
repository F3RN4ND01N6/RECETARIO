namespace Recetario.Services;

/// <summary>
/// Contrato genérico para serialización y deserialización en archivos JSON (UC14, UC15, UC16, UC17).
/// </summary>
// interface define un contrato que cualquier clase que lo implemente debe cumplir obligatoriamente.
// <T> indica que la interfaz es genérica y puede operar con cualquier tipo de dato (por ejemplo RecetaPublica o RecetaPrivada).
public interface IPersistenciaJson<T>
{
    // Guarda y serializa la lista de datos de tipo T en el archivo JSON físico.
    void GuardarEnArchivos(List<T> datos);

    // Carga y deserializa los datos desde el archivo JSON devolviendo una lista de objetos de tipo T.
    List<T> CargarDesdeArchivo();

    // Comprueba si el archivo físico JSON existe en el disco duro o almacenamiento local.
    bool ExisteArchivoJson();

    // Devuelve la ruta absoluta del archivo JSON en el sistema de archivos.
    string ObtenerRutaArchivo();
}
