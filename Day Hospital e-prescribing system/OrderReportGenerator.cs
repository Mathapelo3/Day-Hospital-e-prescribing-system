using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Day_Hospital_e_prescribing_system.ViewModel;
using Day_Hospital_e_prescribing_system.Models;
using iText.Layout.Borders;

namespace Day_Hospital_e_prescribing_system
{
    public class OrderReportGenerator
    {
        private readonly ILogger<OrderReportGenerator> _logger;
        private readonly ApplicationDbContext _context;

        public OrderReportGenerator(ILogger<OrderReportGenerator> logger, ApplicationDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private List<OrderReportDataViewModel> GetOrderReportData(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation($"Executing stored procedure with StartDate: {startDate}, EndDate: {endDate}");

                var orders = _context.OrderReportDataViewModel
                    .FromSqlInterpolated($"EXEC GetOrderReportData @StartDate = {startDate}, @EndDate = {endDate}")
                    .ToList();

                _logger.LogInformation($"Retrieved {orders.Count} order(s) from the database.");
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order report data.");
                throw;
            }
        }

        public MemoryStream GenerateOrderReport(DateTime startDate, DateTime endDate, string anaesthesiologistName, string anaesthesiologistSurname)
        {
            var orders = GetOrderReportData(startDate, endDate);
            var ms = new MemoryStream();

            try
            {
                using (var writer = new PdfWriter(ms, new WriterProperties().SetCompressionLevel(9)))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);

                        // Add title "ANESTHESIOLOGIST REPORT" aligned to the left
                        var title = new Paragraph("ANESTHESIOLOGIST REPORT")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(16); // Adjust font size as needed

                        document.Add(title);

                        // Add subtitle with anesthesiologist info centered
                        document.Add(new Paragraph("ANESTHETIC REPORT")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBold()
                            .SetMarginTop(20));
                        document.Add(new Paragraph($"Dr. {anaesthesiologistName} {anaesthesiologistSurname}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBold());

                        // Create a table with two columns for the date range and generated date
                        var infoTable = new Table(UnitValue.CreatePercentArray(2))
                            .SetWidth(UnitValue.CreatePercentValue(100)); // Set table width to 100%

                        infoTable.AddCell(new Cell()
                            .Add(new Paragraph($"Date Range: {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}")
                                .SetTextAlignment(TextAlignment.LEFT))
                            .SetBorder(Border.NO_BORDER));

                        infoTable.AddCell(new Cell()
                            .Add(new Paragraph($"Report Generated: {DateTime.Now:yyyy-MM-dd}")
                                .SetTextAlignment(TextAlignment.RIGHT))
                            .SetBorder(Border.NO_BORDER));

                        document.Add(infoTable);

                        // Sort orders by date and group them
                        var groupedOrders = orders
                            .GroupBy(o => o.Date)
                            .OrderBy(g => g.Key)
                            .ToList();

                        // Create the table for order data
                        var table1 = new Table(UnitValue.CreatePercentArray(new float[] { 20, 20, 20, 10 }))
                            .SetWidth(UnitValue.CreatePercentValue(70)); // Set table width to 70%

                        // Add header row with bottom border
                        table1.AddCell(new Cell().Add(new Paragraph("DATE")).SetBorder(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("PATIENT")).SetBorder(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("MEDICATION")).SetBorder(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("QTY")).SetBorder(Border.NO_BORDER));

                        // Add bottom border to the header row only
                        var headerBorder = new SolidBorder(1);
                        
                        table1.SetBorderBottom(headerBorder);

                        foreach (var group in groupedOrders)
                        {
                            // Add date cell once for each date group
                            table1.AddCell(group.Key.ToString("yyyy-MM-dd"))
                                  .AddCell(string.Empty)
                                  .AddCell(string.Empty)
                                  .AddCell(string.Empty);

                            // Add order rows for each date
                            foreach (var order in group)
                            {
                                table1.AddCell(string.Empty) // Placeholder for date
                                      .AddCell($"{order.PatientName} {order.PatientSurname}")
                                      .AddCell(order.MedicationName)
                                      .AddCell(order.Quantity);
                            }
                        }

                        document.Add(table1);

                        // Add total patients paragraph
                        int totalPatients = orders.Select(o => o.PatientName).Distinct().Count();
                        var totalPatientsParagraph = new Paragraph($"TOTAL PATIENTS: {totalPatients}")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetBold()
                            .SetMarginTop(10); // Add margin to create space

                        document.Add(totalPatientsParagraph);

                        // Add title above the second table
                        var summaryTitle = new Paragraph("SUMMARY PER MEDICINE:")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetBold()
                            .SetFontSize(12)
                            .SetMarginTop(20); // Add margin to create space above the title

                        document.Add(summaryTitle);

                        // Create the second table with medication summary
                        var table2 = new Table(UnitValue.CreatePercentArray(new float[] { 30, 10 }))
                            .SetWidth(UnitValue.CreatePercentValue(30)); // Set table width to 30%

                        // Add header row with bottom border
                        table2.AddCell(new Cell().Add(new Paragraph("MEDICATION")).SetBorder(Border.NO_BORDER))
                              .AddCell(new Cell().Add(new Paragraph("QTY")).SetBorder(Border.NO_BORDER));

                        // Add bottom border to the header row only
                        
                        table2.SetBorderBottom(headerBorder);

                        var medicationQuantities = orders
                            .GroupBy(o => o.MedicationName)
                            .Select(g => new
                            {
                                MedicationName = g.Key,
                                Quantity = g.Sum(o => int.TryParse(o.Quantity, out int qty) ? qty : 0)
                            });

                        foreach (var medicationQuantity in medicationQuantities)
                        {
                            table2.AddCell(medicationQuantity.MedicationName)
                                  .AddCell(medicationQuantity.Quantity.ToString());
                        }

                        document.Add(table2);
                        document.Close();
                    }
                }

                // Create a copy of the MemoryStream to return
                var reportStream = new MemoryStream(ms.ToArray());
                return reportStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating PDF report.");
                throw;
            }
        }



    }
}
