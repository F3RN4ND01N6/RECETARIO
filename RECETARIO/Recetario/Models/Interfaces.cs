namespace Recetario.Models;

/// <summary>
/// Contrato para entidades que poseen un identificador único y un resumen descriptivo.
/// </summary>
public interface IIdentificable
{
    string ObtenerIdentificador();
    string ObtenerResumenEntidad();
}

/// <summary>
/// Contrato para entidades que validan la integridad de sus propios datos (UC04).
/// </summary>
public interface IValidable
{
    bool EsValido();
    List<string> ObtenerErroresValidacion();
}

/// <summary>
/// Contrato para entidades que pueden recibir calificaciones y calcular su promedio (UC09).
/// </summary>
public interface ICalificable
{
    void AgregarCalificacion(Calificacion calificacion);
    double ObtenerPromedioCalificaciones();
}

/// <summary>
/// Contrato para entidades que pueden evaluarse contra un FiltroReceta (UC06).
/// </summary>
public interface IFiltrable
{
    bool CumpleFiltro(FiltroReceta filtro);
}
