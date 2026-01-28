using Rise.API.Enums;

namespace Rise.API.DTOs
{
    // ===== FORM DTOs =====
    public class CreateFormRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TargetAudience { get; set; } = "all";
        public bool AllowMultipleResponses { get; set; } = false;
        public List<CreateFormQuestionRequest> Questions { get; set; } = new();
    }

    public class UpdateFormRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TargetAudience { get; set; }
        public bool? AllowMultipleResponses { get; set; }
    }

    public class FormDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPublished { get; set; }
        public bool IsActive { get; set; }
        public string? TargetAudience { get; set; }
        public bool AllowMultipleResponses { get; set; }
        public int TotalResponses { get; set; }
        public CreatedByUserDto? CreatedBy { get; set; }
        public List<FormQuestionDTO> Questions { get; set; } = new();
    }

    public class CreatedByUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
    }

    // ===== QUESTION DTOs =====
    public class CreateFormQuestionRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; } = false;
        public int Order { get; set; }
        public List<string> Options { get; set; } = new(); // Pour MultipleChoice, Checkboxes, Dropdown
    }

    public class UpdateFormQuestionRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsRequired { get; set; }
        public int? Order { get; set; }
        public List<string>? Options { get; set; }
    }

    public class FormQuestionDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public List<QuestionOptionDTO> Options { get; set; } = new();
    }

    public class QuestionOptionDTO
    {
        public Guid Id { get; set; }
        public required string OptionText { get; set; }
        public int Order { get; set; }
        public int ResponseCount { get; set; } // Pour les analytics
    }

    // ===== SUBMISSION DTOs =====
    public class SubmitFormRequest
    {
        public List<AnswerRequest> Answers { get; set; } = new();
    }

    public class AnswerRequest
    {
        public Guid QuestionId { get; set; }
        public Guid? OptionId { get; set; } // Pour les options
        public string? ResponseValue { get; set; } // Pour les textes, nombres, etc.
        public List<Guid>? TeamMemberIds { get; set; } // Pour les équipes
    }

    public class FormSubmissionDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public DateTime SubmittedAt { get; set; }
        public List<AnswerDTO> Answers { get; set; } = new();
    }

    public class AnswerDTO
    {
        public Guid QuestionId { get; set; }
        public string QuestionTitle { get; set; } = "";
        public QuestionType QuestionType { get; set; }
        public string? OptionText { get; set; } // Si c'est une option
        public string? ResponseValue { get; set; } // Si c'est du texte/nombre
        public List<TeamMemberDTO> TeamMembers { get; set; } = new(); // Si c'est une équipe
    }

    public class TeamMemberDTO
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string MatriculeNumber { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
    }

    public class UserSearchDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string MatriculeNumber { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string DisplayText => $"{FirstName} {LastName} ({Email})";
    }

    // ===== ANALYTICS DTOs =====
    public class FormAnalyticsDTO
    {
        public Guid FormId { get; set; }
        public string FormTitle { get; set; } = "";
        public int TotalSubmissions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastSubmissionAt { get; set; }
        public double CompletionRate { get; set; } // Pourcentage
        public List<QuestionAnalyticsDTO> QuestionStats { get; set; } = new();
    }

    public class QuestionAnalyticsDTO
    {
        public Guid QuestionId { get; set; }
        public string QuestionTitle { get; set; } = "";
        public QuestionType Type { get; set; }
        public int TotalResponses { get; set; }
        public int SkippedCount { get; set; }
        public double SkipRate { get; set; }
        
        // Pour les questions texte
        public List<string>? TextResponses { get; set; }
        
        // Pour les questions avec options
        public List<OptionAnalyticsDTO> Options { get; set; } = new();
        
        // Pour les questions numériques
        public double? AverageValue { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
    }

    public class OptionAnalyticsDTO
    {
        public Guid OptionId { get; set; }
        public string OptionText { get; set; } = "";
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
