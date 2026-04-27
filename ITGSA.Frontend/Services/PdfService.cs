using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ITGSA.Frontend.Services
{
    public class PdfService
    {
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarEstadoCuenta(string nit, string nombre,
            string saldo, List<(string fecha, string cargo, string abono)> transacciones)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("ITGSA - Estado de Cuenta")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Cliente: {nit} – {nombre}").FontSize(12);
                        col.Item().Text($"Saldo actual: Q. {saldo}").FontSize(12);
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Darken2)
                                    .Text("Fecha").FontColor(Colors.White).FontSize(11);
                                header.Cell().Background(Colors.Grey.Darken2)
                                    .Text("Cargo").FontColor(Colors.White).FontSize(11);
                                header.Cell().Background(Colors.Grey.Darken2)
                                    .Text("Abono").FontColor(Colors.White).FontSize(11);
                            });

                            foreach (var (fecha, cargo, abono) in transacciones)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Text(fecha).FontSize(10);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Text(cargo).FontSize(10).FontColor(Colors.Red.Darken1);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Text(abono).FontSize(10).FontColor(Colors.Green.Darken1);
                            }
                        });
                    });

                    page.Footer().AlignCenter()
                        .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            }).GeneratePdf();
        }

        public byte[] GenerarIngresos(string titulo,
            List<string> bancos, List<string> periodos,
            List<List<decimal>> valores)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text($"ITGSA - Ingresos por Banco")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Período: {titulo}").FontSize(12);
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                foreach (var _ in periodos)
                                    c.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2)
                                    .Text("Banco").FontColor(Colors.White).FontSize(11);
                                foreach (var p in periodos)
                                    header.Cell().Background(Colors.Blue.Darken2)
                                        .Text(p).FontColor(Colors.White).FontSize(11);
                            });

                            for (int i = 0; i < bancos.Count; i++)
                            {
                                var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten3;
                                table.Cell().Background(bg)
                                    .Text(bancos[i]).FontSize(10);
                                for (int j = 0; j < periodos.Count; j++)
                                {
                                    decimal val = j < valores.Count && i < valores[j].Count
                                        ? valores[j][i] : 0;
                                    table.Cell().Background(bg)
                                        .Text($"Q. {val:F2}").FontSize(10);
                                }
                            }
                        });
                    });

                    page.Footer().AlignCenter()
                        .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            }).GeneratePdf();
        }
    }
}