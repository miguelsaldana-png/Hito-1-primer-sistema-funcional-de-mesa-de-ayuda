namespace MesaDeAyuda.Web.Models;

/// <summary>
/// Colección de tickets en memoria. Asigna identificadores correlativos,
/// busca por Id, lista con filtro por estado y cuenta tickets.
/// </summary>
public class RepositorioTickets
{
    private readonly List<Ticket> _tickets = new();
    private readonly object _candado = new();
    private int _siguienteId = 1;

    /// <summary>Crea y guarda un ticket. Si los datos son inválidos lanza ArgumentException y no consume un Id.</summary>
    public Ticket Agregar(string? titulo, string? descripcion, string? solicitante, PrioridadTicket prioridad)
    {
        lock (_candado)
        {
            var ticket = new Ticket(_siguienteId, titulo, descripcion, solicitante, prioridad);
            _siguienteId++;
            _tickets.Add(ticket);
            return ticket;
        }
    }

    public Ticket? ObtenerPorId(int id)
    {
        lock (_candado)
        {
            return _tickets.FirstOrDefault(t => t.Id == id);
        }
    }

    /// <summary>Tickets (más recientes primero). Con <paramref name="estado"/> nulo devuelve todos.</summary>
    public IReadOnlyList<Ticket> Listar(EstadoTicket? estado = null)
    {
        lock (_candado)
        {
            return _tickets
                .Where(t => estado is null || t.Estado == estado)
                .OrderByDescending(t => t.FechaCreacion)
                .ThenByDescending(t => t.Id)
                .ToList();
        }
    }

    /// <summary>Cantidad de tickets. Con <paramref name="estado"/> nulo cuenta el total.</summary>
    public int Contar(EstadoTicket? estado = null)
    {
        lock (_candado)
        {
            return estado is null ? _tickets.Count : _tickets.Count(t => t.Estado == estado);
        }
    }

    /// <summary>Datos ficticios para la demostración. No identifican a personas ni solicitudes reales.</summary>
    public void CargarDatosDeMuestra()
    {
        Agregar("Lector de códigos no lee en caja 2",
                "Lee algunos productos y otros no desde esta mañana.",
                "Cajero de muestra", PrioridadTicket.Media);

        Agregar("Mouse de la oficina falla de forma intermitente",
                "El cursor se congela por unos segundos. Puede esperar.",
                "Administrativo de muestra", PrioridadTicket.Baja);

        var caja = Agregar("Caja 1 no abre el sistema de cobro",
                "Al iniciar el sistema aparece una pantalla en blanco y no se puede cobrar.",
                "Encargado de muestra", PrioridadTicket.Alta);
        caja.CambiarEstado(EstadoTicket.EnProceso);
    }
}
