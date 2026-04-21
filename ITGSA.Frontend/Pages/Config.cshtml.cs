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
        public string RespuestaRaw { get; set; } = "";

        public void OnGet() { }

        public async Task OnPostAsync(IFormFile archivo)
        {
            if (archivo == null) return;
            using var reader = new StreamReader(archivo.OpenReadStream());
            string xml = await reader.ReadToEndAsync();

            var client = _factory.CreateClient("Backend");
            var content = new StringContent(xml,
                System.Text.Encoding.UTF8, "application/xml");

            try
            {
                var resp = await client.PostAsync("grabarConfiguracion", content);
                string respXml = await resp.Content.ReadAsStringAsync();
                RespuestaRaw = respXml;

                var doc = XDocument.Parse(respXml);
                var root = doc.Root!;
                Respuesta = new ConfigRespuesta
                {
                    ClientesCreados = int.Parse(root.Element("clientes")?.Element("creados")?.Value ?? "0"),
                    ClientesActualizados = int.Parse(root.Element("clientes")?.Element("actualizados")?.Value ?? "0"),
                    BancosCreados = int.Parse(root.Element("bancos")?.Element("creados")?.Value ?? "0"),
                    BancosActualizados = int.Parse(root.Element("bancos")?.Element("actualizados")?.Value ?? "0")
                };
            }
            catch (Exception ex)
            {
                RespuestaRaw = ex.Message;
                Respuesta = new ConfigRespuesta();
            }
        }
    }
}