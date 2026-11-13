namespace MesaDeAyuda.Web.Models;

/// <summary>
/// Solicitud de soporte técnico. Encapsula sus propias reglas de negocio:
/// valida sus datos al crearse y controla qué cambios de estado son permitidos.
/// </summary>
public class Ticket
{
    public const int TituloMin = 5, TituloMax = 80;
    public const int DescripcionMin = 10, DescripcionMax = 500;
    public const int SolicitanteMin = 3, SolicitanteMax = 60;

    public const string CampoTitulo = "Titulo";
    public const string CampoDescripcion = "Descripcion";
    public const string CampoSolicitante = "Solicitante";
    public const string CampoPrioridad = "Prioridad";

    // Flujo permitido (Hito 0). Cerrado es un estado final.
    private static readonly Dictionary<EstadoTicket, EstadoTicket[]> Transiciones = new()
    {
        [EstadoTicket.Abierto] = new[] { EstadoTicket.EnProceso },
        [EstadoTicket.EnProceso] = new[] { EstadoTicket.Resuelto },
        [EstadoTicket.Resuelto] = new[] { EstadoTicket.Cerrado, EstadoTicket.EnProceso },
        [EstadoTicket.Cerrado] = Array.Empty<EstadoTicket>()
    };

    public int Id { get; }
    public string Titulo { get; }
    public string Descripcion { get; }
    public string Solicitante { get; }
    public PrioridadTicket Prioridad { get; }
    public EstadoTicket Estado { get; private set; }
    public DateTime FechaCreacion { get; }
    public DateTime FechaActualizacion { get; private set; }

    /// <exception cref="ArgumentException">Si algún dato no cumple las reglas de validación.</exception>
    public Ticket(int id, string? titulo, string? descripcion, string? solicitante, PrioridadTicket prioridad)
    {
        var errores = Validar(titulo, descripcion, solicitante, prioridad);
        if (errores.Count > 0)
            throw new ArgumentException(string.Join(" ", errores.Values));

        Id = id;
        Titulo = titulo!.Trim();
        Descripcion = descripcion!.Trim();
        Solicitante = solicitante!.Trim();
        Prioridad = prioridad;
        Estado = EstadoTicket.Abierto;
        FechaCreacion = DateTime.Now;
        FechaActualizacion = FechaCreacion;
    }

    /// <summary>
    /// Valida los datos de un ticket sin crearlo. Devuelve un diccionario campo -> mensaje
    /// (vacío si todo es válido), para poder mostrar bajo cada campo qué corregir.
    /// </summary>
    public static Dictionary<string, string> Validar(
        string? titulo, string? descripcion, string? solicitante, PrioridadTicket prioridad)
    {
        var errores = new Dictionary<string, string>();

        var t = (titulo ?? "").Trim();
        if (t.Length < TituloMin || t.Length > TituloMax)
            errores[CampoTitulo] = $"El título debe tener entre {TituloMin} y {TituloMax} caracteres.";

        var d = (descripcion ?? "").Trim();
        if (d.Length < DescripcionMin || d.Length > DescripcionMax)
            errores[CampoDescripcion] = $"La descripción debe tener entre {DescripcionMin} y {DescripcionMax} caracteres.";

        var s = (solicitante ?? "").Trim();
        if (s.Length < SolicitanteMin)
            errores[CampoSolicitante] = $"Escriba el nombre de quien reporta (mín. {SolicitanteMin} letras).";
        else if (s.Length > SolicitanteMax)
            errores[CampoSolicitante] = $"El nombre de quien reporta no puede superar {SolicitanteMax} caracteres.";

        if (!Enum.IsDefined(prioridad))
            errores[CampoPrioridad] = "La prioridad no es válida. Elija Baja, Media o Alta.";

        return errores;
    }

    public bool PuedeCambiarA(EstadoTicket nuevo) => Transiciones[Estado].Contains(nuevo);

    /// <summary>
    /// Intenta cambiar el estado. Si el cambio no está permitido, el ticket no se modifica
    /// y <paramref name="mensaje"/> explica el motivo.
    /// </summary>
    public bool TryCambiarEstado(EstadoTicket nuevo, out string mensaje)
    {
        if (!Enum.IsDefined(nuevo))
        {
            mensaje = "El estado indicado no es válido.";
            return false;
        }

        if (Estado == EstadoTicket.Cerrado)
        {
            mensaje = nuevo == EstadoTicket.Cerrado
                ? $"El ticket #{Id} ya está Cerrado y no puede cerrarse otra vez."
                : $"El ticket #{Id} está Cerrado y no admite más cambios.";
            return false;
        }

        if (nuevo == Estado)
        {
            mensaje = $"El ticket #{Id} ya está {Estado.Texto()}.";
            return false;
        }

        if (!PuedeCambiarA(nuevo))
        {
            var permitidos = string.Join(" o ", Transiciones[Estado].Select(e => e.Texto()));
            mensaje = $"No se puede pasar de {Estado.Texto()} a {nuevo.Texto()}. " +
                      $"Desde {Estado.Texto()} solo puede pasar a: {permitidos}.";
            return false;
        }

        Estado = nuevo;
        FechaActualizacion = DateTime.Now;
        mensaje = $"Ticket #{Id} ahora está {nuevo.Texto()}";
        return true;
    }

    /// <exception cref="InvalidOperationException">Si la transición no está permitida.</exception>
    public void CambiarEstado(EstadoTicket nuevo)
    {
        if (!TryCambiarEstado(nuevo, out var mensaje))
            throw new InvalidOperationException(mensaje);
    }
}
