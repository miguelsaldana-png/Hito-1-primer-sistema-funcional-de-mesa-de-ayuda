using MesaDeAyuda.Web.Models;

namespace MesaDeAyuda.Tests;

public class RepositorioTicketsTests
{
    private static Ticket Agregar(RepositorioTickets repo, string titulo = "Título de prueba") =>
        repo.Agregar(titulo, "Descripción suficientemente larga", "Persona de prueba", PrioridadTicket.Media);

    [Fact]
    public void Agregar_AsignaIdsCorrelativosYEstadoAbierto()
    {
        var repo = new RepositorioTickets();

        var a = Agregar(repo);
        var b = Agregar(repo);

        Assert.Equal(1, a.Id);
        Assert.Equal(2, b.Id);
        Assert.Equal(EstadoTicket.Abierto, a.Estado);
        Assert.Equal(2, repo.Contar());
    }

    [Fact]
    public void Agregar_ConDatosInvalidos_NoGuardaNiConsumeId()
    {
        var repo = new RepositorioTickets();

        Assert.Throws<ArgumentException>(() =>
            repo.Agregar("PC", "No sirve", "", PrioridadTicket.Media));

        Assert.Equal(0, repo.Contar());
        Assert.Equal(1, Agregar(repo).Id);
    }

    [Fact]
    public void ObtenerPorId_Existente_DevuelveElTicket()
    {
        var repo = new RepositorioTickets();
        var t = Agregar(repo);
        Assert.Same(t, repo.ObtenerPorId(t.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(99)]
    public void ObtenerPorId_Inexistente_DevuelveNull(int id)
    {
        var repo = new RepositorioTickets();
        Agregar(repo);
        Assert.Null(repo.ObtenerPorId(id));
    }

    [Fact]
    public void Listar_DevuelveMasRecientesPrimero()
    {
        var repo = new RepositorioTickets();
        Agregar(repo, "Primer ticket");
        Agregar(repo, "Segundo ticket");
        Agregar(repo, "Tercer ticket");

        var ids = repo.Listar().Select(t => t.Id).ToArray();

        Assert.Equal(new[] { 3, 2, 1 }, ids);
    }

    [Fact]
    public void Listar_ConFiltro_DevuelveSoloEseEstado()
    {
        var repo = new RepositorioTickets();
        var a = Agregar(repo, "Ticket uno");
        Agregar(repo, "Ticket dos");
        var c = Agregar(repo, "Ticket tres");
        c.CambiarEstado(EstadoTicket.EnProceso);
        c.CambiarEstado(EstadoTicket.Resuelto);
        c.CambiarEstado(EstadoTicket.Cerrado);

        Assert.Equal(2, repo.Listar(EstadoTicket.Abierto).Count);
        Assert.Single(repo.Listar(EstadoTicket.Cerrado));
        Assert.Same(c, repo.Listar(EstadoTicket.Cerrado)[0]);
        Assert.Empty(repo.Listar(EstadoTicket.EnProceso));
        Assert.Equal(3, repo.Listar().Count);
        Assert.Contains(a, repo.Listar(EstadoTicket.Abierto));
    }

    [Fact]
    public void Listar_CambiarElFiltroNoAlteraLosTickets()
    {
        var repo = new RepositorioTickets();
        Agregar(repo);
        Agregar(repo);

        _ = repo.Listar(EstadoTicket.Cerrado);
        _ = repo.Listar(EstadoTicket.Abierto);

        Assert.Equal(2, repo.Contar());
        Assert.Equal(2, repo.Listar().Count);
    }

    [Fact]
    public void Contar_SeActualizaAlRegistrarYAlCambiarEstado()
    {
        var repo = new RepositorioTickets();
        Assert.Equal(0, repo.Contar());

        var a = Agregar(repo);
        Agregar(repo);
        Assert.Equal(2, repo.Contar());
        Assert.Equal(2, repo.Contar(EstadoTicket.Abierto));
        Assert.Equal(0, repo.Contar(EstadoTicket.Cerrado));

        a.CambiarEstado(EstadoTicket.EnProceso);
        a.CambiarEstado(EstadoTicket.Resuelto);
        a.CambiarEstado(EstadoTicket.Cerrado);

        Assert.Equal(2, repo.Contar());
        Assert.Equal(1, repo.Contar(EstadoTicket.Abierto));
        Assert.Equal(1, repo.Contar(EstadoTicket.Cerrado));
    }

    [Fact]
    public void CambiarEstadoDeUnTicket_NoModificaLosDemas()
    {
        var repo = new RepositorioTickets();
        var a = Agregar(repo);
        var b = Agregar(repo);

        a.CambiarEstado(EstadoTicket.EnProceso);

        Assert.Equal(EstadoTicket.EnProceso, repo.ObtenerPorId(a.Id)!.Estado);
        Assert.Equal(EstadoTicket.Abierto, repo.ObtenerPorId(b.Id)!.Estado);
    }

    [Fact]
    public void CargarDatosDeMuestra_RegistraTicketsValidos()
    {
        var repo = new RepositorioTickets();
        repo.CargarDatosDeMuestra();

        Assert.Equal(3, repo.Contar());
        Assert.Equal(1, repo.Contar(EstadoTicket.EnProceso));
    }
}
