using MesaDeAyuda.Web.Models;

namespace MesaDeAyuda.Tests;

public class RepositorioTicketsTests
{
    [Fact]
    public void TicketNuevo_IniciaAbierto()
    {
        var ticket = new Ticket(1, "Problema de red", "No hay conexión en la oficina", "Usuario prueba", PrioridadTicket.Media);
        Assert.Equal(EstadoTicket.Abierto, ticket.Estado);
        Assert.Equal(1, ticket.Id);
    }

    [Fact]
    public void DatosInvalidos_NoPermitenCrearTicket()
    {
        Assert.Throws<ArgumentException>(() => new Ticket(1, "PC", "Falla", "", PrioridadTicket.Media));
    }

    [Fact]
    public void TransicionPermitida_ActualizaEstado()
    {
        var ticket = new Ticket(1, "Problema de red", "No hay conexión en la oficina", "Usuario prueba", PrioridadTicket.Media);
        ticket.CambiarEstado(EstadoTicket.EnProceso);
        Assert.Equal(EstadoTicket.EnProceso, ticket.Estado);
    }

    [Fact]
    public void NoPermiteCerrarTicketAbiertoDirectamente()
    {
        var ticket = new Ticket(1, "Problema de red", "No hay conexión en la oficina", "Usuario prueba", PrioridadTicket.Media);
        Assert.Throws<InvalidOperationException>(() => ticket.CambiarEstado(EstadoTicket.Cerrado));
        Assert.Equal(EstadoTicket.Abierto, ticket.Estado);
    }

    [Fact]
    public void TicketCerrado_NoPuedeCambiarOtraVez()
    {
        var ticket = new Ticket(1, "Problema de red", "No hay conexión en la oficina", "Usuario prueba", PrioridadTicket.Media);
        ticket.CambiarEstado(EstadoTicket.EnProceso);
        ticket.CambiarEstado(EstadoTicket.Resuelto);
        ticket.CambiarEstado(EstadoTicket.Cerrado);
        Assert.Throws<InvalidOperationException>(() => ticket.CambiarEstado(EstadoTicket.Cerrado));
    }

    [Fact]
    public void TicketCerrado_NoPuedeEditarDatos()
    {
        var ticket = new Ticket(1, "Problema de red", "No hay conexión en la oficina", "Usuario prueba", PrioridadTicket.Media);
        ticket.CambiarEstado(EstadoTicket.EnProceso);
        ticket.CambiarEstado(EstadoTicket.Resuelto);
        ticket.CambiarEstado(EstadoTicket.Cerrado);
        Assert.Throws<InvalidOperationException>(() => ticket.ActualizarDatos("Otro título", "Descripción nueva válida", "Usuario prueba", PrioridadTicket.Alta));
    }
}
