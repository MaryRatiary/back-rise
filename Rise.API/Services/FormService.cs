using Microsoft.EntityFrameworkCore;
using Rise.API.Data;
using Rise.API.DTOs;
using Rise.API.Enums;
using Rise.API.Models;
using System.Text;

namespace Rise.API.Services
{
    public class FormService : IFormService
    {
        private readonly RiseDbContext _context;

        public FormService(RiseDbContext context)
        {
            _context = context;
        }

        // ===== FORM MANAGEMENT =====
        public async Task<FormDTO?> CreateFormAsync(CreateFormRequest request, Guid userId)
        {
            var form = new Form
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TargetAudience = request.TargetAudience,
                AllowMultipleResponses = request.AllowMultipleResponses,
                IsPublished = false,
                IsActive = true
            };

            _context.Forms.Add(form);

            // Ajouter les questions
            int order = 0;
            foreach (var q in request.Questions)
            {
                var question = new FormQuestion
                {
                    Id = Guid.NewGuid(),
                    FormId = form.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Type = q.Type,
                    IsRequired = q.IsRequired,
                    Order = order++,
                    CreatedAt = DateTime.UtcNow
                };

                // Ajouter les options si nécessaire
                if (q.Type == QuestionType.MultipleChoice || q.Type == QuestionType.Checkboxes || q.Type == QuestionType.Dropdown)
                {
                    int optionOrder = 0;
                    foreach (var option in q.Options)
                    {
                        question.Options.Add(new QuestionOption
                        {
                            Id = Guid.NewGuid(),
                            OptionText = option,
                            Order = optionOrder++
                        });
                    }
                }

                form.Questions.Add(question);
            }

            await _context.SaveChangesAsync();
            return MapToDTO(form);
        }

        public async Task<FormDTO?> GetFormByIdAsync(Guid formId)
        {
            var form = await _context.Forms
                .Include(f => f.CreatedByUser)
                .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null) return null;

            // Charger le count séparément si nécessaire
            var submissionCount = await _context.FormSubmissions
                .Where(s => s.FormId == formId)
                .CountAsync();

            var dto = MapToDTO(form);
            dto.TotalResponses = submissionCount;
            return dto;
        }

        public async Task<List<FormDTO>> GetAllFormsAsync()
        {
            var forms = await _context.Forms
                .Include(f => f.CreatedByUser)
                .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
                .Where(f => f.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var formDtos = forms.Select(MapToDTO).ToList();

            // Charger les counts séparément en une seule requête
            var submissionCounts = await _context.FormSubmissions
                .GroupBy(s => s.FormId)
                .Select(g => new { FormId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var dto in formDtos)
            {
                var count = submissionCounts.FirstOrDefault(s => s.FormId == dto.Id);
                dto.TotalResponses = count?.Count ?? 0;
            }

            return formDtos;
        }

        public async Task<List<FormDTO>> GetFormsByUserAsync(Guid userId)
        {
            var forms = await _context.Forms
                .Include(f => f.CreatedByUser)
                .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
                .Where(f => f.CreatedBy == userId && f.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var formDtos = forms.Select(MapToDTO).ToList();

            // Charger les counts séparément
            var submissionCounts = await _context.FormSubmissions
                .Where(s => formDtos.Select(f => f.Id).Contains(s.FormId))
                .GroupBy(s => s.FormId)
                .Select(g => new { FormId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var dto in formDtos)
            {
                var count = submissionCounts.FirstOrDefault(s => s.FormId == dto.Id);
                dto.TotalResponses = count?.Count ?? 0;
            }

            return formDtos;
        }

        public async Task<bool> UpdateFormAsync(Guid formId, UpdateFormRequest request, Guid userId)
        {
            var form = await _context.Forms.FirstOrDefaultAsync(f => f.Id == formId);
            if (form == null || form.CreatedBy != userId)
                return false;

            if (!form.IsPublished) // Peut modifier que si non publié
            {
                if (!string.IsNullOrEmpty(request.Title)) form.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Description)) form.Description = request.Description;
                if (request.StartDate.HasValue) form.StartDate = request.StartDate;
                if (request.EndDate.HasValue) form.EndDate = request.EndDate;
                if (!string.IsNullOrEmpty(request.TargetAudience)) form.TargetAudience = request.TargetAudience;
                if (request.AllowMultipleResponses.HasValue) form.AllowMultipleResponses = request.AllowMultipleResponses.Value;
            }

            form.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFormAsync(Guid formId, Guid userId)
        {
            var form = await _context.Forms.FirstOrDefaultAsync(f => f.Id == formId);
            if (form == null || form.CreatedBy != userId)
                return false;

            form.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishFormAsync(Guid formId, Guid userId)
        {
            var form = await _context.Forms
                .Include(f => f.Questions)
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null || form.CreatedBy != userId || form.Questions.Count == 0)
                return false;

            form.IsPublished = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ===== QUESTION MANAGEMENT =====
        public async Task<FormQuestionDTO?> AddQuestionAsync(Guid formId, CreateFormQuestionRequest request, Guid userId)
        {
            var form = await _context.Forms
                .Include(f => f.Questions)
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null || form.CreatedBy != userId || form.IsPublished)
                return null;

            var question = new FormQuestion
            {
                Id = Guid.NewGuid(),
                FormId = formId,
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                IsRequired = request.IsRequired,
                Order = request.Order,
                CreatedAt = DateTime.UtcNow
            };

            // Ajouter les options si nécessaire
            if (request.Type == QuestionType.MultipleChoice || request.Type == QuestionType.Checkboxes || request.Type == QuestionType.Dropdown)
            {
                int optionOrder = 0;
                foreach (var option in request.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        Id = Guid.NewGuid(),
                        OptionText = option,
                        Order = optionOrder++
                    });
                }
            }

            _context.FormQuestions.Add(question);
            await _context.SaveChangesAsync();

            return MapQuestionToDTO(question);
        }

        public async Task<bool> UpdateQuestionAsync(Guid formId, Guid questionId, UpdateFormQuestionRequest request, Guid userId)
        {
            var form = await _context.Forms.FirstOrDefaultAsync(f => f.Id == formId);
            if (form == null || form.CreatedBy != userId || form.IsPublished)
                return false;

            var question = await _context.FormQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionId && q.FormId == formId);

            if (question == null)
                return false;

            if (!string.IsNullOrEmpty(request.Title)) question.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Description)) question.Description = request.Description;
            if (request.IsRequired.HasValue) question.IsRequired = request.IsRequired.Value;
            if (request.Order.HasValue) question.Order = request.Order.Value;

            // Mettre à jour les options
            if (request.Options != null && request.Options.Any())
            {
                _context.QuestionOptions.RemoveRange(question.Options);
                int optionOrder = 0;
                foreach (var option in request.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        Id = Guid.NewGuid(),
                        OptionText = option,
                        Order = optionOrder++
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestionAsync(Guid formId, Guid questionId, Guid userId)
        {
            var form = await _context.Forms.FirstOrDefaultAsync(f => f.Id == formId);
            if (form == null || form.CreatedBy != userId || form.IsPublished)
                return false;

            var question = await _context.FormQuestions.FirstOrDefaultAsync(q => q.Id == questionId && q.FormId == formId);
            if (question == null)
                return false;

            _context.FormQuestions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        // ===== SUBMISSIONS =====
        public async Task<bool> SubmitFormAsync(Guid formId, Guid userId, SubmitFormRequest request)
        {
            var form = await _context.Forms
                .Include(f => f.Questions)
                .FirstOrDefaultAsync(f => f.Id == formId && f.IsPublished && f.IsActive);

            if (form == null)
                return false;

            // Vérifier si l'utilisateur a déjà répondu
            if (!form.AllowMultipleResponses)
            {
                var existingSubmission = await _context.FormSubmissions
                    .FirstOrDefaultAsync(s => s.FormId == formId && s.UserId == userId);

                if (existingSubmission != null)
                    return false;
            }

            var submission = new FormSubmission
            {
                Id = Guid.NewGuid(),
                FormId = formId,
                UserId = userId,
                SubmittedAt = DateTime.UtcNow
            };

            // Debug: log du nombre de réponses reçues
            Console.WriteLine($"[FormSubmit] Nombre de réponses reçues: {request.Answers.Count}");
            
            foreach (var answer in request.Answers)
            {
                var question = form.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question == null)
                    continue;

                var answerRecord = new Answer
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    QuestionId = answer.QuestionId,
                    OptionId = answer.OptionId,
                    ResponseValue = answer.ResponseValue,
                    CreatedAt = DateTime.UtcNow
                };

                // Debug: log de chaque réponse
                Console.WriteLine($"[FormSubmit] Réponse - Question: {question.Title}, OptionId: {answer.OptionId}, ResponseValue: {answer.ResponseValue}");

                // Ajouter les membres de l'équipe si c'est une question d'équipe
                if (question.Type == QuestionType.Team && answer.TeamMemberIds != null)
                {
                    foreach (var memberId in answer.TeamMemberIds.Take(5)) // Max 5 personnes
                    {
                        answerRecord.TeamMembers.Add(new TeamMember
                        {
                            Id = Guid.NewGuid(),
                            UserId = memberId,
                            AddedAt = DateTime.UtcNow
                        });
                    }
                }

                submission.Answers.Add(answerRecord);
            }

            _context.FormSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"[FormSubmit] Soumission sauvegardée avec {submission.Answers.Count} réponses");
            
            return true;
        }

        public async Task<List<FormSubmissionDTO>> GetSubmissionsAsync(Guid formId)
        {
            var submissions = await _context.FormSubmissions
                .Include(s => s.User)
                .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                .Include(s => s.Answers)
                .ThenInclude(a => a.Option)
                .Include(s => s.Answers)
                .ThenInclude(a => a.TeamMembers)
                .ThenInclude(tm => tm.User)
                .Where(s => s.FormId == formId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return submissions.Select(MapSubmissionToDTO).ToList();
        }

        public async Task<FormSubmissionDTO?> GetSubmissionByIdAsync(Guid submissionId)
        {
            var submission = await _context.FormSubmissions
                .Include(s => s.User)
                .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                .Include(s => s.Answers)
                .ThenInclude(a => a.Option)
                .Include(s => s.Answers)
                .ThenInclude(a => a.TeamMembers)
                .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            return submission != null ? MapSubmissionToDTO(submission) : null;
        }

        public async Task<List<FormSubmissionDTO>> GetUserSubmissionsAsync(Guid formId, Guid userId)
        {
            var submissions = await _context.FormSubmissions
                .Include(s => s.User)
                .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                .Include(s => s.Answers)
                .ThenInclude(a => a.Option)
                .Include(s => s.Answers)
                .ThenInclude(a => a.TeamMembers)
                .ThenInclude(tm => tm.User)
                .Where(s => s.FormId == formId && s.UserId == userId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return submissions.Select(MapSubmissionToDTO).ToList();
        }

        public async Task<int> GetFormSubmissionCountAsync(Guid formId)
        {
            return await _context.FormSubmissions
                .Where(s => s.FormId == formId)
                .CountAsync();
        }

        // ===== EXPORT =====
        public async Task<byte[]> ExportToExcelAsync(Guid formId)
        {
            var form = await _context.Forms
                .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null)
                return Array.Empty<byte>();

            // Charger les soumissions avec TOUTES les données nécessaires
            var submissions = await _context.FormSubmissions
                .Where(s => s.FormId == formId)
                .Include(s => s.User)
                .Include(s => s.Answers)
                    .ThenInclude(a => a.Question)
                .Include(s => s.Answers)
                    .ThenInclude(a => a.Option)
                .Include(s => s.Answers)
                    .ThenInclude(a => a.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .OrderByDescending(s => s.SubmittedAt)
                .AsNoTracking()
                .ToListAsync();

            // Créer un StringBuilder pour CSV (Excel peut l'importer)
            var csv = new StringBuilder();

            // En-tête
            csv.Append("Nom d'utilisateur,Email,Date de soumission");
            foreach (var question in form.Questions.OrderBy(q => q.Order))
            {
                csv.Append($",{EscapeCsvValue(question.Title)}");
            }
            csv.AppendLine();

            // Données
            foreach (var submission in submissions)
            {
                csv.Append($"{EscapeCsvValue(submission.User?.FirstName + " " + submission.User?.LastName ?? "Utilisateur supprimé")},{EscapeCsvValue(submission.User?.Email ?? "N/A")},{EscapeCsvValue(submission.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss"))}");

                foreach (var question in form.Questions.OrderBy(q => q.Order))
                {
                    // Récupérer TOUTES les réponses pour cette question
                    var answers = submission.Answers
                        .Where(a => a.QuestionId == question.Id)
                        .ToList();
                    
                    var value = "";

                    if (answers.Count > 0)
                    {
                        // Concaténer toutes les réponses avec un saut de ligne
                        var answerTexts = new List<string>();
                        
                        foreach (var answer in answers)
                        {
                            // Si c'est une réponse d'équipe
                            if (question.Type == QuestionType.Team)
                            {
                                // Charger les TeamMembers s'ils existent
                                if (answer.TeamMembers != null && answer.TeamMembers.Count > 0)
                                {
                                    var teamMemberNames = answer.TeamMembers
                                        .Where(tm => tm.User != null)
                                        .Select(tm => $"{tm.User.FirstName} {tm.User.LastName} ({tm.User.MatriculeNumber})")
                                        .ToList();
                                    
                                    if (teamMemberNames.Count > 0)
                                    {
                                        answerTexts.Add(string.Join("; ", teamMemberNames));
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(answer.Option?.OptionText))
                            {
                                answerTexts.Add(answer.Option.OptionText);
                            }
                            else if (!string.IsNullOrEmpty(answer.ResponseValue))
                            {
                                answerTexts.Add(answer.ResponseValue);
                            }
                        }

                        // Utiliser un saut de ligne pour empiler les réponses verticalement dans la cellule
                        value = string.Join("\n", answerTexts);
                    }

                    csv.Append($",{EscapeCsvValue(value)}");
                }
                csv.AppendLine();
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<string> ExportToCsvAsync(Guid formId)
        {
            var bytes = await ExportToExcelAsync(formId);
            return Encoding.UTF8.GetString(bytes);
        }

        // Méthode pour échapper correctement les valeurs CSV
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Si la valeur contient une virgule, un guillemet ou une nouvelle ligne, l'entourer de guillemets et doubler les guillemets
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        // ===== HELPERS =====
        private FormDTO MapToDTO(Form form)
        {
            return new FormDTO
            {
                Id = form.Id,
                Title = form.Title,
                Description = form.Description,
                CreatedAt = form.CreatedAt,
                UpdatedAt = form.UpdatedAt,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                IsPublished = form.IsPublished,
                IsActive = form.IsActive,
                TargetAudience = form.TargetAudience,
                AllowMultipleResponses = form.AllowMultipleResponses,
                TotalResponses = form.Submissions.Count,
                CreatedBy = form.CreatedByUser != null ? new CreatedByUserDto
                {
                    Id = form.CreatedByUser.Id,
                    FirstName = form.CreatedByUser.FirstName,
                    LastName = form.CreatedByUser.LastName,
                    ProfileImageUrl = form.CreatedByUser.ProfileImageUrl
                } : null,
                Questions = form.Questions.OrderBy(q => q.Order).Select(MapQuestionToDTO).ToList()
            };
        }

        private FormQuestionDTO MapQuestionToDTO(FormQuestion question)
        {
            return new FormQuestionDTO
            {
                Id = question.Id,
                Title = question.Title,
                Description = question.Description,
                Type = question.Type,
                IsRequired = question.IsRequired,
                Order = question.Order,
                Options = question.Options.OrderBy(o => o.Order).Select(o => new QuestionOptionDTO
                {
                    Id = o.Id,
                    OptionText = o.OptionText,
                    Order = o.Order,
                    ResponseCount = o.Answers.Count
                }).ToList()
            };
        }

        private FormSubmissionDTO MapSubmissionToDTO(FormSubmission submission)
        {
            return new FormSubmissionDTO
            {
                Id = submission.Id,
                UserId = submission.UserId,
                UserName = string.IsNullOrEmpty(submission.User?.FirstName) 
                    ? "Utilisateur supprimé" 
                    : $"{submission.User.FirstName} {submission.User.LastName}",
                UserEmail = submission.User?.Email ?? "N/A",
                SubmittedAt = submission.SubmittedAt,
                Answers = submission.Answers.Select(a => new AnswerDTO
                {
                    QuestionId = a.QuestionId,
                    QuestionTitle = a.Question.Title,
                    QuestionType = a.Question.Type,
                    OptionText = a.Option?.OptionText,
                    ResponseValue = a.ResponseValue,
                    TeamMembers = a.TeamMembers?.Select(tm => new TeamMemberDTO
                    {
                        UserId = tm.User.Id,
                        FirstName = tm.User.FirstName,
                        LastName = tm.User.LastName,
                        Email = tm.User.Email,
                        MatriculeNumber = tm.User.MatriculeNumber,
                        ProfileImageUrl = tm.User.ProfileImageUrl
                    }).ToList() ?? new List<TeamMemberDTO>()
                }).ToList()
            };
        }

        // ===== USER SEARCH =====
        public async Task<List<UserSearchDTO>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<UserSearchDTO>();

            var lowerQuery = query.ToLower();
            var users = await _context.Users
                .Where(u => u.IsActive &&
                    (u.FirstName.ToLower().Contains(lowerQuery) ||
                     u.LastName.ToLower().Contains(lowerQuery) ||
                     u.Email.ToLower().Contains(lowerQuery) ||
                     u.MatriculeNumber.ToLower().Contains(lowerQuery)))
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Take(20) // Limiter à 20 résultats
                .Select(u => new UserSearchDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    MatriculeNumber = u.MatriculeNumber,
                    ProfileImageUrl = u.ProfileImageUrl
                })
                .ToListAsync();

            return users;
        }
    }
}
