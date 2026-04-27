using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;
using ITGSA.Frontend.Services;

namespace ITGSA.Frontend.Pages
{
    public class TransaccionVM
    {
        public string Fecha { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Monto { get; set; } = "";
    }

    public class ClienteVM
    {
        public string NIT { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string SaldoActual { get; set; } = "";
        public List<TransaccionVM> Transacciones { get; set; } = new();
    }

    public class EstadoCuentaModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly PdfService _pdf;

        public EstadoCuentaModel(IHttpClientFactory f, PdfService pdf)
        {
            _factory = f;
            _pdf = pdf;
        }

        public List<ClienteVM> Clientes { get; set; } = new();
        public string NitBuscado { get; set; } = "";

        private async Task CargarDatos(string? nit)
        {
            NitBuscado = nit ?? "";
            var client = _factory.CreateClient("Backend");
            string url = string.IsNullOrEmpty(nit)
                ? "devolverEstadoCuenta"
                : $"devolverEstadoCuenta?nit={nit}";
            var resp = await client.GetAsync(url);
            string xml = await resp.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(xml);
            foreach (var e in doc.Root!.Elements("cliente"))
            {
                var vm = new ClienteVM
                {
                    NIT = e.Element("NIT")!.Value,
                    Nombre = e.Element("nombre")!.Value,
                    SaldoActual = e.Element("saldoActual")!.Value
                };
                foreach (var t in e.Element("transacciones")!.Elements("transaccion"))
                    vm.Transacciones.Add(new TransaccionVM
                    {
                        Fecha = t.Element("fecha")!.Value,
                        Tipo = t.Element("tipo")!.Value,
                        Descripcion = t.Element("descripcion")!.Value,
                        Monto = t.Element("monto")!.Value
                    });
                Clientes.Add(vm);
            }
        }

        public async Task OnGetAsync(string? nit)
        {
            await CargarDatos(nit);
        }

        public async Task<IActionResult> OnGetPdfAsync(string? nit)
        {
            await CargarDatos(nit);

            var todasTransacciones = new List<(string, string, string)>();
            string nitCliente = "";
            string nombreCliente = "";
            string saldoCliente = "";

            foreach (var c in Clientes)
            {
                nitCliente = c.NIT;
                nombreCliente = c.Nombre;
                saldoCliente = c.SaldoActual;

                foreach (var t in c.Transacciones)
                {
                    string cargo = t.Tipo == "cargo"
                        ? $"Q. {t.Monto} (Fact. #{t.Descripcion})" : "";
                    string abono = t.Tipo == "abono"
                        ? $"Q. {t.Monto} ({t.Descripcion})" : "";
                    todasTransacciones.Add((t.Fecha, cargo, abono));
                }
            }

            var bytes = _pdf.GenerarEstadoCuenta(
                nitCliente, nombreCliente, saldoCliente, todasTransacciones);
            return File(bytes, "application/pdf", "EstadoCuenta.pdf");
        }
    }
}