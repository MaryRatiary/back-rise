namespace Rise.API.DTOs
{
    public class CreateVoteRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CreateVotePositionRequest> Positions { get; set; } = new();
    }

    public class CreateVotePositionRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
    }

    public class VoteDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool ResultsPublished { get; set; }
        public List<VotePositionDTO> Positions { get; set; } = new();
    }

    public class VotePositionDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public List<VoteOptionDTO> Options { get; set; } = new();
    }

    public class VoteOptionDTO
    {
        public Guid Id { get; set; }
        public string? CandidateName { get; set; }
        public string? CandidateDescription { get; set; }
        public string? CandidateProfileUrl { get; set; }
        public bool IsApproved { get; set; }
        public int VoteCount { get; set; }
        public double Percentage { get; set; }
    }

    public class SubmitCandidacyRequest
    {
        public Guid PositionId { get; set; }
        public string? CandidateDescription { get; set; }
    }

    public class CastVoteRequest
    {
        public Guid VoteId { get; set; }
        public Guid PositionId { get; set; }
        public Guid OptionId { get; set; }
    }
}
