using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Ajouter indexes pour optimiser les performances
            // Index pour Posts
            migrationBuilder.CreateIndex(
                name: "idx_posts_created_by",
                table: "Posts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "idx_posts_event_id",
                table: "Posts",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "idx_posts_is_public",
                table: "Posts",
                column: "IsPublic");

            // Index pour Comments
            migrationBuilder.CreateIndex(
                name: "idx_comments_post_id",
                table: "Comments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "idx_comments_user_id",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_comments_parent_id",
                table: "Comments",
                column: "ParentCommentId");

            // Index pour FormSubmissions
            migrationBuilder.CreateIndex(
                name: "idx_form_submissions_form_id",
                table: "FormSubmissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "idx_form_submissions_user_id",
                table: "FormSubmissions",
                column: "UserId");

            // Index pour FormQuestions
            migrationBuilder.CreateIndex(
                name: "idx_form_questions_form_id",
                table: "FormQuestions",
                column: "FormId");

            // Index pour Forms
            migrationBuilder.CreateIndex(
                name: "idx_forms_created_by",
                table: "Forms",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "idx_forms_is_published",
                table: "Forms",
                column: "IsPublished");

            // Index pour Notifications
            migrationBuilder.CreateIndex(
                name: "idx_notifications_recipient_id",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_is_read",
                table: "Notifications",
                column: "IsRead");

            // Index composé pour les requêtes courantes
            migrationBuilder.CreateIndex(
                name: "idx_posts_event_is_public",
                table: "Posts",
                columns: new[] { "EventId", "IsPublic" });

            migrationBuilder.CreateIndex(
                name: "idx_form_submissions_form_user",
                table: "FormSubmissions",
                columns: new[] { "FormId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Supprimer les indexes
            migrationBuilder.DropIndex(
                name: "idx_posts_created_by",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "idx_posts_event_id",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "idx_posts_is_public",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "idx_comments_post_id",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "idx_comments_user_id",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "idx_comments_parent_id",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "idx_form_submissions_form_id",
                table: "FormSubmissions");

            migrationBuilder.DropIndex(
                name: "idx_form_submissions_user_id",
                table: "FormSubmissions");

            migrationBuilder.DropIndex(
                name: "idx_form_questions_form_id",
                table: "FormQuestions");

            migrationBuilder.DropIndex(
                name: "idx_forms_created_by",
                table: "Forms");

            migrationBuilder.DropIndex(
                name: "idx_forms_is_published",
                table: "Forms");

            migrationBuilder.DropIndex(
                name: "idx_notifications_recipient_id",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "idx_notifications_is_read",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "idx_posts_event_is_public",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "idx_form_submissions_form_user",
                table: "FormSubmissions");
        }
    }
}
