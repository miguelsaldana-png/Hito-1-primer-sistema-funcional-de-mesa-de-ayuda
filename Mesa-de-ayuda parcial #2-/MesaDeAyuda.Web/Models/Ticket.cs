namespace MesaDeAyuda.Web.Models;

/// <summary>Solicitud de soporte técnico. Mantiene encapsuladas sus reglas.</summary>
public class Ticket
{
    public const int TituloMin = 5, TituloMax = 80;
    public const int DescripcionMin = 10, DescripcionMax = 500;
    public const int SolicitanteMin = 3, SolicitanteMax = 60;
    public const string CampoTitulo = "Titulo", CampoDescripcion = "Descripcion",
        CampoSolicitante = "Solicitante", CampoPrioridad = "Prioridad";

    private static readonly Dictionary<EstadoTicket, EstadoTicket[]> Transiciones = new()
    {
        [EstadoTicket.Abierto] = new[] { EstadoTicket.EnProceso },
        [EstadoTicket.EnProceso] = new[] { EstadoTicket.Resuelto },
        [EstadoTicket.Resuelto] = new[] { EstadoTicket.Cerrado, EstadoTicket.EnProceso },
        [EstadoTicket.Cerrado] = Array.Empty<EstadoTicket>()
    };

    public int Id { get; private set; }
    public string Titulo { get; private set; }
    public string Descripcion { get; private set; }
    public string Solicitante { get; private set; }
    public PrioridadTicket Prioridad { get; private set; }
    public EstadoTicket Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaActualizacion { get; private set; }

    public Ticket(int id, string? titulo, string? descripcion, string? solicitante, PrioridadTicket prioridad)
    {
        var errores = Validar(titulo, descripcion, solicitante, prioridad);
        if (errores.Count > 0) throw new ArgumentException(string.Join(" ", errores.Values));
        Id = id;
        Titulo = titulo!.Trim(); Descripcion = descripcion!.Trim(); Solicitante = solicitante!.Trim();
        Prioridad = prioridad; Estado = EstadoTicket.Abierto;
        FechaCreacion = FechaActualizacion = DateTime.Now;
    }

    // Constructor usado únicamente para reconstruir un ticket leído de MySQL.
    public static Ticket Restaurar(int id, string titulo, string descripcion, string solicitante,
        PrioridadTicket prioridad, EstadoTicket estado, DateTime creado, DateTime actualizado)
    {
        var ticket = new Ticket(id, titulo, descripcion, solicitante, prioridad);
        if (!Enum.IsDefined(estado)) throw new InvalidOperationException("Estado almacenado no válido.");
        ticket.Estado = estado; ticket.FechaCreacion = creado; ticket.FechaActualizacion = actualizado;
        return ticket;
    }

    public static Dictionary<string, string> Validar(string? titulo, string? descripcion,
        string? solicitante, PrioridadTicket prioridad)
    {
        var errores = new Dictionary<string, string>();
        var t = (titulo ?? "").Trim();
        var d = (descripcion ?? "").Trim();
        var s = (solicitante ?? "").Trim();
        if (t.Length < TituloMin || t.Length > TituloMax) errores[CampoTitulo] = $"El título debe tener entre {TituloMin} y {TituloMax} caracteres.";
        if (d.Length < DescripcionMin || d.Length > DescripcionMax) errores[CampoDescripcion] = $"La descripción debe tener entre {DescripcionMin} y {DescripcionMax} caracteres.";
        if (s.Length < SolicitanteMin || s.Length > SolicitanteMax) errores[CampoSolicitante] = s.Length < SolicitanteMin ? $"Escriba el nombre de quien reporta (mín. {SolicitanteMin} letras)." : $"El nombre de quien reporta no puede superar {SolicitanteMax} caracteres.";
        if (!Enum.IsDefined(prioridad)) errores[CampoPrioridad] = "Seleccione una prioridad válida.";
        return errores;
    }

    public void ActualizarDatos(string? titulo, string? descripcion, string? solicitante, PrioridadTicket prioridad)
    {
        if (Estado == EstadoTicket.Cerrado) throw new InvalidOperationException("No se puede editar un ticket cerrado.");
        var errores = Validar(titulo, descripcion, solicitante, prioridad);
        if (errores.Count > 0) throw new ArgumentException(string.Join(" ", errores.Values));
        Titulo = titulo!.Trim(); Descripcion = descripcion!.Trim(); Solicitante = solicitante!.Trim();
        Prioridad = prioridad; FechaActualizacion = DateTime.Now;
    }

    public bool PuedeCambiarA(EstadoTicket nuevo) => Transiciones[Estado].Contains(nuevo);

    public bool TryCambiarEstado(EstadoTicket nuevo, out string mensaje)
    {
        if (!Enum.IsDefined(nuevo)) { mensaje = "El estado indicado no es válido."; return false; }
        if (Estado == EstadoTicket.Cerrado) { mensaje = $"El ticket #{Id} está Cerrado y no admite más cambios."; return false; }
        if (nuevo == Estado) { mensaje = $"El ticket #{Id} ya está {Estado.Texto()}."; return false; }
        if (!PuedeCambiarA(nuevo))
        {
            var permitidos = string.Join(" o ", Transiciones[Estado].Select(e => e.Texto()));
            mensaje = $"No se puede pasar de {Estado.Texto()} a {nuevo.Texto()}. Desde este estado solo puede pasar a: {permitidos}.";
            return false;
        }
        Estado = nuevo; FechaActualizacion = DateTime.Now;
        mensaje = $"Ticket #{Id} ahora está {nuevo.Texto()}"; return true;
    }

    public void CambiarEstado(EstadoTicket nuevo)
    {
        if (!TryCambiarEstado(nuevo, out var mensaje)) throw new InvalidOperationException(mensaje);
    }
}
