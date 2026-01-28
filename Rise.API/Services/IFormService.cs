using Rise.API.DTOs;
using Rise.API.Models;

namespace Rise.API.Services
{
    public interface IFormService
    {
        // Form Management
        Task<FormDTO?> CreateFormAsync(CreateFormRequest request, Guid userId);
        Task<FormDTO?> GetFormByIdAsync(Guid formId);
        Task<List<FormDTO>> GetAllFormsAsync();
        Task<List<FormDTO>> GetFormsByUserAsync(Guid userId);
        Task<bool> UpdateFormAsync(Guid formId, UpdateFormRequest request, Guid userId);
        Task<bool> DeleteFormAsync(Guid formId, Guid userId);
        Task<bool> PublishFormAsync(Guid formId, Guid userId);

        // Question Management
        Task<FormQuestionDTO?> AddQuestionAsync(Guid formId, CreateFormQuestionRequest request, Guid userId);
        Task<bool> UpdateQuestionAsync(Guid formId, Guid questionId, UpdateFormQuestionRequest request, Guid userId);
        Task<bool> DeleteQuestionAsync(Guid formId, Guid questionId, Guid userId);

        // Submissions
        Task<bool> SubmitFormAsync(Guid formId, Guid userId, SubmitFormRequest request);
        Task<List<FormSubmissionDTO>> GetSubmissionsAsync(Guid formId);
        Task<FormSubmissionDTO?> GetSubmissionByIdAsync(Guid submissionId);
        Task<List<FormSubmissionDTO>> GetUserSubmissionsAsync(Guid formId, Guid userId);

        // Export
        Task<byte[]> ExportToExcelAsync(Guid formId);
        Task<string> ExportToCsvAsync(Guid formId);

        // User Search (for team autocomplete)
        Task<List<UserSearchDTO>> SearchUsersAsync(string query);
    }
}
