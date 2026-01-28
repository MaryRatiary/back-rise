using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEventFormLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Skip User columns as they already exist from previous migrations
            // Only add the Event and EventRegistration columns
            
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- Add FormId to Events if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Events' AND column_name='FormId') THEN
                        ALTER TABLE ""Events"" ADD COLUMN ""FormId"" uuid;
                    END IF;
                    
                    -- Add RequireFormSubmission to Events if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Events' AND column_name='RequireFormSubmission') THEN
                        ALTER TABLE ""Events"" ADD COLUMN ""RequireFormSubmission"" boolean NOT NULL DEFAULT false;
                    END IF;
                    
                    -- Add FormSubmissionId to EventRegistrations if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='EventRegistrations' AND column_name='FormSubmissionId') THEN
                        ALTER TABLE ""EventRegistrations"" ADD COLUMN ""FormSubmissionId"" uuid;
                    END IF;
                END $$;
            ");

            // Create index for FormId if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Events_FormId') THEN
                        CREATE INDEX ""IX_Events_FormId"" ON ""Events"" (""FormId"");
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_EventRegistrations_FormSubmissionId') THEN
                        CREATE UNIQUE INDEX ""IX_EventRegistrations_FormSubmissionId"" ON ""EventRegistrations"" (""FormSubmissionId"");
                    END IF;
                END $$;
            ");

            // Add foreign keys if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_name = 'FK_EventRegistrations_FormSubmissions_FormSubmissionId'
                    ) THEN
                        ALTER TABLE ""EventRegistrations""
                        ADD CONSTRAINT ""FK_EventRegistrations_FormSubmissions_FormSubmissionId""
                        FOREIGN KEY (""FormSubmissionId"") REFERENCES ""FormSubmissions""(""Id"");
                    END IF;
                    
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_name = 'FK_Events_Forms_FormId'
                    ) THEN
                        ALTER TABLE ""Events""
                        ADD CONSTRAINT ""FK_Events_Forms_FormId""
                        FOREIGN KEY (""FormId"") REFERENCES ""Forms""(""Id"");
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_FormSubmissions_FormSubmissionId",
                table: "EventRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Forms_FormId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_FormId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventRegistrations_FormSubmissionId",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "Associations",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Badges",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EventsAttended",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EventsJoined",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GithubUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InterestCategories",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LinkedinUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NotificationPreferences",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileVisibility",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "QrCodeUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SharedExpertise",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequireFormSubmission",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "FormSubmissionId",
                table: "EventRegistrations");
        }
    }
}
