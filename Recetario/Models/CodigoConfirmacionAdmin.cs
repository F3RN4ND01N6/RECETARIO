using System.Text.Json.Serialization;

namespace Recetario.Models;

/// <summary>
/// Código único de confirmación para habilitar el registro de un Administrador en la plataforma.
/// Se persiste en codigos_admin.json y es de un solo uso (no repetible entre sí).
/// </summary>
// CodigoConfirmacionAdmin implementa IIdentificable para representar los códigos de registro de un solo uso para administradores.
public class CodigoConfirmacionAdmin : IIdentificable
{

    // [JsonInclude] indica que la propiedad debe ser serializada y deserializada en el archivo JSON aunque tenga modificadores específicos.
    [JsonInclude]
    public string Codigo { get; set; } = string.Empty; // texto alfanumérico único del código de confirmación.

    [JsonInclude]
    public bool Utilizado { get; set; } = false; // indica si el código ya fue consumido por un administrador.

    [JsonInclude]
    public string? UsadoPorCorreo { get; set; } // correo electrónico del administrador que canjeó este código.

    [JsonInclude]
    public DateTime? FechaUso { get; set; } // fecha y hora exacta en la que se consumió el código.


    // Constructor vacío necesario para que el deserializador System.Text.Json pueda instanciar la clase al leer el JSON.
    public CodigoConfirmacionAdmin()
    {
    }


    // Constructor con parámetro para generar un nuevo código de activación disponible.
    public CodigoConfirmacionAdmin(string codigo)
    {
        Codigo = codigo;
        Utilizado = false; // por defecto inicia como no utilizado
    }


    // Implementación de IIdentificable: devuelve el texto del código como identificador único.
    public string ObtenerIdentificador() => Codigo;


    // Implementación de IIdentificable: devuelve un resumen del código indicando si está disponible o quién lo utilizó.
    public string ObtenerResumenEntidad() =>
        $"Código: {Codigo} | Estado: {(Utilizado ? $"Utilizado por {UsadoPorCorreo}" : "Disponible")}";
}
