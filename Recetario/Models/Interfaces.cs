namespace Recetario.Models;

/// <summary>
/// Contrato para entidades que poseen un identificador único y un resumen descriptivo.
/// </summary>
// interface define un conjunto de métodos o propiedades que una clase debe implementar obligatoriamente.
public interface IIdentificable
{
    // ObtenerIdentificador devuelve la clave primaria o identificador único de la entidad (ej: ID de usuario o receta).
    string ObtenerIdentificador();

    // ObtenerResumenEntidad devuelve un texto resumen con los datos clave de la entidad para mostrar en la interfaz.
    string ObtenerResumenEntidad();
}

/// <summary>
/// Contrato para entidades que validan la integridad de sus propios datos (UC04).
/// </summary>
// IValidable garantiza que cualquier entidad del modelo verifique sus reglas de negocio antes de ser guardada o serializada.
public interface IValidable
{
    // EsValido evalúa si la entidad cumple con todas sus reglas de negocio y retorna true o false.
    bool EsValido();

    // ObtenerErroresValidacion devuelve una lista con las descripciones de los campos inválidos encontrados.
    List<string> ObtenerErroresValidacion();
}

/// <summary>
/// Contrato para entidades que pueden recibir calificaciones y calcular su promedio (UC09).
/// </summary>
// ICalificable se utiliza en recetas públicas para permitir que los usuarios agreguen reseñas con estrellas y comentarios.
public interface ICalificable
{
    // AgregarCalificacion registra una nueva reseña o calificación en la entidad.
    void AgregarCalificacion(Calificacion calificacion);

    // ObtenerPromedioCalificaciones calcula y devuelve el promedio ponderado de estrellas (de 1.0 a 5.0).
    double ObtenerPromedioCalificaciones();
}

/// <summary>
/// Contrato para entidades que pueden evaluarse contra un FiltroReceta (UC06).
/// </summary>
// IFiltrable permite desacoplar los filtros de búsqueda de la implementación interna de cada receta.
public interface IFiltrable
{
    // CumpleFiltro evalúa si la receta coincide con todos los criterios seleccionados en el filtro.
    bool CumpleFiltro(FiltroReceta filtro);
}
