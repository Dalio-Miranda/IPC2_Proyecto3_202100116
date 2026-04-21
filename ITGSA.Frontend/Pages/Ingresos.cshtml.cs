using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Xml.Linq;

namespace ITGSA.Frontend.Pages
{
    public class IngresosModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public IngresosModel(IHttpClientFactory f) => _factory = f;

        public int Mes { get; set; } = DateTime.Now.Month;
        public int Anio { get; set; } = DateTime.Now.Year;
        public string DatosJson { get; set; } = "{}";

        public async Task OnGetAsync(int mes = 0, int anio = 0)
        {
            Mes = mes > 0 ? mes : DateTime.Now.Month;
            Anio = anio > 0 ? anio : DateTime.Now.Year;

            var client = _factory.CreateClient("Backend");
            var resp = await client.GetAsync(
                $"devolverResumenPagos?mes={Mes}&anio={Anio}");
            string xml = await resp.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(xml);
            var titulo = doc.Root!.Element("mesElegido")!.Value;
            var bancos = doc.Root.Elements("banco")
                .Select(b => b.Element("nombre")!.Value).ToList();
            var periodos = doc.Root.Elements("banco").FirstOrDefault()
                ?.Elements("mes")
                .Select(m => m.Element("periodo")!.Value).ToList() ?? new();
            var valores = periodos.Select((_, pi) =>
                doc.Root.Elements("banco").Select(b =>
                    decimal.Parse(
                        b.Elements("mes").ElementAt(pi).Element("total")!.Value,
                        System.Globalization.CultureInfo.InvariantCulture) / 1000m
                ).ToList()
            ).ToList();

            DatosJson = JsonSerializer.Serialize(
                new { titulo, bancos, meses = periodos, valores });
        }
    }
}