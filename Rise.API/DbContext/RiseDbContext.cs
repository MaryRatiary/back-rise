using Microsoft.EntityFrameworkCore;
using Rise.API.Models;

namespace Rise.API.Data
{
    public class RiseDbContext : DbContext
    {
        public RiseDbContext(DbContextOptions<RiseDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollQuestion> PollQuestions { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<PollResponse> PollResponses { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VotePosition> VotePositions { get; set; }
        public DbSet<VoteOption> VoteOptions { get; set; }
        public DbSet<VoteCast> VotesCast { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TaggedUser> TaggedUsers { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<MessageReaction> MessageReactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<FormQuestion> FormQuestions { get; set; }
        public DbSet<FormSubmission> FormSubmissions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.MatriculeNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // Event configuration
            modelBuilder.Entity<Event>()
                .Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // EventRegistration configuration
            modelBuilder.Entity<EventRegistration>()
                .Property(er => er.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<EventRegistration>()
                .HasOne(er => er.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventRegistration>()
                .HasOne(er => er.User)
                .WithMany(u => u.EventRegistrations)
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Poll configuration
            modelBuilder.Entity<Poll>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // PollQuestion configuration
            modelBuilder.Entity<PollQuestion>()
                .Property(pq => pq.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<PollQuestion>()
                .HasOne(pq => pq.Poll)
                .WithMany(p => p.Questions)
                .HasForeignKey(pq => pq.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            // PollOption configuration
            modelBuilder.Entity<PollOption>()
                .Property(po => po.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<PollOption>()
                .HasOne(po => po.Question)
                .WithMany(pq => pq.Options)
                .HasForeignKey(po => po.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // PollResponse configuration
            modelBuilder.Entity<PollResponse>()
                .Property(pr => pr.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<PollResponse>()
                .HasOne(pr => pr.User)
                .WithMany(u => u.PollResponses)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PollResponse>()
                .HasOne(pr => pr.Option)
                .WithMany(po => po.Responses)
                .HasForeignKey(pr => pr.OptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vote configuration
            modelBuilder.Entity<Vote>()
                .Property(v => v.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // VotePosition configuration
            modelBuilder.Entity<VotePosition>()
                .Property(vp => vp.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<VotePosition>()
                .HasOne(vp => vp.Vote)
                .WithMany(v => v.Positions)
                .HasForeignKey(vp => vp.VoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // VoteOption configuration
            modelBuilder.Entity<VoteOption>()
                .Property(vo => vo.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<VoteOption>()
                .HasOne(vo => vo.Position)
                .WithMany(vp => vp.Options)
                .HasForeignKey(vo => vo.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoteOption>()
                .HasOne(vo => vo.Candidate)
                .WithMany(u => u.Candidates)
                .HasForeignKey(vo => vo.CandidateId)
                .OnDelete(DeleteBehavior.SetNull);

            // VoteCast configuration
            modelBuilder.Entity<VoteCast>()
                .Property(vc => vc.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<VoteCast>()
                .HasOne(vc => vc.Vote)
                .WithMany(v => v.VotesCast)
                .HasForeignKey(vc => vc.VoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoteCast>()
                .HasOne(vc => vc.Option)
                .WithMany(vo => vo.VotesCast)
                .HasForeignKey(vc => vc.OptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post configuration
            modelBuilder.Entity<Post>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Event)
                .WithMany(e => e.Posts)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // PostImage configuration
            modelBuilder.Entity<PostImage>()
                .Property(pi => pi.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<PostImage>()
                .HasOne(pi => pi.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment configuration
            modelBuilder.Entity<Comment>()
                .Property(c => c.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaggedUser configuration
            modelBuilder.Entity<TaggedUser>()
                .Property(tu => tu.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<TaggedUser>()
                .HasOne(tu => tu.Comment)
                .WithMany(c => c.TaggedUsers)
                .HasForeignKey(tu => tu.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaggedUser>()
                .HasOne(tu => tu.User)
                .WithMany(u => u.TaggedInComments)
                .HasForeignKey(tu => tu.TaggedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reaction configuration
            modelBuilder.Entity<Reaction>()
                .Property(r => r.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Comment)
                .WithMany(c => c.Reactions)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reactions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification configuration
            modelBuilder.Entity<Notification>()
                .Property(n => n.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Recipient)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.TriggeredByUser)
                .WithMany(u => u.TriggeredNotifications)
                .HasForeignKey(n => n.TriggeredByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Post)
                .WithMany()
                .HasForeignKey(n => n.PostId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Comment)
                .WithMany()
                .HasForeignKey(n => n.CommentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Message configuration
            modelBuilder.Entity<Message>()
                .Property(m => m.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.ReplyTo)
                .WithMany()
                .HasForeignKey(m => m.ReplyToId)
                .OnDelete(DeleteBehavior.SetNull);

            // Conversation configuration
            modelBuilder.Entity<Conversation>()
                .Property(c => c.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany(u => u.Conversations1)
                .HasForeignKey(c => c.UserId1)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany(u => u.Conversations2)
                .HasForeignKey(c => c.UserId2)
                .OnDelete(DeleteBehavior.Cascade);

            // MessageReaction configuration
            modelBuilder.Entity<MessageReaction>()
                .Property(mr => mr.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<MessageReaction>()
                .HasOne(mr => mr.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(mr => mr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MessageReaction>()
                .HasOne(mr => mr.User)
                .WithMany(u => u.MessageReactions)
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== FORM CONFIGURATION =====
            
            // Form configuration
            modelBuilder.Entity<Form>()
                .Property(f => f.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Form>()
                .HasOne(f => f.CreatedByUser)
                .WithMany(u => u.CreatedForms)
                .HasForeignKey(f => f.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);

            // FormQuestion configuration
            modelBuilder.Entity<FormQuestion>()
                .Property(fq => fq.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<FormQuestion>()
                .HasOne(fq => fq.Form)
                .WithMany(f => f.Questions)
                .HasForeignKey(fq => fq.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuestionOption configuration
            modelBuilder.Entity<QuestionOption>()
                .Property(qo => qo.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<QuestionOption>()
                .HasOne(qo => qo.Question)
                .WithMany(fq => fq.Options)
                .HasForeignKey(qo => qo.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // FormSubmission configuration
            modelBuilder.Entity<FormSubmission>()
                .Property(fs => fs.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<FormSubmission>()
                .HasOne(fs => fs.Form)
                .WithMany(f => f.Submissions)
                .HasForeignKey(fs => fs.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FormSubmission>()
                .HasOne(fs => fs.User)
                .WithMany(u => u.FormSubmissions)
                .HasForeignKey(fs => fs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Answer configuration
            modelBuilder.Entity<Answer>()
                .Property(a => a.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Submission)
                .WithMany(fs => fs.Answers)
                .HasForeignKey(a => a.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(fq => fq.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Option)
                .WithMany(qo => qo.Answers)
                .HasForeignKey(a => a.OptionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
