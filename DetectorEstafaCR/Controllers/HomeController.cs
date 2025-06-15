using DetectorEstafaCR.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DetectorEstafaCR.Data; // Added
using Microsoft.EntityFrameworkCore; // Added
using System.Linq; // Added
using System.Threading.Tasks; // Added
using System.Collections.Generic; // Added for List

namespace DetectorEstafaCR.Controllers
{
    // Simple model for form binding in POST Detect action
    public class DetectInputModel
    {
        public string? ContactInfo { get; set; }
        public string? Message { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Added

        // Updated constructor
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Added
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Detect
        [HttpGet]
        public IActionResult Detect()
        {
            return View(new DetectInputModel()); // Pass an empty model for form generation
        }

        // POST: /Home/Detect
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Detect([Bind("ContactInfo,Message")] DetectInputModel input)
        {
            var viewModel = new DetectResultViewModel
            {
                OriginalContactInfo = input.ContactInfo,
                OriginalMessage = input.Message,
                IsPotentialScam = false, // Default to not a scam
                AnalysisDetails = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(input.ContactInfo) && string.IsNullOrWhiteSpace(input.Message))
            {
                ModelState.AddModelError("", "Debe proporcionar un número/correo o un mensaje.");
                return View(input); // Return to the form view with an error
            }

            // Define scam keywords (simple list)
            var scamKeywords = new List<string>
            {
                "premio", "oferta", "urgente", "banco", "contraseña", "ganaste", "gratis",
                "acceso", "exclusivo", "verificar", "actualizar", "suspension", "limitado",
                "factura", "reembolso", "soporte técnico", "inversion", "crypto"
            };

            // 1. Check if ContactInfo was previously reported
            ReportedEntry? existingEntry = null;
            if (!string.IsNullOrWhiteSpace(input.ContactInfo))
            {
                existingEntry = await _context.ReportedEntries
                                         .FirstOrDefaultAsync(r => r.ContactInfo == input.ContactInfo);
                if (existingEntry != null)
                {
                    viewModel.IsPotentialScam = true; // If contact is known, flag it
                    viewModel.AnalysisDetails.Add($"El contacto '{input.ContactInfo}' ha sido reportado previamente.");
                    viewModel.ReportCount = existingEntry.ReportCount;
                }
            }

            // 2. Analyze message content
            if (!string.IsNullOrWhiteSpace(input.Message))
            {
                // Keyword check
                foreach (var keyword in scamKeywords)
                {
                    if (input.Message.ToLower().Contains(keyword))
                    {
                        viewModel.IsPotentialScam = true;
                        viewModel.AnalysisDetails.Add($"Palabra clave sospechosa encontrada: '{keyword}'.");
                    }
                }

                // URL check (simple)
                if (input.Message.ToLower().Contains("http://") || input.Message.ToLower().Contains("https://"))
                {
                    viewModel.IsPotentialScam = true; // Containing a URL increases suspicion
                    viewModel.AnalysisDetails.Add("El mensaje contiene un enlace (URL).");
                }
            }

            if (!viewModel.IsPotentialScam && viewModel.AnalysisDetails.Count == 0)
            {
                viewModel.AnalysisDetails.Add("No se encontraron indicadores obvios de estafa en este análisis inicial.");
            }


            // 3. Database update
            if (!string.IsNullOrWhiteSpace(input.ContactInfo)) // Only save/update if ContactInfo is present
            {
                if (existingEntry != null)
                {
                    existingEntry.ReportCount++;
                    existingEntry.IsPotentialScam = existingEntry.IsPotentialScam || viewModel.IsPotentialScam; // Update if newly found as scam
                    // Optionally update message if new one is more informative, or store multiple messages? For now, just count.
                    _context.ReportedEntries.Update(existingEntry);
                }
                else
                {
                    existingEntry = new ReportedEntry
                    {
                        ContactInfo = input.ContactInfo,
                        Message = input.Message, // Store the message with the first report of this contact
                        IsPotentialScam = viewModel.IsPotentialScam,
                        ReportCount = 1,
                        Timestamp = DateTime.UtcNow
                    };
                    await _context.ReportedEntries.AddAsync(existingEntry);
                }
                await _context.SaveChangesAsync();
                viewModel.ReportCount = existingEntry.ReportCount; // Ensure viewmodel has latest count
            } else if (viewModel.IsPotentialScam) {
                // If it's a scam based on message but no contact info, we could log it differently
                // For now, we are not saving entries without ContactInfo to keep DB focused on reportable contacts
                viewModel.AnalysisDetails.Add("El mensaje parece sospechoso, pero no se proporcionó información de contacto para registrarlo en la base de datos.");
            }


            return View("DetectResult", viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
