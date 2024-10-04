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
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;

namespace Day_Hospital_e_prescribing_system
{
    public class SurgeriesReportGenerator
    {
        private readonly ILogger<SurgeriesReportGenerator> _logger;
        private readonly ApplicationDbContext _context;

        public SurgeriesReportGenerator(ILogger<SurgeriesReportGenerator> logger, ApplicationDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private List<SurgeryReportDataViewModel> GetSurgeriesReportData(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation($"Executing stored procedure with StartDate: {startDate}, EndDate: {endDate}");

                var surgeries = _context.SurgeryReportDataViewModel
                    .FromSqlInterpolated($"EXEC GetSurgeriesReportData @StartDate = {startDate}, @EndDate = {endDate}")
                    .ToList();

                _logger.LogInformation($"Retrieved {surgeries.Count} surgery(ies) from the database.");
                return surgeries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving surgery report data.");
                throw;
            }
        }

        public MemoryStream GenerateSurgeriesReport(DateTime startDate, DateTime endDate, string surgeonName, string surgeonSurname)
        {
            var surgeries = GetSurgeriesReportData(startDate, endDate);
            var ms = new MemoryStream();

            try
            {
                using (var writer = new PdfWriter(ms))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageNumberEventHandler());

                        var document = new Document(pdf);

                        // Add title "SURGEON REPORT" aligned to the left
                        var title = new Paragraph("SURGEON REPORT")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(16);

                        document.Add(title);

                        // Add subtitle with surgeon info centered
                        document.Add(new Paragraph("SURGERY REPORT")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBold()
                            .SetMarginTop(20));
                        document.Add(new Paragraph($"Dr. {surgeonName} {surgeonSurname}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBold());

                        // Create a table with two columns for the date range and generated date
                        var infoTable = new Table(UnitValue.CreatePercentArray(2))
                            .SetWidth(UnitValue.CreatePercentValue(100));

                        infoTable.AddCell(new Cell()
                            .Add(new Paragraph($"Date Range: {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}")
                                .SetTextAlignment(TextAlignment.LEFT))
                            .SetBorder(Border.NO_BORDER));

                        infoTable.AddCell(new Cell()
                            .Add(new Paragraph($"Report Generated: {DateTime.Now:yyyy-MM-dd}")
                                .SetTextAlignment(TextAlignment.RIGHT))
                            .SetBorder(Border.NO_BORDER));

                        document.Add(infoTable);

                        // Create the table for surgeries data
                        var table1 = new Table(UnitValue.CreatePercentArray(new float[] { 20, 20, 40 }))
                            .SetWidth(UnitValue.CreatePercentValue(80));

                        var bottomBorder = new SolidBorder(1);

                        // Add header row
                        table1.AddCell(new Cell().Add(new Paragraph("DATE")).SetBorderBottom(bottomBorder).SetBorderTop(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("PATIENT")).SetBorderBottom(bottomBorder).SetBorderTop(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));
                        table1.AddCell(new Cell().Add(new Paragraph("TREATMENT CODE")).SetBorderBottom(bottomBorder).SetBorderTop(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));

                        foreach (var group in surgeries.GroupBy(s => s.Date).OrderBy(g => g.Key))
                        {
                            table1.AddCell(new Cell().Add(new Paragraph(group.Key.ToString("yyyy-MM-dd"))).SetBorder(Border.NO_BORDER));
                            table1.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                            table1.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));

                            foreach (var surgery in group)
                            {
                                table1.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                                table1.AddCell(new Cell().Add(new Paragraph($"{surgery.PatientName} {surgery.PatientSurname}")).SetBorder(Border.NO_BORDER));
                                table1.AddCell(new Cell().Add(new Paragraph(surgery.TreatmentCodes)).SetBorder(Border.NO_BORDER));
                            }
                        }

                        document.Add(table1);

                        // Add total patients paragraph
                        int totalPatients = surgeries.Select(s => $"{s.PatientName} {s.PatientSurname}").Distinct().Count();
                        var totalPatientsParagraph = new Paragraph($"TOTAL PATIENTS: {totalPatients}")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetBold()
                            .SetMarginTop(10);

                        document.Add(totalPatientsParagraph);

                        // Add title above the second table
                        var summaryTitle = new Paragraph("SUMMARY PER TREATMENT CODE:")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetBold()
                            .SetFontSize(12)
                            .SetMarginTop(20);

                        document.Add(summaryTitle);

                        // Create the second table with treatment code summary
                        var table2 = new Table(UnitValue.CreatePercentArray(new float[] { 30 }))
                            .SetWidth(UnitValue.CreatePercentValue(30));

                        table2.AddCell(new Cell().Add(new Paragraph("TREATMENT CODES"))
                            .SetBorderBottom(bottomBorder)
                            .SetBorderTop(Border.NO_BORDER)
                            .SetBorderLeft(Border.NO_BORDER)
                            .SetBorderRight(Border.NO_BORDER));

                        var uniqueTreatmentCodes = surgeries
                            .SelectMany(s => s.TreatmentCodes.Split(", ", StringSplitOptions.RemoveEmptyEntries))
                            .Distinct()
                            .OrderBy(code => code);

                        foreach (var code in uniqueTreatmentCodes)
                        {
                            table2.AddCell(new Cell().Add(new Paragraph(code))
                                .SetBorder(Border.NO_BORDER));
                        }

                        document.Add(table2);

                        document.Close();
                    }
                }

                return new MemoryStream(ms.ToArray());
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

                PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

                pdfCanvas.BeginText();
                pdfCanvas.SetFontAndSize(PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA), 10);
                pdfCanvas.MoveText(page.GetPageSize().GetWidth() / 2 - 10, 20);
                pdfCanvas.ShowText($"Page {pageNumber} of {totalPages}");
                pdfCanvas.EndText();
                pdfCanvas.Release();
            }
        }

    }
}
