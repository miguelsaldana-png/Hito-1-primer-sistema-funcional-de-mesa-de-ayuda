using MySqlConnector;

namespace MesaDeAyuda.Web.Models;

/// <summary>Acceso a datos MySQL para las operaciones CRUD de tickets.</summary>
public class RepositorioTickets
{
    private readonly string _conexion;
    public RepositorioTickets(IConfiguration configuration) =>
        _conexion = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:MySql en appsettings.json.");

    private MySqlConnection CrearConexion() => new(_conexion);

    public List<Ticket> Listar(EstadoTicket? estado = null)
    {
        var resultado = new List<Ticket>();
        using var cn = CrearConexion(); cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT id,titulo,descripcion,solicitante,prioridad,estado,fecha_creacion,fecha_actualizacion FROM tickets WHERE (@estado IS NULL OR estado=@estado) ORDER BY fecha_creacion DESC,id DESC";
        cmd.Parameters.AddWithValue("@estado", estado?.ToString());
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) resultado.Add(LeerTicket(rd));
        return resultado;
    }

    public Ticket? ObtenerPorId(int id)
    {
        using var cn = CrearConexion(); cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT id,titulo,descripcion,solicitante,prioridad,estado,fecha_creacion,fecha_actualizacion FROM tickets WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? LeerTicket(rd) : null;
    }

    private static Ticket LeerTicket(MySqlDataReader rd) => Ticket.Restaurar(
        rd.GetInt32("id"), rd.GetString("titulo"), rd.GetString("descripcion"), rd.GetString("solicitante"),
        Enum.Parse<PrioridadTicket>(rd.GetString("prioridad")), Enum.Parse<EstadoTicket>(rd.GetString("estado")),
        rd.GetDateTime("fecha_creacion"), rd.GetDateTime("fecha_actualizacion"));

    public Ticket Agregar(string? titulo, string? descripcion, string? solicitante, PrioridadTicket prioridad)
    {
        var ticket = new Ticket(0, titulo, descripcion, solicitante, prioridad);
        using var cn = CrearConexion(); cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "INSERT INTO tickets(titulo,descripcion,solicitante,prioridad,estado,fecha_creacion,fecha_actualizacion) VALUES(@titulo,@descripcion,@solicitante,@prioridad,'Abierto',NOW(),NOW())";
        cmd.Parameters.AddWithValue("@titulo", ticket.Titulo); cmd.Parameters.AddWithValue("@descripcion", ticket.Descripcion);
        cmd.Parameters.AddWithValue("@solicitante", ticket.Solicitante); cmd.Parameters.AddWithValue("@prioridad", prioridad.ToString());
        cmd.ExecuteNonQuery();
        using var idCmd = cn.CreateCommand(); idCmd.CommandText = "SELECT LAST_INSERT_ID()";
        var id = Convert.ToInt32(idCmd.ExecuteScalar());
        return ObtenerPorId(id)!;
    }

    public void Editar(int id, string? titulo, string? descripcion, string? solicitante, PrioridadTicket prioridad)
    {
        var ticket = ObtenerPorId(id) ?? throw new InvalidOperationException($"No existe el ticket #{id}.");
        ticket.ActualizarDatos(titulo, descripcion, solicitante, prioridad);
        using var cn = CrearConexion(); cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "UPDATE tickets SET titulo=@titulo,descripcion=@descripcion,solicitante=@solicitante,prioridad=@prioridad,fecha_actualizacion=@actualizado WHERE id=@id";
        cmd.Parameters.AddWithValue("@titulo", ticket.Titulo); cmd.Parameters.AddWithValue("@descripcion", ticket.Descripcion);
        cmd.Parameters.AddWithValue("@solicitante", ticket.Solicitante); cmd.Parameters.AddWithValue("@prioridad", prioridad.ToString());
        cmd.Parameters.AddWithValue("@actualizado", ticket.FechaActualizacion); cmd.Parameters.AddWithValue("@id", id);
        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("No se pudo actualizar el ticket.");
    }

    public void CambiarEstado(int id, EstadoTicket nuevo)
    {
        var ticket = ObtenerPorId(id) ?? throw new InvalidOperationException($"No existe el ticket #{id}.");
        ticket.CambiarEstado(nuevo);
        using var cn = CrearConexion(); cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "UPDATE tickets SET estado=@estado,fecha_actualizacion=@fecha WHERE id=@id";
        cmd.Parameters.AddWithValue("@estado", ticket.Estado.ToString()); cmd.Parameters.AddWithValue("@fecha", ticket.FechaActualizacion); cmd.Parameters.AddWithValue("@id", id);
        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("No se pudo guardar el cambio de estado.");
    }

    public void Eliminar(int id)
    {
        using var cn = CrearConexion(); cn.Open();
        using var cmd = cn.CreateCommand(); cmd.CommandText = "DELETE FROM tickets WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException($"No existe el ticket #{id} para eliminar.");
    }

    public int Contar(EstadoTicket? estado = null)
    {
        using var cn = CrearConexion(); cn.Open(); using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM tickets WHERE (@estado IS NULL OR estado=@estado)";
        cmd.Parameters.AddWithValue("@estado", estado?.ToString());
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
