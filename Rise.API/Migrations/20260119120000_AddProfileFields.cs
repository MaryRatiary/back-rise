using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add columns that don't already exist
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='ProfileImageUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""ProfileImageUrl"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='CoverImageUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""CoverImageUrl"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='JobTitle') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""JobTitle"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Company') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Company"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Location') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Location"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Phone') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Phone"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Bio') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Bio"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Specialization') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Specialization"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='InterestCategories') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""InterestCategories"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Associations') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Associations"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='SharedExpertise') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""SharedExpertise"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Languages') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Languages"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='LinkedinUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""LinkedinUrl"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='InstagramUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""InstagramUrl"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='TwitterUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""TwitterUrl"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='GithubUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""GithubUrl"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='QrCodeUrl') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""QrCodeUrl"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='NotificationPreferences') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""NotificationPreferences"" TEXT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='ProfileVisibility') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""ProfileVisibility"" TEXT NOT NULL DEFAULT 'public';
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='EventsJoined') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""EventsJoined"" INTEGER NOT NULL DEFAULT 0;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='EventsAttended') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""EventsAttended"" INTEGER NOT NULL DEFAULT 0;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Badges') THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""Badges"" TEXT;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InterestCategories",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Associations",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SharedExpertise",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LinkedinUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GithubUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "QrCodeUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NotificationPreferences",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileVisibility",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EventsJoined",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EventsAttended",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Badges",
                table: "Users");
        }
    }
}
