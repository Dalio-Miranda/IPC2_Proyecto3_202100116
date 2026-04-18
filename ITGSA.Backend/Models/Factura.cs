namespace ITGSA.Backend.Models
{
    public class Factura
    {
        public string NumeroFactura { get; set; } = "";
        public string NITCliente { get; set; } = "";
        public string Fecha { get; set; } = "";
        public decimal Valor { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}