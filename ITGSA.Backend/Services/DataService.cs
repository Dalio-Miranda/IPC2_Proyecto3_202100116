using System.Xml.Linq;
using System.Text.RegularExpressions;
using ITGSA.Backend.Models;

namespace ITGSA.Backend.Services
{
    public class DataService
    {
        private readonly string _dataPath;

        public DataService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        // ─── Rutas de archivos XML ───────────────────────────
        private string ClientesPath => Path.Combine(_dataPath, "clientes.xml");
        private string BancosPath => Path.Combine(_dataPath, "bancos.xml");
        private string FacturasPath => Path.Combine(_dataPath, "facturas.xml");
        private string PagosPath => Path.Combine(_dataPath, "pagos.xml");

        // ─── Helpers ─────────────────────────────────────────
        private string LimpiarTexto(string valor) =>
            Regex.Replace(valor.Trim(), @"[^\w\s\-]", "").Trim();

        private string LimpiarNIT(string valor)
        {
            var match = Regex.Match(valor.Trim(), @"[\w\-]+");
            return match.Success ? match.Value.Trim() : valor.Trim();
        }

        private string LimpiarFecha(string valor)
        {
            var match = Regex.Match(valor, @"\d{2}/\d{2}/\d{4}");
            return match.Success ? match.Value : valor.Trim();
        }

        private decimal LimpiarDecimal(string valor)
        {
            var match = Regex.Match(valor, @"[\d]+([.,]\d+)?");
            if (match.Success)
                return decimal.Parse(match.Value.Replace(",", "."),
                    System.Globalization.CultureInfo.InvariantCulture);
            return 0;
        }

        // ─── RESET ───────────────────────────────────────────
        public void LimpiarDatos()
        {
            foreach (var f in new[] { ClientesPath, BancosPath, FacturasPath, PagosPath })
                if (File.Exists(f)) File.Delete(f);
        }

        // ─── CLIENTES ────────────────────────────────────────
        public ListaEnlazada<Cliente> ObtenerClientes()
        {
            var lista = new ListaEnlazada<Cliente>();
            if (!File.Exists(ClientesPath)) return lista;
            var doc = XDocument.Load(ClientesPath);
            foreach (var e in doc.Root!.Elements("cliente"))
                lista.Agregar(new Cliente
                {
                    NIT = e.Element("NIT")!.Value,
                    Nombre = e.Element("nombre")!.Value
                });
            return lista;
        }

        public void GuardarClientes(ListaEnlazada<Cliente> clientes)
        {
            var doc = new XDocument(new XElement("clientes",
                clientes.Select(c => new XElement("cliente",
                    new XElement("NIT", c.NIT),
                    new XElement("nombre", c.Nombre)))));
            doc.Save(ClientesPath);
        }

        // ─── BANCOS ──────────────────────────────────────────
        public ListaEnlazada<Banco> ObtenerBancos()
        {
            var lista = new ListaEnlazada<Banco>();
            if (!File.Exists(BancosPath)) return lista;
            var doc = XDocument.Load(BancosPath);
            foreach (var e in doc.Root!.Elements("banco"))
                lista.Agregar(new Banco
                {
                    Codigo = int.Parse(e.Element("codigo")!.Value),
                    Nombre = e.Element("nombre")!.Value
                });
            return lista;
        }

        public void GuardarBancos(ListaEnlazada<Banco> bancos)
        {
            var doc = new XDocument(new XElement("bancos",
                bancos.Select(b => new XElement("banco",
                    new XElement("codigo", b.Codigo),
                    new XElement("nombre", b.Nombre)))));
            doc.Save(BancosPath);
        }

        // ─── FACTURAS ────────────────────────────────────────
        public ListaEnlazada<Factura> ObtenerFacturas()
        {
            var lista = new ListaEnlazada<Factura>();
            if (!File.Exists(FacturasPath)) return lista;
            var doc = XDocument.Load(FacturasPath);
            foreach (var e in doc.Root!.Elements("factura"))
                lista.Agregar(new Factura
                {
                    NumeroFactura = e.Element("numeroFactura")!.Value,
                    NITCliente = e.Element("NITcliente")!.Value,
                    Fecha = e.Element("fecha")!.Value,
                    Valor = decimal.Parse(e.Element("valor")!.Value,
                                        System.Globalization.CultureInfo.InvariantCulture),
                    SaldoPendiente = decimal.Parse(e.Element("saldoPendiente")!.Value,
                                        System.Globalization.CultureInfo.InvariantCulture)
                });
            return lista;
        }

        public void GuardarFacturas(ListaEnlazada<Factura> facturas)
        {
            var doc = new XDocument(new XElement("facturas",
                facturas.Select(f => new XElement("factura",
                    new XElement("numeroFactura", f.NumeroFactura),
                    new XElement("NITcliente", f.NITCliente),
                    new XElement("fecha", f.Fecha),
                    new XElement("valor", f.Valor.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)),
                    new XElement("saldoPendiente", f.SaldoPendiente.ToString(
                        System.Globalization.CultureInfo.InvariantCulture))))));
            doc.Save(FacturasPath);
        }

        // ─── PAGOS ───────────────────────────────────────────
        public ListaEnlazada<Pago> ObtenerPagos()
        {
            var lista = new ListaEnlazada<Pago>();
            if (!File.Exists(PagosPath)) return lista;
            var doc = XDocument.Load(PagosPath);
            foreach (var e in doc.Root!.Elements("pago"))
                lista.Agregar(new Pago
                {
                    CodigoBanco = int.Parse(e.Element("codigoBanco")!.Value),
                    Fecha = e.Element("fecha")!.Value,
                    NITCliente = e.Element("NITcliente")!.Value,
                    Valor = decimal.Parse(e.Element("valor")!.Value,
                                      System.Globalization.CultureInfo.InvariantCulture)
                });
            return lista;
        }

        public void GuardarPagos(ListaEnlazada<Pago> pagos)
        {
            var doc = new XDocument(new XElement("pagos",
                pagos.Select(p => new XElement("pago",
                    new XElement("codigoBanco", p.CodigoBanco),
                    new XElement("fecha", p.Fecha),
                    new XElement("NITcliente", p.NITCliente),
                    new XElement("valor", p.Valor.ToString(
                        System.Globalization.CultureInfo.InvariantCulture))))));
            doc.Save(PagosPath);
        }

        // ─── PROCESAR CONFIG ─────────────────────────────────
        public XDocument ProcesarConfig(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            var clientes = ObtenerClientes();
            var bancos = ObtenerBancos();
            int cliCreados = 0, cliActualizados = 0;
            int banCreados = 0, banActualizados = 0;

            foreach (var e in doc.Root!.Element("clientes")?.Elements("cliente")
                     ?? Enumerable.Empty<XElement>())
            {
                string nit = LimpiarNIT(e.Element("NIT")?.Value ?? "");
                string nombre = LimpiarTexto(e.Element("nombre")?.Value ?? "");
                if (string.IsNullOrEmpty(nit)) continue;

                var existente = clientes.FirstOrDefault(c =>
                    c.NIT.Equals(nit, StringComparison.OrdinalIgnoreCase));
                if (existente != null)
                {
                    existente.Nombre = nombre;
                    cliActualizados++;
                }
                else
                {
                    clientes.Agregar(new Cliente { NIT = nit, Nombre = nombre });
                    cliCreados++;
                }
            }

            foreach (var e in doc.Root!.Element("bancos")?.Elements("banco")
                     ?? Enumerable.Empty<XElement>())
            {
                var codigoMatch = Regex.Match(e.Element("codigo")?.Value ?? "", @"\d+");
                if (!codigoMatch.Success) continue;
                int codigo = int.Parse(codigoMatch.Value);
                string nombre = LimpiarTexto(e.Element("nombre")?.Value ?? "");

                var existente = bancos.FirstOrDefault(b => b.Codigo == codigo);
                if (existente != null)
                {
                    existente.Nombre = nombre;
                    banActualizados++;
                }
                else
                {
                    bancos.Agregar(new Banco { Codigo = codigo, Nombre = nombre });
                    banCreados++;
                }
            }

            GuardarClientes(clientes);
            GuardarBancos(bancos);

            return new XDocument(new XElement("respuesta",
                new XElement("clientes",
                    new XElement("creados", cliCreados),
                    new XElement("actualizados", cliActualizados)),
                new XElement("bancos",
                    new XElement("creados", banCreados),
                    new XElement("actualizados", banActualizados))));
        }

        // ─── PROCESAR TRANSACCIONES ──────────────────────────
        public XDocument ProcesarTransacciones(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            var clientes = ObtenerClientes();
            var bancos = ObtenerBancos();
            var facturas = ObtenerFacturas();
            var pagos = ObtenerPagos();

            int facNuevas = 0, facDuplicadas = 0, facError = 0;
            int pagNuevos = 0, pagDuplicados = 0, pagError = 0;

            foreach (var e in doc.Root!.Element("facturas")?.Elements("factura")
                     ?? Enumerable.Empty<XElement>())
            {
                string numFac = LimpiarTexto(e.Element("numeroFactura")?.Value ?? "");
                string nit = LimpiarNIT(e.Element("NITcliente")?.Value ?? "");
                string fecha = LimpiarFecha(e.Element("fecha")?.Value ?? "");
                decimal valor = LimpiarDecimal(e.Element("valor")?.Value ?? "0");

                if (string.IsNullOrEmpty(numFac) || string.IsNullOrEmpty(nit) ||
                    string.IsNullOrEmpty(fecha) || valor <= 0)
                { facError++; continue; }

                if (!clientes.Any(c => c.NIT.Equals(nit, StringComparison.OrdinalIgnoreCase)))
                { facError++; continue; }

                if (facturas.Any(f => f.NumeroFactura.Equals(numFac,
                    StringComparison.OrdinalIgnoreCase)))
                { facDuplicadas++; continue; }

                facturas.Agregar(new Factura
                {
                    NumeroFactura = numFac,
                    NITCliente = nit,
                    Fecha = fecha,
                    Valor = valor,
                    SaldoPendiente = valor
                });
                facNuevas++;
            }

            foreach (var e in doc.Root!.Element("pagos")?.Elements("pago")
                     ?? Enumerable.Empty<XElement>())
            {
                var codMatch = Regex.Match(e.Element("codigoBanco")?.Value ?? "", @"\d+");
                string nit = LimpiarNIT(e.Element("NITcliente")?.Value ?? "");
                string fecha = LimpiarFecha(e.Element("fecha")?.Value ?? "");
                decimal valor = LimpiarDecimal(e.Element("valor")?.Value ?? "0");

                if (!codMatch.Success || string.IsNullOrEmpty(nit) ||
                    string.IsNullOrEmpty(fecha) || valor <= 0)
                { pagError++; continue; }

                int codigo = int.Parse(codMatch.Value);

                if (!clientes.Any(c => c.NIT.Equals(nit, StringComparison.OrdinalIgnoreCase)))
                { pagError++; continue; }

                if (!bancos.Any(b => b.Codigo == codigo))
                { pagError++; continue; }

                // Aplicar pago a facturas más antiguas primero (FIFO)
                var facturasCliente = facturas
                    .Where(f => f.NITCliente.Equals(nit, StringComparison.OrdinalIgnoreCase)
                             && f.SaldoPendiente > 0)
                    .OrderBy(f => {
                        var p = f.Fecha.Split('/');
                        return p.Length == 3
                            ? new DateTime(int.Parse(p[2]), int.Parse(p[1]), int.Parse(p[0]))
                            : DateTime.MinValue;
                    }).ToArray();

                decimal montoRestante = valor;
                foreach (var factura in facturasCliente)
                {
                    if (montoRestante <= 0) break;
                    if (montoRestante >= factura.SaldoPendiente)
                    {
                        montoRestante -= factura.SaldoPendiente;
                        factura.SaldoPendiente = 0;
                    }
                    else
                    {
                        factura.SaldoPendiente -= montoRestante;
                        montoRestante = 0;
                    }
                }

                pagos.Agregar(new Pago
                {
                    CodigoBanco = codigo,
                    Fecha = fecha,
                    NITCliente = nit,
                    Valor = valor
                });
                pagNuevos++;
            }

            GuardarFacturas(facturas);
            GuardarPagos(pagos);

            return new XDocument(new XElement("transacciones",
                new XElement("facturas",
                    new XElement("nuevasFacturas", facNuevas),
                    new XElement("facturasDuplicadas", facDuplicadas),
                    new XElement("facturasConError", facError)),
                new XElement("pagos",
                    new XElement("nuevosPagos", pagNuevos),
                    new XElement("pagosDuplicados", pagDuplicados),
                    new XElement("pagosConError", pagError))));
        }

        // ─── ESTADO DE CUENTA ────────────────────────────────
        public XDocument ObtenerEstadoCuenta(string? nit)
        {
            var clientes = ObtenerClientes();
            var facturas = ObtenerFacturas();
            var pagos = ObtenerPagos();
            var bancos = ObtenerBancos();

            var clientesFiltrados = string.IsNullOrEmpty(nit)
                ? clientes.OrderBy(c => c.NIT).ToArray()
                : clientes.Where(c => c.NIT.Equals(nit,
                      StringComparison.OrdinalIgnoreCase)).ToArray();

            var root = new XElement("estadosCuenta");

            foreach (var cliente in clientesFiltrados)
            {
                var facturasCliente = facturas
                    .Where(f => f.NITCliente.Equals(cliente.NIT,
                        StringComparison.OrdinalIgnoreCase)).ToArray();
                var pagosCliente = pagos
                    .Where(p => p.NITCliente.Equals(cliente.NIT,
                        StringComparison.OrdinalIgnoreCase)).ToArray();

                decimal totalFacturado = facturasCliente.Sum(f => f.Valor);
                decimal totalPagado = pagosCliente.Sum(p => p.Valor);
                decimal saldo = totalPagado - totalFacturado;

                var transacciones = new ListaEnlazada<XElement>();

                foreach (var f in facturasCliente)
                    transacciones.Agregar(new XElement("transaccion",
                        new XElement("fecha", f.Fecha),
                        new XElement("tipo", "cargo"),
                        new XElement("descripcion", f.NumeroFactura),
                        new XElement("monto", f.Valor.ToString("F2",
                            System.Globalization.CultureInfo.InvariantCulture))));

                foreach (var p in pagosCliente)
                {
                    var banco = bancos.FirstOrDefault(b => b.Codigo == p.CodigoBanco);
                    transacciones.Agregar(new XElement("transaccion",
                        new XElement("fecha", p.Fecha),
                        new XElement("tipo", "abono"),
                        new XElement("descripcion", banco?.Nombre ?? p.CodigoBanco.ToString()),
                        new XElement("monto", p.Valor.ToString("F2",
                            System.Globalization.CultureInfo.InvariantCulture))));
                }

                var transOrdenadas = transacciones
                    .OrderByDescending(t =>
                    {
                        var partes = t.Element("fecha")!.Value.Split('/');
                        if (partes.Length == 3)
                            return new DateTime(int.Parse(partes[2]),
                                int.Parse(partes[1]), int.Parse(partes[0]));
                        return DateTime.MinValue;
                    }).ToArray();

                root.Add(new XElement("cliente",
                    new XElement("NIT", cliente.NIT),
                    new XElement("nombre", cliente.Nombre),
                    new XElement("saldoActual", saldo.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture)),
                    new XElement("transacciones", transOrdenadas)));
            }

            return new XDocument(root);
        }

        // ─── RESUMEN PAGOS ───────────────────────────────────
        public XDocument ObtenerResumenPagos(int mes, int anio)
        {
            var pagos = ObtenerPagos();
            var bancos = ObtenerBancos();

            var meses = new ListaEnlazada<(int m, int a)>();
            for (int i = 0; i < 3; i++)
            {
                meses.Agregar((mes, anio));
                mes--;
                if (mes == 0) { mes = 12; anio--; }
            }

            var root = new XElement("resumenPagos",
                new XElement("mesElegido",
                    $"{meses.First().m:D2}/{meses.First().a}"));

            foreach (var banco in bancos)
            {
                var bancoPagos = new XElement("banco",
                    new XElement("codigo", banco.Codigo),
                    new XElement("nombre", banco.Nombre));

                foreach (var (m, a) in meses)
                {
                    decimal total = pagos
                        .Where(p =>
                        {
                            var partes = p.Fecha.Split('/');
                            if (partes.Length == 3)
                                return p.CodigoBanco == banco.Codigo
                                    && int.Parse(partes[1]) == m
                                    && int.Parse(partes[2]) == a;
                            return false;
                        })
                        .Sum(p => p.Valor);

                    bancoPagos.Add(new XElement("mes",
                        new XElement("periodo", $"{m:D2}/{a}"),
                        new XElement("total", total.ToString("F2",
                            System.Globalization.CultureInfo.InvariantCulture))));
                }
                root.Add(bancoPagos);
            }

            return new XDocument(root);
        }
    }
}