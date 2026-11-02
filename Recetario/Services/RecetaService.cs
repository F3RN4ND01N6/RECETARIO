using Recetario.Models;

namespace Recetario.Services;

/// <summary>
/// Servicio principal de negocio y orquestación de Casos de Uso (UC01..UC09, UC14..UC17).
/// </summary>
public class RecetaService
{
    private IPersistenciaJson<RecetaPrivada> repoPrivado;
    private IPersistenciaJson<RecetaPublica> repoPublicoFeed;

    private List<RecetaPrivada> cachePrivadas = new();
    private List<RecetaPublica> cachePublicas = new();
    private List<IngredienteTemporada> catalogoTemporada = new();

    // Perfiles de los 3 actores del Diagrama de Casos de Uso para demostración y pruebas
    public UsuarioNormal PerfilUsuarioNormal { get; }
    public CreadorEspecialista PerfilCreadorEspecialista { get; }
    public Administrador PerfilAdministrador { get; }

    public Usuario UsuarioActivo { get; private set; }

    // Eventos de sincronización entre pestañas de la barra inferior
    public event Action? DatosActualizados;
    public event Action<RecetaPrivada>? SolicitudEditarReceta;
    public event Action<Estacion>? SolicitudFiltrarFeedPorEstacion;
    public event Action<string>? SolicitudCambiarPestana;

    public void NavegarAPestana(string destino)
    {
        SolicitudCambiarPestana?.Invoke(destino);
    }

    public RecetaService()
    {
        repoPrivado = new PersistenciaJsonService<RecetaPrivada>("recetas_privadas_local.json");
        repoPublicoFeed = new PersistenciaJsonService<RecetaPublica>("recetas_publicas_feed.json");

        var configInicial = new ConfiguracionPerfil(Estacion.PRIMAVERA, Origen.CHILE, "Valle Central", true);

        PerfilUsuarioNormal = new UsuarioNormal(
            idUsuario: "USR-01",
            nombreUsuario: "Fernando Cocinero",
            correo: "fernando@raccolta.cl",
            biografia: "Amante de la cocina casera con frutas y verduras frescas de estación.",
            configuracion: configInicial);

        PerfilCreadorEspecialista = new CreadorEspecialista(
            idUsuario: "CHEF-01",
            nombreUsuario: "Chef Valentina Rojas",
            correo: "valentina.chef@raccolta.cl",
            biografia: "Chef ejecutiva y nutricionista especializada en gastronomía sustentable de temporada.",
            credencialesChef: "Chef Ejecutiva & Nutricionista gastronómica certificada",
            configuracion: new ConfiguracionPerfil(Estacion.VERANO, Origen.CHILE, "Región del Maule", true));

        PerfilAdministrador = new Administrador(
            idUsuario: "ADM-01",
            nombreUsuario: "Admin Moderador",
            correo: "admin@raccolta.cl",
            biografia: "Administrador general del catálogo y moderación del Feed Público.",
            nivelAcceso: "ROOT_MODERADOR_GLOBAL",
            configuracion: new ConfiguracionPerfil(Estacion.TODOELANHIO, Origen.CHILE, "Santiago", true));

        UsuarioActivo = PerfilUsuarioNormal;
        UsuarioActivo.EjecutarAccionRol();

        InicializarCatalogoTemporada();
        CargarOInicializarDatos();
    }

    public void EstablecerUsuarioSesion(Usuario usuarioAutenticado)
    {
        UsuarioActivo = usuarioAutenticado ?? PerfilUsuarioNormal;
        UsuarioActivo.EjecutarAccionRol();
        DatosActualizados?.Invoke();
    }

    public void CambiarRolActivo(TipoPerfilUsuario nuevoTipo)
    {
        UsuarioActivo = nuevoTipo switch
        {
            TipoPerfilUsuario.CHEF_VERIFICADO => PerfilCreadorEspecialista,
            TipoPerfilUsuario.ADMINISTRADOR => PerfilAdministrador,
            _ => PerfilUsuarioNormal
        };
        UsuarioActivo.EjecutarAccionRol();
        DatosActualizados?.Invoke();
    }

    // === MÉTODOS EXACTOS DEL DIAGRAMA DE CLASES ===

    /// <summary>
    /// UC01 / UC04 / UC14: Valida y guarda una nueva receta privada en el repositorio JSON local.
    /// </summary>
    public void GuardarRecetaPrivada(RecetaPrivada receta)
    {
        if (receta == null)
            throw new ArgumentNullException(nameof(receta));

        if (!receta.EsValido())
        {
            string errores = string.Join("\n• ", receta.ObtenerErroresValidacion());
            throw new InvalidOperationException($"La receta no superó la validación (UC04):\n• {errores}");
        }

        int idx = cachePrivadas.FindIndex(r => r.ObtenerIdentificador() == receta.ObtenerIdentificador());
        if (idx >= 0)
        {
            cachePrivadas[idx] = receta;
        }
        else
        {
            cachePrivadas.Insert(0, receta);
        }

        repoPrivado.GuardarEnArchivos(cachePrivadas);
        DatosActualizados?.Invoke();
    }

    /// <summary>
    /// UC02 / UC04 / UC14: Edita una receta privada existente y actualiza el JSON local.
    /// </summary>
    public void EditarRecetaPrivada(RecetaPrivada receta)
    {
        if (receta == null)
            throw new ArgumentNullException(nameof(receta));

        if (!receta.EsValido())
        {
            string errores = string.Join("\n• ", receta.ObtenerErroresValidacion());
            throw new InvalidOperationException($"Error de validación al editar (UC04):\n• {errores}");
        }

        int idx = cachePrivadas.FindIndex(r => r.ObtenerIdentificador() == receta.ObtenerIdentificador());
        if (idx >= 0)
        {
            cachePrivadas[idx] = receta;
        }
        else
        {
            cachePrivadas.Insert(0, receta);
        }

        repoPrivado.GuardarEnArchivos(cachePrivadas);
        DatosActualizados?.Invoke();
    }

    /// <summary>
    /// UC02 / UC14: Elimina una receta privada por su identificador y sincroniza el JSON local.
    /// </summary>
    public void EliminarRecetaPrivada(string idReceta)
    {
        cachePrivadas.RemoveAll(r => r.ObtenerIdentificador() == idReceta);
        repoPrivado.GuardarEnArchivos(cachePrivadas);
        DatosActualizados?.Invoke();
    }

    /// <summary>
    /// UC16: Publica una receta en el Feed Global JSON (requiere rol CreadorEspecialista o convierte desde privada).
    /// </summary>
    public void PublicarRecetaEnFeed(RecetaPublica receta, Usuario autor)
    {
        if (receta == null)
            throw new ArgumentNullException(nameof(receta));

        if (!receta.EsValido())
        {
            string errores = string.Join("\n• ", receta.ObtenerErroresValidacion());
            throw new InvalidOperationException($"No se puede publicar en el Feed Global (UC04):\n• {errores}");
        }

        autor?.EjecutarAccionRol();

        int idx = cachePublicas.FindIndex(r => r.ObtenerIdentificador() == receta.ObtenerIdentificador());
        if (idx >= 0)
        {
            cachePublicas[idx] = receta;
        }
        else
        {
            cachePublicas.Insert(0, receta);
        }

        repoPublicoFeed.GuardarEnArchivos(cachePublicas);
        DatosActualizados?.Invoke();
    }

    /// <summary>
    /// UC06: Sobrecarga 1 — Filtra el Feed Global aplicando un objeto FiltroReceta completo.
    /// </summary>
    public List<RecetaPublica> FiltrarFeedGlobal(FiltroReceta filtro)
    {
        if (filtro == null)
            return new List<RecetaPublica>(cachePublicas);

        return cachePublicas.Where(r => r.CumpleFiltro(filtro)).ToList();
    }

    /// <summary>
    /// UC06: Sobrecarga 2 — Filtra el Feed Global directamente por Estación y País de Origen.
    /// </summary>
    public List<RecetaPublica> FiltrarFeedGlobal(Estacion estacion, Origen pais)
    {
        var filtroRapido = new FiltroReceta(
            estacionTemporada: estacion,
            pais: pais);
        return FiltrarFeedGlobal(filtroRapido);
    }

    /// <summary>
    /// UC06: Filtra las recetas privadas de un usuario según un FiltroReceta.
    /// </summary>
    public List<RecetaPrivada> FiltrarRecetasPrivadas(string idUsuario, FiltroReceta filtro)
    {
        var baseQuery = cachePrivadas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(idUsuario) && idUsuario != "TODOS")
        {
            baseQuery = baseQuery.Where(r =>
                string.IsNullOrWhiteSpace(r.IdUsuarioAutor) ||
                r.IdUsuarioAutor == idUsuario ||
                r.IdUsuarioAutor == "USR-01" ||
                r.IdUsuarioAutor == "CHEF-01");
        }

        if (filtro == null)
            return baseQuery.ToList();

        return baseQuery.Where(r => filtro.CumpleCriterios(r)).ToList();
    }

    /// <summary>
    /// UC07: Consulta los ingredientes de fruta y verdura correspondientes a una estación del año.
    /// </summary>
    public List<IngredienteTemporada> ConsultarCalendarioTemporada(Estacion estacion)
    {
        if (estacion == Estacion.TODOELANHIO)
            return new List<IngredienteTemporada>(catalogoTemporada);

        return catalogoTemporada
            .Where(i => i.EstacionPrincipal() == estacion || i.EstacionPrincipal() == Estacion.TODOELANHIO)
            .ToList();
    }

    // === MÉTODOS ADICIONALES PARA CASOS DE USO SOCIALES Y DE ARCHIVOS (UC02, UC08, UC09, UC14..UC17) ===

    public bool EsRecetaFavorita(string idReceta) =>
        PerfilUsuarioNormal.EsFavorita(idReceta);

    public bool AlternarFavorita(string idReceta)
    {
        if (PerfilUsuarioNormal.EsFavorita(idReceta))
        {
            PerfilUsuarioNormal.DesmarcarFavorita(idReceta);
            DatosActualizados?.Invoke();
            return false;
        }
        else
        {
            PerfilUsuarioNormal.MarcarFavorita(idReceta);
            DatosActualizados?.Invoke();
            return true;
        }
    }

    public void CalificarRecetaPublica(string idReceta, Calificacion calificacion)
    {
        var receta = cachePublicas.FirstOrDefault(r => r.ObtenerIdentificador() == idReceta);
        if (receta != null)
        {
            receta.AgregarCalificacion(calificacion);
            repoPublicoFeed.GuardarEnArchivos(cachePublicas);
            DatosActualizados?.Invoke();
        }
    }

    public void EliminarRecetaPublicaComoAdmin(string idReceta)
    {
        if (UsuarioActivo is Administrador admin)
        {
            admin.ForzarEliminacionReceta(idReceta);
        }
        cachePublicas.RemoveAll(r => r.ObtenerIdentificador() == idReceta);
        repoPublicoFeed.GuardarEnArchivos(cachePublicas);
        DatosActualizados?.Invoke();
    }

    public void GuardarRespaldoPrivadoManual()
    {
        repoPrivado.GuardarEnArchivos(cachePrivadas);
        DatosActualizados?.Invoke();
    }

    public void CargarRespaldoPrivadoManual()
    {
        if (repoPrivado.ExisteArchivoJson())
        {
            cachePrivadas = repoPrivado.CargarDesdeArchivo();
            DatosActualizados?.Invoke();
        }
    }

    public void CargarRespaldoGlobalManual()
    {
        if (repoPublicoFeed.ExisteArchivoJson())
        {
            cachePublicas = repoPublicoFeed.CargarDesdeArchivo();
            DatosActualizados?.Invoke();
        }
    }

    public string ObtenerRutaJsonPrivado() => repoPrivado.ObtenerRutaArchivo();
    public string ObtenerRutaJsonPublico() => repoPublicoFeed.ObtenerRutaArchivo();
    public int TotalRecetasPrivadas => cachePrivadas.Count;
    public int TotalRecetasPublicas => cachePublicas.Count;
    public int TotalFavoritas => PerfilUsuarioNormal.ObtenerIdsFavoritas().Count;

    public void DispararEdicionReceta(RecetaPrivada receta)
    {
        SolicitudEditarReceta?.Invoke(receta);
        SolicitudCambiarPestana?.Invoke("CrearEditar");
    }

    public void DispararFiltroEstacionEnFeed(Estacion estacion)
    {
        SolicitudFiltrarFeedPorEstacion?.Invoke(estacion);
        SolicitudCambiarPestana?.Invoke("Feed");
    }

    // === DATOS SEMILLA (SEED DATA) ===

    private void InicializarCatalogoTemporada()
    {
        catalogoTemporada = new List<IngredienteTemporada>
        {
            // VERANO
            new("Tomate Limachino", TipoVegetal.FRUTA, Estacion.VERANO, Origen.CHILE, "Valle de Limache"),
            new("Choclo Pastelero", TipoVegetal.VERDURA, Estacion.VERANO, Origen.CHILE, "Zona Central / O'Higgins"),
            new("Albahaca Fresca", TipoVegetal.VERDURA, Estacion.VERANO, Origen.CHILE, "Valle del Aconcagua"),
            new("Durazno conservero", TipoVegetal.FRUTA, Estacion.VERANO, Origen.ARGENTINA, "Mendoza"),
            new("Mango Kent", TipoVegetal.FRUTA, Estacion.VERANO, Origen.PERU, "Piura"),

            // OTOÑO
            new("Zapallo Camote", TipoVegetal.VERDURA, Estacion.OTONIO, Origen.CHILE, "Región del Maule"),
            new("Palta Hass", TipoVegetal.FRUTA, Estacion.OTONIO, Origen.CHILE, "Quillota / La Ligua"),
            new("Manzana Fuji", TipoVegetal.FRUTA, Estacion.OTONIO, Origen.CHILE, "Curicó"),
            new("Camote Morado", TipoVegetal.VERDURA, Estacion.OTONIO, Origen.PERU, "Valle de Cañete"),

            // INVIERNO
            new("Brócoli Fresco", TipoVegetal.VERDURA, Estacion.INVIERNO, Origen.CHILE, "Sabana Central"),
            new("Espinaca de Hoja", TipoVegetal.VERDURA, Estacion.INVIERNO, Origen.ARGENTINA, "Buenos Aires"),
            new("Naranja Valencia", TipoVegetal.FRUTA, Estacion.INVIERNO, Origen.COLOMBIA, "Eje Cafetero"),
            new("Coliflor Blanca", TipoVegetal.VERDURA, Estacion.INVIERNO, Origen.CHILE, "Melipilla"),

            // PRIMAVERA
            new("Espárragos Verdes", TipoVegetal.VERDURA, Estacion.PRIMAVERA, Origen.CHILE, "Ñuble / Chillán"),
            new("Frutilla Dulce", TipoVegetal.FRUTA, Estacion.PRIMAVERA, Origen.CHILE, "San Pedro / Chanco"),
            new("Alcachofa Chilena", TipoVegetal.VERDURA, Estacion.PRIMAVERA, Origen.CHILE, "Coquimbo / La Serena"),
            new("Maracuyá Dorado", TipoVegetal.FRUTA, Estacion.PRIMAVERA, Origen.COLOMBIA, "Valle del Cauca")
        };
    }

    private void CargarOInicializarDatos()
    {
        if (repoPublicoFeed.ExisteArchivoJson())
        {
            cachePublicas = repoPublicoFeed.CargarDesdeArchivo();
        }

        if (cachePublicas.Count == 0)
        {
            cachePublicas = CrearRecetasPublicasSemilla();
        }
        else
        {
            foreach (var r in cachePublicas)
                r.ImagenPlatillo = Receta.NormalizarRutaImagen(r.ImagenPlatillo, r.Titulo);
        }
        repoPublicoFeed.GuardarEnArchivos(cachePublicas);

        if (repoPrivado.ExisteArchivoJson())
        {
            cachePrivadas = repoPrivado.CargarDesdeArchivo();
        }

        if (cachePrivadas.Count == 0)
        {
            cachePrivadas = CrearRecetasPrivadasSemilla();
        }
        else
        {
            foreach (var r in cachePrivadas)
                r.ImagenPlatillo = Receta.NormalizarRutaImagen(r.ImagenPlatillo, r.Titulo);
        }
        repoPrivado.GuardarEnArchivos(cachePrivadas);

        // Marcar una receta inicial como favorita para demostración
        if (cachePublicas.Count > 0)
        {
            PerfilUsuarioNormal.MarcarFavorita(cachePublicas[0].ObtenerIdentificador());
        }
    }

    private static List<RecetaPublica> CrearRecetasPublicasSemilla()
    {
        return new List<RecetaPublica>
        {
            new RecetaPublica(
                idReceta: "PUB-101",
                idUsuarioAutor: "CHEF-01",
                titulo: "Pastelera de Choclo y Albahaca de Verano",
                categoria: "Plato Principal",
                tipoDieta: "Vegetariana",
                etiquetas: new List<string> { "Choclo", "Verano", "Tradicional", "SinCarne" },
                imagenPlatillo: "pastelera_choclo.jpg",
                tiempoPreparacion: 45,
                porciones: 6,
                dificultad: NivelDificultad.MEDIA,
                estacion: Estacion.VERANO,
                origen: Origen.CHILE,
                region: "Valle Central",
                ingredientes: new List<IngredienteReceta>
                {
                    new("Choclo Pastelero rallado", 6, UnidadMedida.UNIDADES, Estacion.VERANO, true, TipoVegetal.VERDURA),
                    new("Tomate Limachino en gajos", 3, UnidadMedida.UNIDADES, Estacion.VERANO, true, TipoVegetal.FRUTA),
                    new("Hojas de Albahaca fresca", 2, UnidadMedida.CUCHARADAS, Estacion.VERANO, true, TipoVegetal.VERDURA),
                    new("Leche tibia", 250, UnidadMedida.MILILITRO, Estacion.TODOELANHIO, false, TipoVegetal.NO_APLICA),
                    new("Sal de mar y pimienta", 1, UnidadMedida.ALGUSTO, Estacion.TODOELANHIO, false, TipoVegetal.NO_APLICA)
                },
                pasos: new List<PasoPreparacion>
                {
                    new(1, "Rallar o procesar los choclos pasteleros junto con las hojas de albahaca fresca.", 10),
                    new(2, "Cocinar a fuego suave revolviendo con cuchara de palo e incorporando la leche tibia poco a poco.", 20),
                    new(3, "Llevar a fuente de greda y gratinar al horno por 15 minutos; servir con ensalada de tomate limachino.", 15)
                },
                especialidadChef: "Cocina Patrimonial Chilena",
                verificadaPorChef: true,
                destacadaEnFeed: true,
                calificaciones: new List<Calificacion>
                {
                    new("Fernando Cocinero", 5, "¡Increíble textura cremosa! Con tomates de verano queda insuperable."),
                    new("Camila Huerta", 5, "Fácil de seguir paso a paso y muy aromática con la albahaca.")
                }),

            new RecetaPublica(
                idReceta: "PUB-102",
                idUsuarioAutor: "CHEF-01",
                titulo: "Crema Rostizada de Zapallo Camote y Naranja",
                categoria: "Sopas y Cremas",
                tipoDieta: "Vegana",
                etiquetas: new List<string> { "Zapallo", "Reconfortante", "Otoño", "Saludable" },
                imagenPlatillo: "crema_zapallo.jpg",
                tiempoPreparacion: 35,
                porciones: 4,
                dificultad: NivelDificultad.FACIL,
                estacion: Estacion.OTONIO,
                origen: Origen.ARGENTINA,
                region: "Mendoza",
                ingredientes: new List<IngredienteReceta>
                {
                    new("Zapallo Camote en cubos", 800, UnidadMedida.GRAMO, Estacion.OTONIO, true, TipoVegetal.VERDURA),
                    new("Jugo de Naranja natural", 150, UnidadMedida.MILILITRO, Estacion.INVIERNO, true, TipoVegetal.FRUTA),
                    new("Cebolla morada", 1, UnidadMedida.UNIDADES, Estacion.TODOELANHIO, true, TipoVegetal.VERDURA),
                    new("Aceite de oliva extra virgen", 2, UnidadMedida.CUCHARADAS, Estacion.TODOELANHIO, false, TipoVegetal.NO_APLICA)
                },
                pasos: new List<PasoPreparacion>
                {
                    new(1, "Hornear los cubos de zapallo camote y cebolla con aceite de oliva a 200°C hasta dorar.", 20),
                    new(2, "Licuar con caldo caliente y el toque cítrico de jugo de naranja fresco.", 10),
                    new(3, "Servir bien caliente decorado con semillas de zapallo tostadas.", 5)
                },
                especialidadChef: "Nutrición Basada en Plantas",
                verificadaPorChef: true,
                destacadaEnFeed: true,
                calificaciones: new List<Calificacion>
                {
                    new("Martín Gómez", 4, "El toque de naranja realza muchísimo el dulzor del zapallo.")
                }),

            new RecetaPublica(
                idReceta: "PUB-103",
                idUsuarioAutor: "CHEF-01",
                titulo: "Salteado Tibio de Brócoli, Espárragos y Limón",
                categoria: "Ensalada Tibia",
                tipoDieta: "Keto / Baja en Carbohidratos",
                etiquetas: new List<string> { "Brócoli", "Verde", "Rápida", "Antioxidante" },
                imagenPlatillo: "salteado_brocoli.jpg",
                tiempoPreparacion: 20,
                porciones: 2,
                dificultad: NivelDificultad.FACIL,
                estacion: Estacion.INVIERNO,
                origen: Origen.CHILE,
                region: "Sabana Central",
                ingredientes: new List<IngredienteReceta>
                {
                    new("Brócoli Fresco en arbolitos", 400, UnidadMedida.GRAMO, Estacion.INVIERNO, true, TipoVegetal.VERDURA),
                    new("Espárragos Verdes tiernos", 6, UnidadMedida.UNIDADES, Estacion.PRIMAVERA, true, TipoVegetal.VERDURA),
                    new("Almendras laminadas tostadas", 50, UnidadMedida.GRAMO, Estacion.TODOELANHIO, false, TipoVegetal.OTRO)
                },
                pasos: new List<PasoPreparacion>
                {
                    new(1, "Blanquear los arbolitos de brócoli y espárragos en agua hirviendo por 3 minutos.", 5),
                    new(2, "Saltear a fuego fuerte con ajo laminado, aceite de oliva y ralladura de limón.", 10),
                    new(3, "Emplatar con las almendras tostadas por encima.", 5)
                },
                especialidadChef: "Cocina Saludable de Estación",
                verificadaPorChef: true,
                destacadaEnFeed: false,
                calificaciones: new List<Calificacion>
                {
                    new("Lucía Pérez", 5, "Queda al dente y llena de color, ideal para una cena ligera.")
                }),

            new RecetaPublica(
                idReceta: "PUB-104",
                idUsuarioAutor: "USR-01",
                titulo: "Tartaleta Rústica de Frutillas y Maracuyá",
                categoria: "Postre de Estación",
                tipoDieta: "Vegetariana",
                etiquetas: new List<string> { "Frutilla", "Primavera", "Dulce", "Frutal" },
                imagenPlatillo: "tartaleta_frutilla.jpg",
                tiempoPreparacion: 40,
                porciones: 8,
                dificultad: NivelDificultad.MEDIA,
                estacion: Estacion.PRIMAVERA,
                origen: Origen.PERU,
                region: "Costa Central",
                ingredientes: new List<IngredienteReceta>
                {
                    new("Frutilla Dulce fresca", 500, UnidadMedida.GRAMO, Estacion.PRIMAVERA, true, TipoVegetal.FRUTA),
                    new("Pulpa de Maracuyá Dorado", 1, UnidadMedida.TAZAS, Estacion.PRIMAVERA, true, TipoVegetal.FRUTA),
                    new("Harina de avena y almendra", 2, UnidadMedida.TAZAS, Estacion.TODOELANHIO, false, TipoVegetal.OTRO)
                },
                pasos: new List<PasoPreparacion>
                {
                    new(1, "Hornear la base crujiente de avena por 15 minutos hasta que esté dorada.", 15),
                    new(2, "Rellenar con crema suave de maracuyá y coronar con frutillas frescas laminadas.", 25)
                },
                especialidadChef: "Repostería Casera",
                verificadaPorChef: false,
                destacadaEnFeed: false,
                calificaciones: new List<Calificacion>
                {
                    new("Chef Valentina Rojas", 5, "Excelente balance entre la acidez del maracuyá y el dulzor de la frutilla.")
                })
        };
    }

    private static List<RecetaPrivada> CrearRecetasPrivadasSemilla()
    {
        return new List<RecetaPrivada>
        {
            new RecetaPrivada(
                idReceta: "PRIV-001",
                idUsuarioAutor: "USR-01",
                titulo: "Tostadas Francesas de Durazno y Miel de Ulmo",
                categoria: "Desayuno / Brunch",
                tipoDieta: "Vegetariana",
                etiquetas: new List<string> { "Durazno", "Desayuno", "Pastel" },
                imagenPlatillo: "tostadas_durazno.jpg",
                tiempoPreparacion: 20,
                porciones: 2,
                dificultad: NivelDificultad.FACIL,
                estacion: Estacion.VERANO,
                origen: Origen.CHILE,
                region: "O'Higgins",
                ingredientes: new List<IngredienteReceta>
                {
                    new("Durazno maduro en láminas", 2, UnidadMedida.UNIDADES, Estacion.VERANO, true, TipoVegetal.FRUTA),
                    new("Pan brioche grueso", 4, UnidadMedida.UNIDADES, Estacion.TODOELANHIO, false, TipoVegetal.NO_APLICA),
                    new("Miel de ulmo y canela", 2, UnidadMedida.CUCHARADAS, Estacion.TODOELANHIO, false, TipoVegetal.NO_APLICA)
                },
                pasos: new List<PasoPreparacion>
                {
                    new(1, "Caramelizar los duraznos en sartén con una cucharada de miel y canela.", 8),
                    new(2, "Dorar el pan brioche por ambos lados y servir con los duraznos tibios encima.", 12)
                },
                notasPersonales: "Receta familiar de fin de semana. Probar también con manzanas en otoño.",
                soloDispositivoLocal: true),

            new RecetaPrivada(
                idReceta: "PRIV-002",
                idUsuarioAutor: "CHEF-01",
                titulo: "Ceviche Andino de Mango, Palta y Camote",
                categoria: "Entrada Fresca",
                tipoDieta: "Vegana",
                etiquetas: new List<string> { "Mango", "Palta", "Fresco", "Verano" },
                imagenPlatillo: "ceviche_mango.jpg",
                tiempoPreparacion: 25,
                porciones: 4,
                dificultad: NivelDificultad.FACIL,
                estacion: Estacion.VERANO,
                origen: Origen.PERU,
                region: "Piura",
                ingredientes: new List<IngredienteReceta>
                {
                    new("Mango Kent firme en cubos", 2, UnidadMedida.UNIDADES, Estacion.VERANO, true, TipoVegetal.FRUTA),
                    new("Palta Hass en cubos", 2, UnidadMedida.UNIDADES, Estacion.OTONIO, true, TipoVegetal.FRUTA),
                    new("Camote Morado glaseado", 300, UnidadMedida.GRAMO, Estacion.OTONIO, true, TipoVegetal.VERDURA),
                    new("Jugo de limón sutil y cilantro", 100, UnidadMedida.MILILITRO, Estacion.TODOELANHIO, true, TipoVegetal.FRUTA)
                },
                pasos: new List<PasoPreparacion>
                {
                    new(1, "Cortar el mango y la palta en cubos regulares de 1.5 cm.", 10),
                    new(2, "Emulsionar el jugo de limón sutil con cilantro picado, ají limo suave y sal.", 5),
                    new(3, "Mezclar suavemente y acompañar con medallones de camote morado tibio.", 10)
                },
                notasPersonales: "Lista para certificar y publicar en el Feed Oficial cuando esté en modo Chef.",
                soloDispositivoLocal: false)
        };
    }
}
