namespace Recetario.Models;

/// <summary>
/// Nivel de complejidad técnica de una receta.
/// </summary>
public enum NivelDificultad
{
    FACIL,
    MEDIA,
    DIFICIL
}

/// <summary>
/// Estación del año asociada a recetas e ingredientes de temporada.
/// </summary>
public enum Estacion
{
    TODOELANHIO,
    PRIMAVERA,
    VERANO,
    OTONIO,
    INVIERNO
}

/// <summary>
/// País de origen gastronómico o de cultivo.
/// </summary>
public enum Origen
{
    CHILE,
    ARGENTINA,
    PERU,
    COLOMBIA,
    OTRO
}

/// <summary>
/// Rol o tipo de perfil del usuario dentro del sistema Raccolta di ricette.
/// </summary>
public enum TipoPerfilUsuario
{
    USUARIO_NORMAL,
    CHEF_VERIFICADO,
    ADMINISTRADOR
}

/// <summary>
/// Unidades de medida para los ingredientes de una receta.
/// </summary>
public enum UnidadMedida
{
    GRAMO,
    KILOGRAMO,
    MILILITRO,
    LITRO,
    TAZAS,
    CUCHARADAS,
    UNIDADES,
    ALGUSTO
}

/// <summary>
/// Clasificación botánica/culinaria del ingrediente vegetal.
/// </summary>
public enum TipoVegetal
{
    FRUTA,
    VERDURA,
    OTRO,
    NO_APLICA
}
