using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ITGSA.Frontend.Pages
{
    public class ResetModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public ResetModel(IHttpClientFactory f) => _factory = f;
        public string Mensaje { get; set; } = "";

        public void OnGet() { }

        public async Task OnPostAsync()
        {
            var client = _factory.CreateClient("Backend");
            var resp = await client.PostAsync("limpiarDatos", null);
            Mensaje = resp.IsSuccessStatusCode
                ? "✅ Datos reseteados correctamente."
                : "❌ Error al resetear.";
        }
    }
}