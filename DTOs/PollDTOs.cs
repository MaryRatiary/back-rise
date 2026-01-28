namespace Rise.API.DTOs
{
    public class CreatePollRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TargetAudience { get; set; } = "all";
        public List<CreatePollQuestionRequest> Questions { get; set; } = new();
    }

    public class CreatePollQuestionRequest
    {
        public required string QuestionText { get; set; }
        public bool AllowMultipleChoice { get; set; } = false;
        public List<string> Options { get; set; } = new();
    }

    public class PollDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string TargetAudience { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<PollQuestionDTO> Questions { get; set; } = new();
        public bool HasUserResponded { get; set; }
    }

    public class PollQuestionDTO
    {
        public Guid Id { get; set; }
        public required string QuestionText { get; set; }
        public bool AllowMultipleChoice { get; set; }
        public List<PollOptionDTO> Options { get; set; } = new();
    }

    public class PollOptionDTO
    {
        public Guid Id { get; set; }
        public required string OptionText { get; set; }
        public int ResponseCount { get; set; }
        public double Percentage { get; set; }
    }

    public class PollResponseRequest
    {
        public Guid QuestionId { get; set; }
        public List<Guid> SelectedOptionIds { get; set; } = new();
    }
}
