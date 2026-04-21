using Microsoft.AspNetCore.Mvc;
using ITGSA.Backend.Services;

namespace ITGSA.Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly DataService _ds;
        public ApiController(DataService ds) => _ds = ds;

        // POST /api/grabarConfiguracion
        [HttpPost("grabarConfiguracion")]
        public async Task<IActionResult> GrabarConfiguracion()
        {
            using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8);
            string xml = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(xml))
                return BadRequest("<error>XML vacio</error>");

            var respuesta = _ds.ProcesarConfig(xml);
            return Content(respuesta.ToString(), "application/xml");
        }

        // POST /api/grabarTransaccion
        [HttpPost("grabarTransaccion")]
        public async Task<IActionResult> GrabarTransaccion()
        {
            using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8);
            string xml = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(xml))
                return BadRequest("<error>XML vacio</error>");

            var respuesta = _ds.ProcesarTransacciones(xml);
            return Content(respuesta.ToString(), "application/xml");
        }

        // POST /api/limpiarDatos
        [HttpPost("limpiarDatos")]
        public IActionResult LimpiarDatos()
        {
            _ds.LimpiarDatos();
            return Content(
                "<respuesta><mensaje>Datos eliminados correctamente</mensaje></respuesta>",
                "application/xml");
        }

        // GET /api/devolverEstadoCuenta?nit=xxx
        [HttpGet("devolverEstadoCuenta")]
        public IActionResult DevolverEstadoCuenta([FromQuery] string? nit)
        {
            var respuesta = _ds.ObtenerEstadoCuenta(nit);
            return Content(respuesta.ToString(), "application/xml");
        }

        // GET /api/devolverResumenPagos?mes=3&anio=2024
        [HttpGet("devolverResumenPagos")]
        public IActionResult DevolverResumenPagos([FromQuery] int mes, [FromQuery] int anio)
        {
            var respuesta = _ds.ObtenerResumenPagos(mes, anio);
            return Content(respuesta.ToString(), "application/xml");
        }
    }
}