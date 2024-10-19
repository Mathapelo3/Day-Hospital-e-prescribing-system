using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using iText.Layout;

namespace Day_Hospital_e_prescribing_system
{
    public class AdministerMedsReportGenerator
    {
        private readonly ILogger<AdministerMedsReportGenerator> _logger;
        private readonly ApplicationDbContext _context;

        public AdministerMedsReportGenerator(ILogger<AdministerMedsReportGenerator> logger, ApplicationDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private List<AdministerMedsReportVM> GetMedsReportData(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation($"Executing stored procedure with StartDate: {startDate}, EndDate: {endDate}");

                var meds = _context.AdministerMedsReportVM
                    .FromSqlInterpolated($"EXEC AdministerMedsReport @StartDate = {startDate}, @EndDate = {endDate}")
                    .ToList();

                _logger.LogInformation($"Retrieved {meds.Count} order(s) from the database.");
                return meds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order report data.");
                throw;
            }
        }

        public MemoryStream GenerateMedsReport(DateTime startDate, DateTime endDate, string nurseName, string nurseSurname)
        {
            var meds = GetMedsReportData(startDate, endDate);
            var ms = new MemoryStream();

            try
            {
                using (var writer = new PdfWriter(ms, new WriterProperties().SetCompressionLevel(9)))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        // Attach the page number event handler
                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageNumberEventHandler());

                        var document = new Document(pdf);

                        // Add title "NURSE REPORT" aligned to the left
                        var title = new Paragraph("NURSE REPORT")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(16); // Adjust font size as needed

                        document.Add(title);

                        // Add subtitle with anesthesiologist info centered
                        document.Add(new Paragraph("MEDICATION REPORT")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBold()
                            .SetMarginTop(20));
                        document.Add(new Paragraph($" {nurseName} {nurseSurname}")
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
                        var groupedMedication = meds
                            .GroupBy(o => o.Date)
                            .OrderBy(g => g.Key)
                            .ToList();

                        // Create the table for order data (without borders except for the header)
                        var table1 = new Table(UnitValue.CreatePercentArray(new float[] { 20, 20, 20, 10 }))
                            .SetWidth(UnitValue.CreatePercentValue(70)); // Set table width to 70%

                        // Define the border for the bottom of the header row
                        var bottomBorder = new SolidBorder(1); // You can adjust the thickness here

                        // Add header row for the first table with bottom border only
                        table1.AddCell(new Cell().Add(new Paragraph("DATE")).SetBorderBottom(bottomBorder).SetBorderTop(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("PATIENT")).SetBorderBottom(bottomBorder).SetBorderTop(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("MEDICATION")).SetBorderBottom(bottomBorder).SetBorderTop(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("QTY")).SetBorderBottom(bottomBorder).SetBorderTop(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("TIME")).SetBorderBottom(bottomBorder).SetBorderTop(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));

                        // Add the rest of the rows with no borders
                        foreach (var group in groupedMedication)
                        {
                            // Add the date once for each date group without borders
                            table1.AddCell(new Cell().Add(new Paragraph(group.Key.ToString("yyyy-MM-dd"))).SetBorder(Border.NO_BORDER));
                            table1.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                            table1.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                            table1.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                            table1.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));


                            foreach (var med in group)
                            {
                              
                                // Remove borders for all other cells
                                table1.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                                table1.AddCell(new Cell().Add(new Paragraph($"{med.PatientName} {med.PatientSurname}")).SetBorder(Border.NO_BORDER));
                                table1.AddCell(new Cell().Add(new Paragraph(med.MedicationName)).SetBorder(Border.NO_BORDER));
                                table1.AddCell(new Cell().Add(new Paragraph(med.Quantity)).SetBorder(Border.NO_BORDER));
                                table1.AddCell(new Cell().Add(new Paragraph(med.Time.ToString(@"hh\:mm"))).SetBorder(Border.NO_BORDER));
                            }
                        }
                        

                        // Add the table to the document
                        document.Add(table1);

                        // Add total patients paragraph
                        int totalPatients = meds.Select(o => o.PatientName).Distinct().Count();
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

                        // Define the border for the bottom of the header row
                        /*var bottomBorder = new SolidBorder(1);*/ // Adjust the thickness as needed

                        // Add header row with bottom border only
                        table2.AddCell(new Cell().Add(new Paragraph("MEDICATION"))
                              .SetBorderBottom(bottomBorder) // Add bottom border
                              .SetBorderTop(Border.NO_BORDER)
                              .SetBorderLeft(Border.NO_BORDER)
                              .SetBorderRight(Border.NO_BORDER));

                        table2.AddCell(new Cell().Add(new Paragraph("QTY ADMINISTRERED"))
                            .SetBorderBottom(bottomBorder) // Add bottom border
                            .SetBorderTop(Border.NO_BORDER)
                            .SetBorderLeft(Border.NO_BORDER)
                            .SetBorderRight(Border.NO_BORDER));
                        // Add the rest of the rows with no borders
                        var medicationQuantities = meds
                            .GroupBy(o => o.MedicationName)
                            .Select(g => new
                            {
                                MedicationName = g.Key,
                                Quantity = g.Sum(o => int.TryParse(o.Quantity, out int qty) ? qty : 0)
                            });

                        foreach (var medicationQuantity in medicationQuantities)
                        {
                            table2.AddCell(new Cell().Add(new Paragraph(medicationQuantity.MedicationName))
                                .SetBorder(Border.NO_BORDER));

                            table2.AddCell(new Cell().Add(new Paragraph(medicationQuantity.Quantity.ToString()))
                                .SetBorder(Border.NO_BORDER));
                        }

                        // Add the second table to the document
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

        // Custom event handler class to add page numbers
        private class PageNumberEventHandler : IEventHandler
        {
            public void HandleEvent(Event @event)
            {
                PdfDocumentEvent pdfDocEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = pdfDocEvent.GetDocument();
                PdfPage page = pdfDocEvent.GetPage();
                int pageNumber = pdfDoc.GetPageNumber(page);
                int totalPages = pdfDoc.GetNumberOfPages();

                // Create the PdfCanvas to write the page number
                PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

                // Write the page number at the bottom of the page
                pdfCanvas.BeginText();
                pdfCanvas.SetFontAndSize(PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA), 10);
                pdfCanvas.MoveText(page.GetPageSize().GetWidth() / 2 - 10, 20); // Adjust position for your needs
                pdfCanvas.ShowText($"Page {pageNumber} of {totalPages}");
                pdfCanvas.EndText();
                pdfCanvas.Release();
            }
        }



    }
}
