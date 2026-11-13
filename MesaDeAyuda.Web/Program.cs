using MesaDeAyuda.Web.Components;
using MesaDeAyuda.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var repositorio = new RepositorioTickets();
repositorio.CargarDatosDeMuestra();
builder.Services.AddSingleton(repositorio);

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
