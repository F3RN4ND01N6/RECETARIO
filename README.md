Recetario APP

Una aplicacion desarrollada con .NET MAUI diseñada para gestionar, compartir y descubrir recetas de cocina. El proyecto
incluye un enfoque Offline-First, gestionando la persistencia de datos de forma local mediante archivos JSON.

CARACTERISTICAS PIRNCIPALES
- Gestion Integral de Recetas (CRUD):
  - Feed Público: Explora recetas compartidas por la comunidad (FeedPage).
  - Mis Recetas: Panel personal para gestionar tus propias creaciones (MisRecetasPage).
  - Creación y Edición: Interfaz intuitiva para añadir y modificar recetas (CrearEditarRecetaPage).
- Filtros Estacionales: Descubre recetas basadas en ingredientes de temporada mediante la vista TemporadaPage.
- Sistema de Autenticación y Roles: Manejo de sesiones diferenciando entre Usuario Normales y Administradores (Mediante Código
  de verificación).
- Base de Datos Local JSON: Motor de persistencia propio (PersistenciaJsonService) que almacena de forma segura usuarios,
  credenciales y recetas de forma categorizada.
- Arquitectura Modular: Uso de componenetes UI reutilizables (como la BarraInferiorView).

TECNOLOGÍA UTILIZADAS
-  Framework: .NET MAUI
-  Lenguaje: C# / XAML
-  Almacenamiento: Archivos locales estructurados en formato JSON.
-  Plataformas Soportadas: Windows

ESTRUCTURA DEL PROYECTO
- /BaseDatos_RecetasApp: Contiene los Archivos JSON que actuan como base de datos (Feed público, recetas privadas, usuarios
  , codigos admin).
- /Credenciales: Almacenamiento local de credenciales separadas por roles.
- /Models: Modelos de datos (Recetas, Usuarios, ComponentesReceta, Enums).
- /Services: Lógica de negocio y persistencia (AuthService, RecetaService, PersistenciaJsonService).
- /Views: Interface de usuario en XAML y su lógica asociada.

SOLUCIÓN A PROBLEMAS COMUNES (TROUBLESHOOTING)

Error de versionado al compilar o generar el paquete
Durante el desarrollo de este proyecto, es posible encontrarse con un error relacionado con la configuración de las versiones
al intentar compilar o empaquetar la aplicación.

SOLUCIÓN: 
El problema se soluciona modificando la configuración del proyecto (Recetario.csproj). Tuvimos que añadir manualmente las
etiquetas <PackageVersion> y <Version> fijandolas en la version 2.0.

INTALACIÓN Y USO
1. Asegúrate de tener instalado Visual Studio 2022 (o superior) con la carga de trabajo de Desarrollo de la interfaz de
   usuario de la  aplicacion .NET MAUI
2. Clona este repositorio
3. Abre el archivo de solucion Recetaroi.sln en Visual Studio.
4. Si experimentas el error de versiones mensionado en la sección anterior, asegurate de aplicar el parche en el archivo .csproj.
   ademas de cargar nuevamente los packetes Nuget en el mismo apartado realizndo un click dercho sobre el archivo y recargar paquetes Nuget.
6. Ejecuta la aplicacion.
