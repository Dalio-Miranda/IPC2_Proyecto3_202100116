using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace ITGSA.Frontend.Pages
{
    public class ConfigRespuesta
    {
        public int ClientesCreados { get; set; }
        public int ClientesActualizados { get; set; }
        public int BancosCreados { get; set; }
        public int BancosActualizados { get; set; }
    }

    public class ConfigModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public ConfigModel(IHttpClientFactory f) => _factory = f;
        public ConfigRespuesta? Respuesta { get; set; }

        public void OnGet() { }

        public async Task OnPostAsync(IFormFile archivo)
        {
            if (archivo == null) return;
            using var reader = new StreamReader(archivo.OpenReadStream());
            string xml = await reader.ReadToEndAsync();

            var client = _factory.CreateClient("Backend");
            var content = new StringContent(xml,
                System.Text.Encoding.UTF8, "application/xml");
            var resp = await client.PostAsync("grabarConfiguracion", content);
            string respXml = await resp.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(respXml);
            Respuesta = new ConfigRespuesta
            {
                ClientesCreados = int.Parse(doc.Root!.Element("clientes")!.Element("creados")!.Value),
                ClientesActualizados = int.Parse(doc.Root!.Element("clientes")!.Element("actualizados")!.Value),
                BancosCreados = int.Parse(doc.Root!.Element("bancos")!.Element("creados")!.Value),
                BancosActualizados = int.Parse(doc.Root!.Element("bancos")!.Element("actualizados")!.Value)
            };
        }
    }
}