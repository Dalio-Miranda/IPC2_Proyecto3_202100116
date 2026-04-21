using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

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
        public EstadoCuentaModel(IHttpClientFactory f) => _factory = f;

        public List<ClienteVM> Clientes { get; set; } = new();
        public string NitBuscado { get; set; } = "";

        public async Task OnGetAsync(string? nit)
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
    }
}