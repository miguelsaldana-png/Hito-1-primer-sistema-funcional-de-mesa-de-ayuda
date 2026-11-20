namespace MesaDeAyuda.Web.Models;

public enum EstadoTicket
{
    Abierto,
    EnProceso,
    Resuelto,
    Cerrado
}

public enum PrioridadTicket
{
    Baja,
    Media,
    Alta
}

public static class EnumeracionesExtensiones
{
    /// <summary>Texto legible para mostrar al usuario ("En proceso" en lugar de "EnProceso").</summary>
    public static string Texto(this EstadoTicket estado) => estado switch
    {
        EstadoTicket.EnProceso => "En proceso",
        _ => estado.ToString()
    };
}
