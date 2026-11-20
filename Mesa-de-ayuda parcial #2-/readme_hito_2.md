# Mesa de Ayuda — Hito 2: Persistencia de datos

Aplicación web de mesa de ayuda desarrollada en C#, Blazor Server, .NET 10 y MySQL. Permite registrar y consultar tickets de soporte, editarlos, cambiar su estado, eliminarlos con confirmación, filtrar resultados y visualizar cantidades globales. Los datos se almacenan en MySQL y se conservan al cerrar y reiniciar la aplicación.

**Autores:** Snyder Diaz y Miguel Saldaña.

## Requisitos

* .NET SDK 10.0 (`net10.0`).
* MySQL Server 8.0 o compatible (local o alojado externamente).
* Visual Studio 2022 / 2026 o editor compatible con .NET 10.
* Navegador web moderno.

## 1. Ubicación y ejecución del script de base de datos

El script SQL obligatorio se encuentra en:

* `database/crear_base_datos.sql`

Para crear la base de datos y las tablas:

1. Inicie su servidor MySQL.
2. Abra MySQL Workbench, phpMyAdmin o la consola de MySQL:
   ```bash
   mysql -u TU_USUARIO -p < database/crear_base_datos.sql
   ```
3. Ejecute el archivo `database/crear_base_datos.sql` completo.
4. Verifique que exista la base de datos `mesa_ayuda` junto con su estructura y datos iniciales de demostración.

## 2. Configuración de la conexión (sin revelar credenciales)

Por seguridad, el repositorio no contiene contraseñas reales. En `MesaDeAyuda.Web/appsettings.json` se mantiene únicamente una plantilla de referencia:

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Port=3306;Database=mesa_ayuda;User ID=TU_USUARIO;Password=TU_CONTRASENA;"
  }
}
```

Para ejecutar la aplicación localmente sin exponer credenciales en GitHub, configure la cadena de conexión mediante **User Secrets** (en Visual Studio o consola) o mediante **variables de entorno**:

### Opción A: User Secrets (recomendado)

Desde Visual Studio:
1. Haga clic derecho sobre el proyecto `MesaDeAyuda.Web` en el Explorador de soluciones.
2. Seleccione **Administrar secretos de usuario**.
3. Configure en `secrets.json` la sección `ConnectionStrings:MySql` con sus credenciales locales.

O desde la terminal, dentro de la carpeta `MesaDeAyuda.Web`:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:MySql" "Server=localhost;Port=3306;Database=mesa_ayuda;User ID=TU_USUARIO;Password=TU_CONTRASENA_REAL;"
```

### Opción B: Variable de entorno

En PowerShell, antes de iniciar la aplicación:

```powershell
$env:ConnectionStrings__MySql="Server=localhost;Port=3306;Database=mesa_ayuda;User ID=TU_USUARIO;Password=TU_CONTRASENA_REAL;"
```

## 3. Ejecución de la aplicación

Desde la carpeta raíz del repositorio:

```bash
dotnet restore
dotnet build
dotnet run --project MesaDeAyuda.Web
```

Abra en el navegador la dirección HTTP o HTTPS que muestre la consola.

## Funciones implementadas

1. **Registrar y consultar:** crea tickets con título, descripción, solicitante y prioridad, asignando identificador único automático y estado inicial `Abierto`. Permite seleccionar cualquier ticket en el listado para ver su detalle completo.
2. **Editar:** modifica los datos de un ticket existente y actualiza el listado y el detalle sin duplicar registros ni alterar otros tickets. Los tickets cerrados no pueden editarse.
3. **Cerrar:** aplica las reglas de transición del modelo (`Abierto` → `En proceso` → `Resuelto` → `Cerrado`) e impide volver a cerrar un ticket que ya se encuentra en estado `Cerrado`.
4. **Eliminar con confirmación:** solicita confirmación antes de eliminar; al cancelar conserva el ticket intacto y al confirmar elimina únicamente el seleccionado.
5. **Información relacionada y filtros:** muestra información significativa de prioridad y estado, y permite filtrar por estado sin modificar ni borrar registros, informando cuando no existen coincidencias y permitiendo volver al listado completo.
6. **Cantidades globales:** muestra el total de tickets, los abiertos y los cerrados del conjunto completo, manteniéndose actualizados tras cada operación incluso cuando hay un filtro activo.
7. **Validación y manejo de errores:** rechaza títulos (5–80 caracteres), descripciones (10–500), solicitantes (3–60) o prioridades inválidas indicando qué corregir, y distingue entre operaciones exitosas, ausencia de resultados y fallos de ejecución/conexión.
8. **Persistencia:** todos los registros, ediciones, cambios de estado y eliminaciones se conservan en MySQL tras reiniciar la aplicación.

## Organización del código

* `MesaDeAyuda.Web/Components/Pages/Tickets.razor`: interfaz Blazor, manejo de estado visual y eventos de usuario.
* `MesaDeAyuda.Web/Models/Ticket.cs`: entidad de dominio con encapsulamiento, validaciones y reglas de transición de estado.
* `MesaDeAyuda.Web/Models/RepositorioTickets.cs`: acceso a datos en MySQL mediante ADO.NET (`MySqlConnector`) con comandos parametrizados.
* `MesaDeAyuda.Web/Models/Enumeraciones.cs`: definiciones de estados y prioridades.
* `MesaDeAyuda.Tests/`: pruebas unitarias del modelo y comportamiento del repositorio.
* `database/crear_base_datos.sql`: script de creación de base de datos, tablas y datos de muestra.

## Seguridad y decisiones técnicas

* **Acceso a datos parametrizado:** todas las consultas y comandos SQL utilizan parámetros (`MySqlParameter`) en lugar de concatenar valores de entrada, previniendo inyección SQL.
* **Protección de credenciales:** la cadena de conexión real se administra fuera del repositorio mediante User Secrets o variables de entorno.
* **Encapsulamiento:** `Ticket` protege su estado interno mediante propiedades de escritura privada y métodos controlados (`Validar`, `Editar`, `TryCambiarEstado`).
* **Separación de responsabilidades:** la interfaz Blazor delega las reglas de negocio al modelo y la persistencia al repositorio.

## Limitaciones conocidas

* La aplicación no incluye autenticación ni control de acceso por roles.
* Requiere que el servicio de MySQL esté activo y accesible con la cadena de conexión configurada.
* La confirmación de eliminación depende del diálogo de confirmación del navegador.