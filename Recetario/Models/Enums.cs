namespace Recetario.Models;

/// <summary>
/// Nivel de complejidad técnica de una receta.
/// </summary>
// enum define un conjunto de constantes enteras con nombres descriptivos para clasificar la dificultad.
public enum NivelDificultad
{
    FACIL, // preparación simple y rápida
    MEDIA, // requiere técnicas intermedias de cocción
    DIFICIL // recetas avanzadas o elaboradas
}

/// <summary>
/// Estación del año asociada a recetas e ingredientes de temporada.
/// </summary>
// Estacion permite categorizar frutas, verduras y platillos según el ciclo estacional.
public enum Estacion
{
    TODOELANHIO, // disponible en todas las épocas del año
    PRIMAVERA, // temporada de floración y brotes
    VERANO, // temporada de mayor calor y frutas dulces
    OTONIO, // temporada de cosechas y vegetales consistentes
    INVIERNO // temporada de frío, cítricos y raíces
}

/// <summary>
/// País de origen gastronómico o de cultivo.
/// </summary>
// Origen identifica la procedencia culinaria de las recetas o la zona de cultivo de los vegetales.
public enum Origen
{
    CHILE, // gastronomía o cultivo chileno
    ARGENTINA, // gastronomía o cultivo argentino
    PERU, // gastronomía o cultivo peruano
    COLOMBIA, // gastronomía o cultivo colombiano
    OTRO // otros orígenes internacionales
}

/// <summary>
/// Rol o tipo de perfil del usuario dentro del sistema Raccolta di ricette.
/// </summary>
// TipoPerfilUsuario define los 3 roles de usuario disponibles en la plataforma según el diagrama de casos de uso.
public enum TipoPerfilUsuario
{
    USUARIO_NORMAL, // cocinero amateur con acceso a recetario privado y feed
    CHEF_VERIFICADO, // creador especialista que puede certificar y publicar recetas en el catálogo global
    ADMINISTRADOR // moderador general con permisos para forzar eliminación y moderar contenidos
}

/// <summary>
/// Unidades de medida para los ingredientes de una receta.
/// </summary>
// UnidadMedida estandariza las proporciones y cantidades utilizadas en los ingredientes.
public enum UnidadMedida
{
    GRAMO, // peso en gramos (g)
    KILOGRAMO, // peso en kilogramos (kg)
    MILILITRO, // volumen en mililitros (ml)
    LITRO, // volumen en litros (l)
    TAZAS, // medida culinaria en tazas
    CUCHARADAS, // medida culinaria en cucharadas
    UNIDADES, // cantidad por piezas enteras
    ALGUSTO // porción libre según preferencia del cocinero
}

/// <summary>
/// Clasificación botánica/culinaria del ingrediente vegetal.
/// </summary>
// TipoVegetal clasifica si el ingrediente de temporada es botánicamente una fruta, verdura u otro.
public enum TipoVegetal
{
    FRUTA, // fruto carnoso dulce o ácido
    VERDURA, // hortaliza, tallo, hoja o raíz
    OTRO, // semillas, frutos secos u otros complementos
    NO_APLICA // lácteos, carnes o ingredientes no vegetales
}
