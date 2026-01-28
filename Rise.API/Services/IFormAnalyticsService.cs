using Rise.API.DTOs;

namespace Rise.API.Services
{
    public interface IFormAnalyticsService
    {
        Task<FormAnalyticsDTO?> GetFormAnalyticsAsync(Guid formId);
        Task<QuestionAnalyticsDTO?> GetQuestionAnalyticsAsync(Guid questionId);
        Task<Dictionary<string, object>> GetDashboardStatsAsync();
        Task<List<ChartDataDTO>> GetQuestionChartDataAsync(Guid questionId);
        Task<ResponseTrendDTO> GetResponseTrendAsync(Guid formId);
        Task<ParticipationRateDTO> GetParticipationRateAsync(Guid formId);
    }

    // DTOs pour les graphiques et statistiques
    public class ChartDataDTO
    {
        public string Label { get; set; } = "";
        public int Value { get; set; }
        public double Percentage { get; set; }
    }

    public class ResponseTrendDTO
    {
        public List<TrendDataPoint> DataPoints { get; set; } = new();
    }

    public class TrendDataPoint
    {
        public DateTime Date { get; set; }
        public int ResponseCount { get; set; }
        public int CumulativeCount { get; set; }
    }

    public class ParticipationRateDTO
    {
        public int TotalUsers { get; set; }
        public int RespondedUsers { get; set; }
        public int NotRespondedUsers { get; set; }
        public double ParticipationPercentage { get; set; }
    }
}
