using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace ITGSA.Frontend.Pages
{
    public class TransRespuesta
    {
        public int FacturasNuevas { get; set; }
        public int FacturasDuplicadas { get; set; }
        public int FacturasError { get; set; }
        public int PagosNuevos { get; set; }
        public int PagosDuplicados { get; set; }
        public int PagosError { get; set; }
    }

    public class TransaccionesModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public TransaccionesModel(IHttpClientFactory f) => _factory = f;
        public TransRespuesta? Respuesta { get; set; }

        public void OnGet() { }

        public async Task OnPostAsync(IFormFile archivo)
        {
            if (archivo == null) return;
            using var reader = new StreamReader(archivo.OpenReadStream());
            string xml = await reader.ReadToEndAsync();

            var client = _factory.CreateClient("Backend");
            var content = new StringContent(xml,
                System.Text.Encoding.UTF8, "application/xml");
            var resp = await client.PostAsync("grabarTransaccion", content);
            string respXml = await resp.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(respXml);
            Respuesta = new TransRespuesta
            {
                FacturasNuevas = int.Parse(doc.Root!.Element("facturas")!.Element("nuevasFacturas")!.Value),
                FacturasDuplicadas = int.Parse(doc.Root!.Element("facturas")!.Element("facturasDuplicadas")!.Value),
                FacturasError = int.Parse(doc.Root!.Element("facturas")!.Element("facturasConError")!.Value),
                PagosNuevos = int.Parse(doc.Root!.Element("pagos")!.Element("nuevosPagos")!.Value),
                PagosDuplicados = int.Parse(doc.Root!.Element("pagos")!.Element("pagosDuplicados")!.Value),
                PagosError = int.Parse(doc.Root!.Element("pagos")!.Element("pagosConError")!.Value)
            };
        }
    }
}