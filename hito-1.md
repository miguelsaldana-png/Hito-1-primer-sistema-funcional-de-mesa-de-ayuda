# Hito 1: Primer sistema funcional

**Proyecto:** Mesa de Ayuda (Tickets de Soporte Técnico)
**Autores:** Snyder Diaz, Miguel Saldaña

> No se repite la descripción del problema (ver [`hito-0.md`](hito-0.md)).

## 1. Qué se implementó

Aplicación Blazor Server (.NET 8) con las cuatro acciones del Hito 0 integradas en una sola pantalla: registrar, encontrar en la lista, consultar y cambiar estado. Los datos están en memoria, sin modificar código para la demostración (la aplicación arranca con 3 tickets de muestra ficticios).

Mapa de requisitos del enunciado:

| Requisito | Dónde se cumple |
|---|---|
| Registro con Id y estado inicial Abierto | `RepositorioTickets.Agregar` asigna Id correlativo; el constructor de `Ticket` fija `Estado = Abierto` |
| Listado | Tabla en `Tickets.razor` con ID, título, prioridad, estado y fecha |
| Consulta de un ticket | Botón *Ver* / búsqueda por número → panel de detalle |
| Cambio de estado | `Ticket.TryCambiarEstado`; la lista y el detalle leen el mismo objeto, así que ambos muestran el cambio al instante |
| Validación del registro | `Ticket.Validar` devuelve un mensaje por campo (título, descripción, solicitante, prioridad) |
| Consulta por estado | Selector de estado; solo cambia lo que se muestra, no los datos |
| Cantidades actualizadas | Total / Abiertos / Cerrados (+ En proceso y Resueltos) calculados con `Repositorio.Contar` en cada render |
| Conservación durante el uso | `RepositorioTickets` registrado como *singleton* |

## 2. Cambios respecto del Hito 0

| # | Cambio | Motivo |
|---|---|---|
| 1 | Se agregaron **contadores** (total, abiertos, cerrados, en proceso, resueltos). | Requisito nuevo del Hito 1; en el Hito 0 solo se mencionaba la necesidad de saber cuántos tickets hay pendientes. |
| 2 | Se agregó la regla de validación de **prioridad no permitida** (`Enum.IsDefined`). | El enunciado pide impedir prioridades fuera del conjunto permitido. En la interfaz se usa un selector, pero el modelo no confía en la interfaz. |
| 3 | La prioridad se elige con un **selector desplegable** en vez de botones de radio. | Más simple de enlazar en Blazor sin `EditForm`; mismo conjunto de valores y mismo valor por defecto (Media). |
| 4 | **Buscar por número** abre el detalle del ticket; ya no filtra la lista. | Evita dos mecanismos distintos para lo mismo. La lista solo se filtra por estado; la búsqueda lleva directo al detalle (sección 2.3 del Hito 0). Los mensajes de error se conservan: "Escriba un número válido." y "No existe el ticket #N.". |
| 5 | El mensaje de transición no permitida ahora es **genérico** e indica los destinos válidos (ej. "No se puede pasar de Abierto a Cerrado. Desde Abierto solo puede pasar a: En proceso."). | Una sola regla cubre todas las combinaciones inválidas en lugar de un texto escrito a mano para un caso. |
| 6 | Se agregaron mensajes específicos para **Cerrado → Cerrado** ("ya está Cerrado y no puede cerrarse otra vez") y para repetir el estado actual. | Requisito explícito: un ticket cerrado no debe poder cerrarse otra vez. |
| 7 | Se agregaron **datos de muestra** ficticios al iniciar. | Permite demostrar el flujo sin código ni escritura previa. No identifican a personas ni solicitudes reales. |

## 3. Correcciones al Hito 0

- **Mensaje de éxito inconsistente:** la regla 1 decía "Ticket registrado correctamente" y la sección 2.1 "Ticket #5 registrado correctamente". Se unificó en `Ticket #N registrado correctamente`.
- **Boceto de interfaz:** el boceto mostraba solo ID, título, estado y acción en la lista, mientras que la sección 2.2 pedía también prioridad y fecha. La implementación sigue la sección 2.2.
- **Pregunta abierta sobre persistencia:** el enunciado del Hito 1 pide datos en memoria, así que se mantiene el `RepositorioTickets` en memoria. Está aislado en una sola clase, de modo que una versión con archivos o base de datos podría reemplazarlo sin cambiar `Ticket` ni la pantalla.

## 4. Decisiones de diseño y cómo cumplen las reglas del ticket

1. **`Ticket` protege sus propias reglas.** Las propiedades tienen `set` privado (o solo se asignan en el constructor) y el constructor llama a `Validar`; un ticket inválido no puede existir. Así, ninguna pantalla ni otra clase puede saltarse las reglas 1 a 3.
2. **El estado solo cambia por `TryCambiarEstado`.** Esta es la única vía para modificar `Estado`, y consulta una tabla de transiciones permitidas (regla 4). Cerrado no tiene destinos, por lo que un ticket cerrado no puede cerrarse otra vez.
3. **`Validar` es estático y devuelve un diccionario campo → mensaje.** Permite que la interfaz valide *antes* de crear nada, muestre un mensaje bajo cada campo y conserve lo escrito, sin duplicar las reglas en la interfaz.
4. **`TryCambiarEstado` devuelve `bool` + mensaje en vez de lanzar excepción.** Un cambio no permitido es un caso normal de uso, no un error de programa. `CambiarEstado` existe como variante que lanza `InvalidOperationException` para el código que sí espera que el cambio sea válido (por ejemplo, los datos de muestra).
5. **`RepositorioTickets` es el único dueño de la colección y de los Id.** El Id se incrementa solo si el ticket se creó con éxito, así que un registro inválido no deja "huecos" en la numeración.
6. **Detalle y lista leen el mismo objeto `Ticket`.** Por eso un cambio de estado se ve a la vez en ambos lugares y no afecta a los demás tickets. Los contadores se calculan desde el repositorio en cada render, nunca se guardan aparte, así no se desincronizan.
7. **El filtro por estado es solo una consulta.** `Listar(estado)` crea una lista nueva y no toca la colección, por lo que cambiar el filtro no borra ni modifica tickets.
8. **Pruebas unitarias antes que pantallas** (como se anticipó en el Hito 0): la carpeta `MesaDeAyuda.Tests` cubre validaciones en sus límites, todas las transiciones permitidas y no permitidas, y el comportamiento del repositorio (Id, filtros, conteos).

> Cada integrante debe poder explicar estas decisiones con sus propias palabras durante la presentación.

## 5. Recorrido de demostración (5 min)

1. Abrir la aplicación: se ven 3 tickets de muestra y los contadores (Total 3, Abiertos 2, Cerrados 0).
2. Intentar **Guardar** con el formulario vacío → mensajes de error bajo cada campo.
3. Registrar un ticket válido → "Ticket #4 registrado correctamente"; aparece primero en la lista y Total pasa a 4.
4. Pulsar **Ver** en distintos tickets → el detalle cambia.
5. En un ticket Abierto pulsar **Cerrado** → mensaje de error en rojo y el estado no cambia.
6. Llevar un ticket a En proceso → Resuelto → Cerrado; ver el estado actualizado en detalle y lista, y los contadores.
7. Pulsar **Cerrado** otra vez en ese ticket → se rechaza.
8. Filtrar por Abierto / Cerrado / Todos → los tickets no se alteran.
9. Buscar por `abc` y por `99` → mensajes de error.

## 6. Limitaciones conocidas

Ver la sección correspondiente del [`README.md`](README.md).

## 7. Contribución de cada integrante

> Completar antes de la entrega con enlaces a commits y pull requests propios.

| Integrante | Contribución | Commits / Pull requests |
|---|---|---|
| Snyder Diaz | | |
| Miguel Saldaña | | |
