using Microsoft.EntityFrameworkCore;
using Rise.API.Data;
using Rise.API.DTOs;
using Rise.API.Enums;
using Rise.API.Models;

namespace Rise.API.Services
{
    public class FormAnalyticsService : IFormAnalyticsService
    {
        private readonly RiseDbContext _context;
        
        // ✅ CACHE: Stocker les analytics en cache pendant 5 minutes
        private static Dictionary<Guid, (DateTime ExpiresAt, FormAnalyticsDTO Analytics)> _analyticsCache = new();
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);

        public FormAnalyticsService(RiseDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère les analytics complètes d'un formulaire (avec cache)
        /// </summary>
        public async Task<FormAnalyticsDTO?> GetFormAnalyticsAsync(Guid formId)
        {
            // ✅ Vérifier le cache d'abord
            if (_analyticsCache.TryGetValue(formId, out var cached))
            {
                if (DateTime.UtcNow < cached.ExpiresAt)
                {
                    return cached.Analytics; // Retourner depuis le cache
                }
                else
                {
                    _analyticsCache.Remove(formId); // Cache expiré
                }
            }

            var form = await _context.Forms
                .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
                .Include(f => f.Questions)
                .ThenInclude(q => q.Answers)
                .Include(f => f.Submissions)
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null)
                return null;

            var analytics = new FormAnalyticsDTO
            {
                FormId = form.Id,
                FormTitle = form.Title,
                TotalSubmissions = form.Submissions.Count,
                CreatedAt = form.CreatedAt,
                LastSubmissionAt = form.Submissions.OrderByDescending(s => s.SubmittedAt).FirstOrDefault()?.SubmittedAt,
                CompletionRate = CalculateCompletionRate(form),
                QuestionStats = form.Questions.OrderBy(q => q.Order)
                    .Select(q => CalculateQuestionAnalytics(q, form.Submissions))
                    .ToList()
            };

            // ✅ Stocker dans le cache
            _analyticsCache[formId] = (DateTime.UtcNow.Add(CACHE_DURATION), analytics);

            return analytics;
        }

        /// <summary>
        /// Récupère les analytics d'une question spécifique
        /// </summary>
        public async Task<QuestionAnalyticsDTO?> GetQuestionAnalyticsAsync(Guid questionId)
        {
            var question = await _context.FormQuestions
                .Include(q => q.Options)
                .Include(q => q.Answers)
                .ThenInclude(a => a.Submission)
                .Include(q => q.Form)
                .ThenInclude(f => f.Submissions)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return null;

            return CalculateQuestionAnalytics(question, question.Form.Submissions);
        }

        /// <summary>
        /// Récupère les données pour afficher un graphique d'une question
        /// </summary>
        public async Task<List<ChartDataDTO>> GetQuestionChartDataAsync(Guid questionId)
        {
            var question = await _context.FormQuestions
                .Include(q => q.Options)
                .ThenInclude(o => o.Answers)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return new List<ChartDataDTO>();

            // Pour les questions avec options (MultipleChoice, Checkboxes, Dropdown, Scale)
            if (question.Type == QuestionType.MultipleChoice ||
                question.Type == QuestionType.Checkboxes ||
                question.Type == QuestionType.Dropdown ||
                question.Type == QuestionType.Scale)
            {
                var totalAnswers = question.Answers.Count;
                var chartData = new List<ChartDataDTO>();

                foreach (var option in question.Options.OrderBy(o => o.Order))
                {
                    var count = option.Answers.Count;
                    chartData.Add(new ChartDataDTO
                    {
                        Label = option.OptionText,
                        Value = count,
                        Percentage = totalAnswers > 0 ? Math.Round((count * 100.0) / totalAnswers, 2) : 0
                    });
                }

                return chartData;
            }

            return new List<ChartDataDTO>();
        }

        /// <summary>
        /// Récupère la tendance des réponses au fil du temps
        /// </summary>
        public async Task<ResponseTrendDTO> GetResponseTrendAsync(Guid formId)
        {
            var submissions = await _context.FormSubmissions
                .Where(s => s.FormId == formId)
                .OrderBy(s => s.SubmittedAt)
                .ToListAsync();

            var trend = new ResponseTrendDTO();

            if (!submissions.Any())
                return trend;

            // Grouper par jour
            var groupedByDay = submissions
                .GroupBy(s => s.SubmittedAt.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int cumulativeCount = 0;
            foreach (var dayGroup in groupedByDay)
            {
                cumulativeCount += dayGroup.Count();
                trend.DataPoints.Add(new TrendDataPoint
                {
                    Date = dayGroup.Key,
                    ResponseCount = dayGroup.Count(),
                    CumulativeCount = cumulativeCount
                });
            }

            return trend;
        }

        /// <summary>
        /// Récupère le taux de participation au formulaire
        /// </summary>
        public async Task<ParticipationRateDTO> GetParticipationRateAsync(Guid formId)
        {
            var form = await _context.Forms
                .Include(f => f.Submissions)
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null)
                return new ParticipationRateDTO();

            // Récupérer le nombre total d'utilisateurs actifs
            var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
            var respondedUsers = form.Submissions.Select(s => s.UserId).Distinct().Count();
            var notRespondedUsers = totalUsers - respondedUsers;

            return new ParticipationRateDTO
            {
                TotalUsers = totalUsers,
                RespondedUsers = respondedUsers,
                NotRespondedUsers = notRespondedUsers,
                ParticipationPercentage = totalUsers > 0 ? Math.Round((respondedUsers * 100.0) / totalUsers, 2) : 0
            };
        }

        /// <summary>
        /// Récupère les statistiques du dashboard (tous les formulaires)
        /// </summary>
        public async Task<Dictionary<string, object>> GetDashboardStatsAsync()
        {
            var stats = new Dictionary<string, object>();

            // Nombre total de formulaires
            var totalForms = await _context.Forms.CountAsync(f => f.IsActive);
            stats["TotalForms"] = totalForms;

            // Nombre total de réponses
            var totalSubmissions = await _context.FormSubmissions.CountAsync();
            stats["TotalSubmissions"] = totalSubmissions;

            // Nombre total de questions
            var totalQuestions = await _context.FormQuestions.CountAsync();
            stats["TotalQuestions"] = totalQuestions;

            // Formulaires actifs (publiés)
            var activeForms = await _context.Forms.CountAsync(f => f.IsActive && f.IsPublished);
            stats["ActiveForms"] = activeForms;

            // Formulaires en brouillon
            var draftForms = await _context.Forms.CountAsync(f => f.IsActive && !f.IsPublished);
            stats["DraftForms"] = draftForms;

            // Réponses moyenne par formulaire
            stats["AverageResponsesPerForm"] = totalForms > 0 ? Math.Round((totalSubmissions * 1.0) / totalForms, 2) : 0;

            // Formulaires les plus populaires
            var topForms = await _context.Forms
                .Include(f => f.Submissions)
                .Where(f => f.IsActive)
                .OrderByDescending(f => f.Submissions.Count)
                .Take(5)
                .Select(f => new { f.Title, ResponseCount = f.Submissions.Count })
                .ToListAsync();

            stats["TopForms"] = topForms;

            // Types de questions utilisés
            var questionTypes = await _context.FormQuestions
                .GroupBy(q => q.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            stats["QuestionTypeDistribution"] = questionTypes;

            return stats;
        }

        // ===== HELPERS =====

        /// <summary>
        /// Calcule le taux de complétude d'un formulaire
        /// </summary>
        private double CalculateCompletionRate(Form form)
        {
            if (form.Submissions.Count == 0)
                return 0;

            var totalPossibleAnswers = form.Submissions.Count * form.Questions.Count;
            var totalActualAnswers = form.Questions.Sum(q => q.Answers.Count);

            return totalPossibleAnswers > 0 ? Math.Round((totalActualAnswers * 100.0) / totalPossibleAnswers, 2) : 0;
        }

        /// <summary>
        /// Calcule les analytics d'une question
        /// </summary>
        private QuestionAnalyticsDTO CalculateQuestionAnalytics(FormQuestion question, ICollection<FormSubmission> submissions)
        {
            var totalSubmissions = submissions.Count;
            var totalResponses = question.Answers.Count;
            var skippedCount = totalSubmissions - totalResponses;

            var analytics = new QuestionAnalyticsDTO
            {
                QuestionId = question.Id,
                QuestionTitle = question.Title,
                Type = question.Type,
                TotalResponses = totalResponses,
                SkippedCount = skippedCount,
                SkipRate = totalSubmissions > 0 ? Math.Round((skippedCount * 100.0) / totalSubmissions, 2) : 0
            };

            // Analyser par type de question
            switch (question.Type)
            {
                case QuestionType.ShortText:
                case QuestionType.LongText:
                case QuestionType.Email:
                    // Récupérer les réponses texte
                    analytics.TextResponses = question.Answers
                        .Where(a => !string.IsNullOrEmpty(a.ResponseValue))
                        .Select(a => a.ResponseValue!)
                        .Take(100) // Limiter à 100 réponses
                        .ToList();
                    break;

                case QuestionType.Number:
                    // Calcul des statistiques numériques
                    var numberAnswers = question.Answers
                        .Where(a => !string.IsNullOrEmpty(a.ResponseValue) && double.TryParse(a.ResponseValue, out _))
                        .Select(a => double.Parse(a.ResponseValue!))
                        .ToList();

                    if (numberAnswers.Any())
                    {
                        analytics.AverageValue = Math.Round(numberAnswers.Average(), 2);
                        analytics.MinValue = numberAnswers.Min();
                        analytics.MaxValue = numberAnswers.Max();
                    }
                    break;

                case QuestionType.MultipleChoice:
                case QuestionType.Checkboxes:
                case QuestionType.Dropdown:
                case QuestionType.Scale:
                    // Analyser les réponses par option
                    foreach (var option in question.Options.OrderBy(o => o.Order))
                    {
                        var count = option.Answers.Count;
                        analytics.Options.Add(new OptionAnalyticsDTO
                        {
                            OptionId = option.Id,
                            OptionText = option.OptionText,
                            Count = count,
                            Percentage = totalResponses > 0 ? Math.Round((count * 100.0) / totalResponses, 2) : 0
                        });
                    }
                    break;
            }

            return analytics;
        }
    }
}
