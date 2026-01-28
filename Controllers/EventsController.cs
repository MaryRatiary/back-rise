using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rise.API.DTOs;
using Rise.API.Services;
using System.Security.Claims;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Rise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EventDTO>>> GetAllEvents()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userIdGuid = userId != null ? Guid.Parse(userId) : (Guid?)null;
            var events = await _eventService.GetAllEventsAsync(userIdGuid);
            return Ok(events);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<ActionResult<List<EventDTO>>> GetAllEventsForAdmin()
        {
            var events = await _eventService.GetAllEventsForAdminAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDTO>> GetEventById(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userIdGuid = userId != null ? Guid.Parse(userId) : (Guid?)null;
            var @event = await _eventService.GetEventByIdAsync(id, userIdGuid);

            if (@event == null)
                return NotFound();

            return Ok(@event);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<EventDTO>> CreateEvent([FromBody] CreateEventRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var @event = await _eventService.CreateEventAsync(request, userIdGuid);
            if (@event == null)
                return BadRequest(new { message = "Failed to create event" });

            return CreatedAtAction(nameof(GetEventById), new { id = @event.Id }, @event);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<EventDTO>> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var @event = await _eventService.UpdateEventAsync(id, request, userIdGuid);
            if (@event == null)
                return NotFound();

            return Ok(@event);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var success = await _eventService.DeleteEventAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/publish")]
        public async Task<IActionResult> PublishEvent(Guid id)
        {
            var success = await _eventService.PublishEventAsync(id);
            if (!success)
                return NotFound();

            return Ok(new { message = "Event published successfully" });
        }

        [Authorize]
        [HttpPost("{id}/register")]
        public async Task<IActionResult> RegisterToEvent(Guid id, [FromBody] EventRegistrationRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _eventService.RegisterToEventAsync(id, userIdGuid, request.FormSubmissionId);
            if (!success)
                return BadRequest(new { message = "Failed to register to event or form submission required" });

            return Ok(new { message = "Successfully registered to event" });
        }

        [Authorize]
        [HttpPost("{id}/unregister")]
        public async Task<IActionResult> UnregisterFromEvent(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _eventService.UnregisterFromEventAsync(id, userIdGuid);
            if (!success)
                return BadRequest(new { message = "Failed to unregister from event" });

            return Ok(new { message = "Successfully unregistered from event" });
        }

        [Authorize]
        [HttpGet("my-registrations")]
        public async Task<ActionResult<List<EventRegistrationDTO>>> GetMyRegistrations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var registrations = await _eventService.GetUserRegistrationsAsync(userIdGuid);
            return Ok(registrations);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/export-registrations")]
        public async Task<IActionResult> ExportEventRegistrations(Guid id, [FromQuery] string format = "excel")
        {
            try
            {
                var submissions = await _eventService.GetEventRegistrationsWithFormAsync(id);
                
                if (format.ToLower() == "csv")
                {
                    var csv = GenerateCSV(submissions);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"event-registrations-{id}.csv");
                }
                else // Excel par défaut
                {
                    var bytes = GenerateExcel(submissions);
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"event-registrations-{id}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string GenerateCSV(List<FormSubmissionDTO> submissions)
        {
            var csv = new StringBuilder();
            
            if (submissions.Count == 0)
                return "Aucune donnée";

            // En-têtes
            var headers = new List<string> { "Nom", "Email", "Date de soumission" };
            
            // Ajouter les questions comme colonnes
            if (submissions.Count > 0)
            {
                headers.AddRange(submissions[0].Answers.Select(a => a.QuestionTitle));
            }

            csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

            // Données
            foreach (var submission in submissions)
            {
                var row = new List<string>
                {
                    $"\"{submission.UserName}\"",
                    $"\"{submission.UserEmail}\"",
                    $"\"{submission.SubmittedAt:yyyy-MM-dd HH:mm:ss}\""
                };

                foreach (var answer in submission.Answers)
                {
                    var value = answer.OptionText ?? answer.ResponseValue ?? "";
                    row.Add($"\"{value}\"");
                }

                csv.AppendLine(string.Join(",", row));
            }

            return csv.ToString();
        }

        private byte[] GenerateExcel(List<FormSubmissionDTO> submissions)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Réponses au Formulaire");

                if (submissions.Count == 0)
                {
                    worksheet.Cells[1, 1].Value = "Aucune donnée disponible";
                    return package.GetAsByteArray();
                }

                // Configuration des colonnes
                var headers = new List<string> { "Nom", "Email", "Date de soumission" };
                
                if (submissions.Count > 0)
                {
                    headers.AddRange(submissions[0].Answers.Select(a => a.QuestionTitle));
                }

                // Largeur des colonnes
                worksheet.Cells.Style.Font.Name = "Calibri";
                worksheet.Cells.Style.Font.Size = 11;

                // En-tête du formulaire
                var headerRowStart = 1;
                for (int col = 0; col < headers.Count; col++)
                {
                    var cell = worksheet.Cells[headerRowStart, col + 1];
                    cell.Value = headers[col];
                    
                    // Style en-tête
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 102, 204)); // Bleu
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.WrapText = true;
                }

                // Ajouter les données
                int dataRowStart = headerRowStart + 1;
                for (int row = 0; row < submissions.Count; row++)
                {
                    var submission = submissions[row];
                    int excelRow = dataRowStart + row;

                    // Informations utilisateur
                    worksheet.Cells[excelRow, 1].Value = submission.UserName;
                    worksheet.Cells[excelRow, 2].Value = submission.UserEmail;
                    worksheet.Cells[excelRow, 3].Value = submission.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss");

                    // Réponses du formulaire
                    for (int col = 0; col < submission.Answers.Count; col++)
                    {
                        var answer = submission.Answers[col];
                        var cell = worksheet.Cells[excelRow, col + 4];
                        
                        // Formater la réponse selon son type
                        var responseValue = FormatAnswerForExcel(answer);
                        cell.Value = responseValue;
                        
                        // Styles pour les cellules de données
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        cell.Style.WrapText = true;
                        
                        // Alternance de couleurs pour meilleure lisibilité
                        if (row % 2 == 0)
                        {
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242)); // Gris clair
                        }
                    }

                    // Style pour les 3 premières colonnes
                    for (int col = 1; col <= 3; col++)
                    {
                        var cell = worksheet.Cells[excelRow, col];
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        cell.Style.WrapText = true;
                        
                        if (row % 2 == 0)
                        {
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                        }
                    }
                }

                // Ajouter des bordures
                var range = worksheet.Cells[headerRowStart, 1, dataRowStart + submissions.Count - 1, headers.Count];
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Color.SetColor(Color.Gray);
                range.Style.Border.Right.Color.SetColor(Color.Gray);
                range.Style.Border.Top.Color.SetColor(Color.Gray);
                range.Style.Border.Bottom.Color.SetColor(Color.Gray);

                // Ajuster la largeur des colonnes automatiquement
                for (int col = 1; col <= headers.Count; col++)
                {
                    double maxLength = 0;
                    
                    // Vérifier la longueur du header
                    var headerCell = worksheet.Cells[headerRowStart, col];
                    if (headerCell.Value != null)
                    {
                        maxLength = headerCell.Value.ToString()?.Length ?? 0;
                    }
                    
                    // Vérifier la longueur des données
                    for (int row = dataRowStart; row < dataRowStart + submissions.Count; row++)
                    {
                        var cell = worksheet.Cells[row, col];
                        if (cell.Value != null)
                        {
                            var cellLength = cell.Value.ToString()?.Length ?? 0;
                            maxLength = Math.Max(maxLength, cellLength);
                        }
                    }
                    
                    // Définir la largeur (avec un minimum et maximum)
                    worksheet.Column(col).Width = Math.Min(Math.Max(maxLength + 2, 15), 50);
                }

                // Geler la première ligne
                worksheet.View.FreezePanes(2, 1);

                // Ajouter un sommaire en bas
                int summaryRow = dataRowStart + submissions.Count + 2;
                worksheet.Cells[summaryRow, 1].Value = $"Total de réponses: {submissions.Count}";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 1].Style.Font.Size = 12;

                return package.GetAsByteArray();
            }
        }

        private string FormatAnswerForExcel(AnswerDTO answer)
        {
            // Convertir l'enum en string pour la comparaison
            var questionTypeString = answer.QuestionType.ToString().ToLower();
            
            // Formater la réponse selon son type
            return questionTypeString switch
            {
                "multiplechoice" => answer.OptionText ?? "Non répondu",
                "checkbox" => answer.OptionText ?? "Non répondu",
                "shorttext" => answer.ResponseValue ?? "Non répondu",
                "longtext" => answer.ResponseValue ?? "Non répondu",
                "email" => answer.ResponseValue ?? "Non répondu",
                "phone" => answer.ResponseValue ?? "Non répondu",
                "number" => answer.ResponseValue ?? "Non répondu",
                "date" => answer.ResponseValue ?? "Non répondu",
                "rating" => answer.ResponseValue ?? "Non répondu",
                "linearscale" => answer.ResponseValue ?? "Non répondu",
                _ => answer.OptionText ?? answer.ResponseValue ?? "Non répondu"
            };
        }
    }
}
