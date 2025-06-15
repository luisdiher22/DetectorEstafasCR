using System.Collections.Generic;

namespace DetectorEstafaCR.Models
{
    public class DetectResultViewModel
    {
        public string? OriginalContactInfo { get; set; }
        public string? OriginalMessage { get; set; }
        public bool IsPotentialScam { get; set; }
        public List<string> AnalysisDetails { get; set; } = new List<string>();
        public int ReportCount { get; set; }
    }
}
