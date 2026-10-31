# Mesa de Ayuda — Tickets de Soporte Técnico

Aplicación web (Blazor Server, C#) para registrar y dar seguimiento a solicitudes de soporte técnico de una tienda con dos sucursales. Los tickets se guardan **en memoria** mientras la aplicación está en ejecución.

**Autores:** Snyder Diaz, Miguel Saldaña

## Documentación del proyecto

- [`hito-0.md`](hito-0.md): propuesta y modelo inicial.
- [`hito-1.md`](hito-1.md): cambios respecto del Hito 0, correcciones y decisiones de diseño.

## Requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) o superior (el proyecto apunta a `net8.0`; un SDK más reciente también lo compila).
- Un navegador web actual.
- No requiere base de datos ni conexión a internet para ejecutarse (solo para restaurar paquetes NuGet la primera vez).

## Pasos de ejecución

Desde la carpeta principal del repositorio:

```bash
dotnet run --project MesaDeAyuda.Web
```

Abra en el navegador la dirección que aparece en la consola (por defecto `http://localhost:5080`).

La aplicación arranca con tres tickets de muestra (datos ficticios) para poder demostrar el flujo sin escribir código.

### Ejecutar las pruebas

```bash
dotnet test MesaDeAyuda.Tests
```

## Funciones disponibles

Todo se hace desde una sola pantalla:

1. **Registrar ticket**: título, descripción, solicitante y prioridad (Baja, Media, Alta; por defecto Media). El ticket recibe un número correlativo y nace en estado *Abierto*. Si hay errores, no se crea nada, se muestra bajo cada campo qué corregir y lo escrito se conserva.
2. **Listar y filtrar**: la lista muestra número, título, prioridad, estado y fecha, con los más recientes primero. Se puede filtrar por estado (Todos, Abierto, En proceso, Resuelto, Cerrado) y buscar por número.
3. **Consultar**: con *Ver* (o buscando por número) se muestra el detalle completo. Al elegir otro ticket, el detalle cambia. Consultar no modifica datos.
4. **Cambiar estado**: con los botones del detalle. El nuevo estado se refleja de inmediato en el detalle, en la lista y en los contadores, sin tocar los demás tickets.
5. **Cantidades**: total, abiertos y cerrados (y también en proceso y resueltos), actualizadas al registrar o cambiar estado.

### Flujo de estados permitido

```
Abierto → En proceso → Resuelto → Cerrado
                ↑          │
                └──────────┘   (si el problema reaparece)
```

*Cerrado* es un estado final: un ticket cerrado no puede cerrarse otra vez ni cambiar a otro estado.

### Reglas de validación

| Campo | Regla |
|---|---|
| Título | Obligatorio, 5 a 80 caracteres |
| Descripción | Obligatoria, 10 a 500 caracteres |
| Solicitante | Obligatorio, 3 a 60 caracteres |
| Prioridad | Debe ser Baja, Media o Alta |

## Estructura del repositorio

```
MesaDeAyuda.Web/        Aplicación Blazor
  Models/               Ticket, enumeraciones y RepositorioTickets
  Components/Pages/     Pantalla principal (Tickets.razor)
MesaDeAyuda.Tests/      Pruebas unitarias (xUnit)
hito-0.md, hito-1.md    Documentación de los hitos
```

## Limitaciones conocidas

- **Sin persistencia:** los tickets viven en memoria; al detener la aplicación se pierden y se vuelve a los datos de muestra.
- **Una sola colección compartida:** todas las pestañas y usuarios conectados ven los mismos tickets.
- **Sin usuarios ni autenticación:** el registro lo hace el técnico; no hay inicio de sesión.
- **No hay edición ni eliminación** de tickets ya registrados (solo cambio de estado).
- No hay inventario de equipos ni historial de piezas (fuera del alcance definido en el Hito 0).
