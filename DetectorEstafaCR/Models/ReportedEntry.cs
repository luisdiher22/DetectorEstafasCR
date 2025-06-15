using System;
using System.ComponentModel.DataAnnotations;

namespace DetectorEstafaCR.Models
{
    public class ReportedEntry
    {
        [Key]
        public int Id { get; set; }

        public string? ContactInfo { get; set; } // Number or email

        public string? Message { get; set; }

        public bool IsPotentialScam { get; set; }

        public int ReportCount { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
