using MesaDeAyuda.Web.Models;

namespace MesaDeAyuda.Tests;

public class TicketTests
{
    private static Ticket NuevoTicket() =>
        new(1, "Lector de códigos no lee", "Lee algunos productos y otros no.", "Cajero de muestra", PrioridadTicket.Media);

    
    private static Ticket TicketEn(EstadoTicket estado)
    {
        var t = NuevoTicket();
        var camino = estado switch
        {
            EstadoTicket.Abierto => Array.Empty<EstadoTicket>(),
            EstadoTicket.EnProceso => new[] { EstadoTicket.EnProceso },
            EstadoTicket.Resuelto => new[] { EstadoTicket.EnProceso, EstadoTicket.Resuelto },
            EstadoTicket.Cerrado => new[] { EstadoTicket.EnProceso, EstadoTicket.Resuelto, EstadoTicket.Cerrado },
            _ => throw new ArgumentOutOfRangeException(nameof(estado))
        };
        foreach (var paso in camino) t.CambiarEstado(paso);
        return t;
    }

    

    [Fact]
    public void Crear_ConDatosValidos_NaceAbiertoConFechas()
    {
        var antes = DateTime.Now;
        var t = NuevoTicket();

        Assert.Equal(EstadoTicket.Abierto, t.Estado);
        Assert.Equal(1, t.Id);
        Assert.True(t.FechaCreacion >= antes);
        Assert.Equal(t.FechaCreacion, t.FechaActualizacion);
    }

    [Fact]
    public void Crear_RecortaEspaciosSobrantes()
    {
        var t = new Ticket(1, "  Caja sin red  ", "  La caja 3 no tiene red.  ", "  Ana  ", PrioridadTicket.Baja);
        Assert.Equal("Caja sin red", t.Titulo);
        Assert.Equal("Ana", t.Solicitante);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData("PC")]
    [InlineData("1234")]
    public void Validar_TituloInvalido_DaError(string? titulo)
    {
        var errores = Ticket.Validar(titulo, "Descripción suficientemente larga", "Ana", PrioridadTicket.Media);
        Assert.True(errores.ContainsKey(Ticket.CampoTitulo));
        Assert.Equal("El título debe tener entre 5 y 80 caracteres.", errores[Ticket.CampoTitulo]);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(80)]
    public void Validar_TituloEnLosLimites_EsValido(int largo)
    {
        var errores = Ticket.Validar(new string('a', largo), "Descripción suficientemente larga", "Ana", PrioridadTicket.Media);
        Assert.False(errores.ContainsKey(Ticket.CampoTitulo));
    }

    [Fact]
    public void Validar_TituloDe81Caracteres_DaError()
    {
        var errores = Ticket.Validar(new string('a', 81), "Descripción suficientemente larga", "Ana", PrioridadTicket.Media);
        Assert.True(errores.ContainsKey(Ticket.CampoTitulo));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("No sirve")]   // 8 caracteres
    public void Validar_DescripcionInvalida_DaError(string? descripcion)
    {
        var errores = Ticket.Validar("Título válido", descripcion, "Ana", PrioridadTicket.Media);
        Assert.Equal("La descripción debe tener entre 10 y 500 caracteres.", errores[Ticket.CampoDescripcion]);
    }

    [Fact]
    public void Validar_DescripcionDe501Caracteres_DaError()
    {
        var errores = Ticket.Validar("Título válido", new string('a', 501), "Ana", PrioridadTicket.Media);
        Assert.True(errores.ContainsKey(Ticket.CampoDescripcion));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Al")]
    public void Validar_SolicitanteInvalido_DaError(string? solicitante)
    {
        var errores = Ticket.Validar("Título válido", "Descripción suficientemente larga", solicitante, PrioridadTicket.Media);
        Assert.Equal("Escriba el nombre de quien reporta (mín. 3 letras).", errores[Ticket.CampoSolicitante]);
    }

    [Fact]
    public void Validar_PrioridadNoPermitida_DaError()
    {
        var errores = Ticket.Validar("Título válido", "Descripción suficientemente larga", "Ana", (PrioridadTicket)99);
        Assert.True(errores.ContainsKey(Ticket.CampoPrioridad));
    }

    [Fact]
    public void Validar_TodoValido_NoDaErrores()
    {
        var errores = Ticket.Validar("Título válido", "Descripción suficientemente larga", "Ana", PrioridadTicket.Alta);
        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_VariosCamposMal_ReportaTodos()
    {
        var errores = Ticket.Validar("", "", "", (PrioridadTicket)99);
        Assert.Equal(4, errores.Count);
    }

    [Fact]
    public void Crear_ConDatosInvalidos_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ticket(1, "PC", "No sirve", "", PrioridadTicket.Media));
    }

    // ---------- Cambio de estado ----------

    [Theory]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.EnProceso)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.EnProceso)]
    public void CambiarEstado_Permitido_ActualizaEstadoYFecha(EstadoTicket origen, EstadoTicket destino)
    {
        var t = TicketEn(origen);
        var fechaAnterior = t.FechaActualizacion;

        var ok = t.TryCambiarEstado(destino, out var mensaje);

        Assert.True(ok);
        Assert.Equal(destino, t.Estado);
        Assert.True(t.FechaActualizacion >= fechaAnterior);
        Assert.Equal($"Ticket #1 ahora está {destino.Texto()}", mensaje);
    }

    [Theory]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.EnProceso)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Cerrado, EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Cerrado, EstadoTicket.EnProceso)]
    [InlineData(EstadoTicket.Cerrado, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Cerrado, EstadoTicket.Cerrado)]
    public void CambiarEstado_NoPermitido_NoModificaNada(EstadoTicket origen, EstadoTicket destino)
    {
        var t = TicketEn(origen);
        var fechaAnterior = t.FechaActualizacion;

        var ok = t.TryCambiarEstado(destino, out var mensaje);

        Assert.False(ok);
        Assert.Equal(origen, t.Estado);
        Assert.Equal(fechaAnterior, t.FechaActualizacion);
        Assert.False(string.IsNullOrWhiteSpace(mensaje));
    }

    [Fact]
    public void CambiarEstado_AbiertoACerrado_ExplicaQueFaltaElPasoIntermedio()
    {
        var t = TicketEn(EstadoTicket.Abierto);
        t.TryCambiarEstado(EstadoTicket.Cerrado, out var mensaje);

        Assert.Contains("No se puede pasar de Abierto a Cerrado", mensaje);
        Assert.Contains("En proceso", mensaje);
    }

    [Fact]
    public void CambiarEstado_TicketCerradoNoSePuedeCerrarOtraVez()
    {
        var t = TicketEn(EstadoTicket.Cerrado);

        var ok = t.TryCambiarEstado(EstadoTicket.Cerrado, out var mensaje);

        Assert.False(ok);
        Assert.Contains("ya está Cerrado", mensaje);
    }

    [Fact]
    public void CambiarEstado_NoPermitido_LanzaInvalidOperationException()
    {
        var t = TicketEn(EstadoTicket.Abierto);
        Assert.Throws<InvalidOperationException>(() => t.CambiarEstado(EstadoTicket.Cerrado));
    }

    [Fact]
    public void CambiarEstado_ValorFueraDeLaEnumeracion_SeRechaza()
    {
        var t = TicketEn(EstadoTicket.Abierto);
        Assert.False(t.TryCambiarEstado((EstadoTicket)99, out _));
        Assert.Equal(EstadoTicket.Abierto, t.Estado);
    }

    [Fact]
    public void PuedeCambiarA_ReflejaElFlujoPermitido()
    {
        var t = TicketEn(EstadoTicket.Resuelto);
        Assert.True(t.PuedeCambiarA(EstadoTicket.Cerrado));
        Assert.True(t.PuedeCambiarA(EstadoTicket.EnProceso));
        Assert.False(t.PuedeCambiarA(EstadoTicket.Abierto));
    }
}
