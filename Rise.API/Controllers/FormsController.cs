using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rise.API.DTOs;
using Rise.API.Services;
using System.Security.Claims;

namespace Rise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormsController : ControllerBase
    {
        private readonly IFormService _formService;
        private readonly IFormAnalyticsService _analyticsService;

        public FormsController(IFormService formService, IFormAnalyticsService analyticsService)
        {
            _formService = formService;
            _analyticsService = analyticsService;
        }

        // ===== FORM MANAGEMENT =====

        /// <summary>
        /// Crée un nouveau formulaire (Admin uniquement)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<FormDTO>> CreateForm([FromBody] CreateFormRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var form = await _formService.CreateFormAsync(request, userIdGuid);
            if (form == null)
                return BadRequest(new { message = "Erreur lors de la création du formulaire" });

            return CreatedAtAction(nameof(GetFormById), new { id = form.Id }, form);
        }

        /// <summary>
        /// Récupère tous les formulaires actifs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FormDTO>>> GetAllForms()
        {
            var forms = await _formService.GetAllFormsAsync();
            return Ok(forms);
        }

        /// <summary>
        /// Récupère les formulaires créés par l'utilisateur connecté
        /// </summary>
        [Authorize]
        [HttpGet("my-forms")]
        public async Task<ActionResult<List<FormDTO>>> GetMyForms()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var forms = await _formService.GetFormsByUserAsync(userIdGuid);
            return Ok(forms);
        }

        /// <summary>
        /// Récupère un formulaire spécifique par ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FormDTO>> GetFormById(Guid id)
        {
            var form = await _formService.GetFormByIdAsync(id);
            if (form == null)
                return NotFound(new { message = "Formulaire non trouvé" });

            return Ok(form);
        }

        /// <summary>
        /// Modifie un formulaire (Admin uniquement, avant publication)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<FormDTO>> UpdateForm(Guid id, [FromBody] UpdateFormRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _formService.UpdateFormAsync(id, request, userIdGuid);
            if (!success)
                return BadRequest(new { message = "Impossible de modifier ce formulaire" });

            var form = await _formService.GetFormByIdAsync(id);
            return Ok(form);
        }

        /// <summary>
        /// Supprime un formulaire (Admin uniquement)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForm(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _formService.DeleteFormAsync(id, userIdGuid);
            if (!success)
                return BadRequest(new { message = "Impossible de supprimer ce formulaire" });

            return Ok(new { message = "Formulaire supprimé avec succès" });
        }

        /// <summary>
        /// Publie un formulaire (Admin uniquement)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/publish")]
        public async Task<ActionResult<FormDTO>> PublishForm(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _formService.PublishFormAsync(id, userIdGuid);
            if (!success)
                return BadRequest(new { message = "Impossible de publier ce formulaire (doit avoir au moins une question)" });

            var form = await _formService.GetFormByIdAsync(id);
            return Ok(form);
        }

        // ===== QUESTION MANAGEMENT =====

        /// <summary>
        /// Ajoute une question au formulaire
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{formId}/questions")]
        public async Task<ActionResult<FormQuestionDTO>> AddQuestion(Guid formId, [FromBody] CreateFormQuestionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var question = await _formService.AddQuestionAsync(formId, request, userIdGuid);
            if (question == null)
                return BadRequest(new { message = "Impossible d'ajouter la question" });

            return CreatedAtAction(nameof(GetFormById), new { id = formId }, question);
        }

        /// <summary>
        /// Modifie une question
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{formId}/questions/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(Guid formId, Guid questionId, [FromBody] UpdateFormQuestionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _formService.UpdateQuestionAsync(formId, questionId, request, userIdGuid);
            if (!success)
                return BadRequest(new { message = "Impossible de modifier cette question" });

            return Ok(new { message = "Question modifiée avec succès" });
        }

        /// <summary>
        /// Supprime une question
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{formId}/questions/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(Guid formId, Guid questionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _formService.DeleteQuestionAsync(formId, questionId, userIdGuid);
            if (!success)
                return BadRequest(new { message = "Impossible de supprimer cette question" });

            return Ok(new { message = "Question supprimée avec succès" });
        }

        // ===== SUBMISSIONS =====

        /// <summary>
        /// Soumet les réponses à un formulaire
        /// </summary>
        [Authorize]
        [HttpPost("{formId}/submit")]
        public async Task<IActionResult> SubmitForm(Guid formId, [FromBody] SubmitFormRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _formService.SubmitFormAsync(formId, userIdGuid, request);
            if (!success)
                return BadRequest(new { message = "Erreur lors de la soumission du formulaire" });

            return Ok(new { message = "Formulaire soumis avec succès" });
        }

        /// <summary>
        /// Récupère toutes les réponses d'un formulaire
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{formId}/submissions")]
        public async Task<ActionResult<List<FormSubmissionDTO>>> GetSubmissions(Guid formId)
        {
            var submissions = await _formService.GetSubmissionsAsync(formId);
            return Ok(submissions);
        }

        /// <summary>
        /// Récupère une réponse spécifique
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("submissions/{submissionId}")]
        public async Task<ActionResult<FormSubmissionDTO>> GetSubmissionById(Guid submissionId)
        {
            var submission = await _formService.GetSubmissionByIdAsync(submissionId);
            if (submission == null)
                return NotFound(new { message = "Réponse non trouvée" });

            return Ok(submission);
        }

        /// <summary>
        /// Récupère les réponses d'un utilisateur spécifique pour un formulaire
        /// </summary>
        [Authorize]
        [HttpGet("{formId}/my-submissions")]
        public async Task<ActionResult<List<FormSubmissionDTO>>> GetMySubmissions(Guid formId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var submissions = await _formService.GetUserSubmissionsAsync(formId, userIdGuid);
            return Ok(submissions);
        }

        // ===== EXPORT =====

        /// <summary>
        /// Exporte les réponses en fichier Excel/CSV
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{formId}/export/excel")]
        public async Task<IActionResult> ExportToExcel(Guid formId)
        {
            var data = await _formService.ExportToExcelAsync(formId);
            if (data.Length == 0)
                return NotFound(new { message = "Formulaire non trouvé" });

            var fileName = $"formulaire_{formId:N}.csv";
            return File(data, "text/csv", fileName);
        }

        /// <summary>
        /// Exporte les réponses en format CSV (texte)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{formId}/export/csv")]
        public async Task<IActionResult> ExportToCsv(Guid formId)
        {
            var data = await _formService.ExportToCsvAsync(formId);
            if (string.IsNullOrEmpty(data))
                return NotFound(new { message = "Formulaire non trouvé" });

            var fileName = $"formulaire_{formId:N}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(data), "text/csv", fileName);
        }

        // ===== ANALYTICS =====

        /// <summary>
        /// Récupère les analytics complètes d'un formulaire (comme PowerBI)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{formId}/analytics")]
        public async Task<ActionResult<FormAnalyticsDTO>> GetFormAnalytics(Guid formId)
        {
            var analytics = await _analyticsService.GetFormAnalyticsAsync(formId);
            if (analytics == null)
                return NotFound(new { message = "Formulaire non trouvé" });

            return Ok(analytics);
        }

        /// <summary>
        /// Récupère les analytics d'une question spécifique
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("questions/{questionId}/analytics")]
        public async Task<ActionResult<QuestionAnalyticsDTO>> GetQuestionAnalytics(Guid questionId)
        {
            var analytics = await _analyticsService.GetQuestionAnalyticsAsync(questionId);
            if (analytics == null)
                return NotFound(new { message = "Question non trouvée" });

            return Ok(analytics);
        }

        /// <summary>
        /// Récupère les données pour un graphique d'une question
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("questions/{questionId}/chart")]
        public async Task<ActionResult<List<ChartDataDTO>>> GetQuestionChartData(Guid questionId)
        {
            var chartData = await _analyticsService.GetQuestionChartDataAsync(questionId);
            return Ok(chartData);
        }

        /// <summary>
        /// Récupère la tendance des réponses au fil du temps
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{formId}/trend")]
        public async Task<ActionResult<ResponseTrendDTO>> GetResponseTrend(Guid formId)
        {
            var trend = await _analyticsService.GetResponseTrendAsync(formId);
            return Ok(trend);
        }

        /// <summary>
        /// Récupère le taux de participation au formulaire
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{formId}/participation")]
        public async Task<ActionResult<ParticipationRateDTO>> GetParticipationRate(Guid formId)
        {
            var participation = await _analyticsService.GetParticipationRateAsync(formId);
            return Ok(participation);
        }

        /// <summary>
        /// Récupère les statistiques du dashboard (tous les formulaires)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("analytics/dashboard")]
        public async Task<ActionResult<Dictionary<string, object>>> GetDashboardStats()
        {
            var stats = await _analyticsService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        // ===== USER SEARCH (FOR TEAM QUESTIONS) =====

        /// <summary>
        /// Recherche les utilisateurs pour l'autocomplétion des équipes
        /// </summary>
        [Authorize]
        [HttpGet("users/search")]
        public async Task<ActionResult<List<UserSearchDTO>>> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Ok(new List<UserSearchDTO>());

            var users = await _formService.SearchUsersAsync(query);
            return Ok(users);
        }
    }
}
