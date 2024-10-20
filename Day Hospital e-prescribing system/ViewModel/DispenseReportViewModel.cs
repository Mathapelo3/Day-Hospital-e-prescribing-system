namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class DispenseReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<DispenseReportDataViewModel> ReportData { get; set; } = new List<DispenseReportDataViewModel>();
    }

}
