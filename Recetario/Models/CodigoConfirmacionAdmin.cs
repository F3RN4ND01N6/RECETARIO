using System.Text.Json.Serialization;

namespace Recetario.Models;

/// <summary>
/// Código único de confirmación para habilitar el registro de un Administrador en la plataforma.
/// Se persiste en codigos_admin.json y es de un solo uso (no repetible entre sí).
/// </summary>
public class CodigoConfirmacionAdmin : IIdentificable
{
    [JsonInclude]
    public string Codigo { get; set; } = string.Empty;

    [JsonInclude]
    public bool Utilizado { get; set; } = false;

    [JsonInclude]
    public string? UsadoPorCorreo { get; set; }

    [JsonInclude]
    public DateTime? FechaUso { get; set; }

    public CodigoConfirmacionAdmin()
    {
    }

    public CodigoConfirmacionAdmin(string codigo)
    {
        Codigo = codigo;
        Utilizado = false;
    }

    public string ObtenerIdentificador() => Codigo;

    public string ObtenerResumenEntidad() =>
        $"Código: {Codigo} | Estado: {(Utilizado ? $"Utilizado por {UsadoPorCorreo}" : "Disponible")}";
}
