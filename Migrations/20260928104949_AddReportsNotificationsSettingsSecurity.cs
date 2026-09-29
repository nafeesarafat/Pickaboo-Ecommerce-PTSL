using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReportsNotificationsSettingsSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationSettings_UserId",
                table: "NotificationSettings");

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiresAt",
                table: "UserInformation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationTokenHash",
                table: "UserInformation",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "UserInformation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTwoFactorEnabled",
                table: "UserInformation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorQrCodeUrlProtected",
                table: "UserInformation",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecretProtected",
                table: "UserInformation",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoreKey",
                table: "StoreSettings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "Session",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Session",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreSettings_StoreKey",
                table: "StoreSettings",
                column: "StoreKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_UserId",
                table: "NotificationSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoreSettings_StoreKey",
                table: "StoreSettings");

            migrationBuilder.DropIndex(
                name: "IX_NotificationSettings_UserId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiresAt",
                table: "UserInformation");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenHash",
                table: "UserInformation");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "UserInformation");

            migrationBuilder.DropColumn(
                name: "IsTwoFactorEnabled",
                table: "UserInformation");

            migrationBuilder.DropColumn(
                name: "TwoFactorQrCodeUrlProtected",
                table: "UserInformation");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecretProtected",
                table: "UserInformation");

            migrationBuilder.DropColumn(
                name: "StoreKey",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "ExpiresAtUtc",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Session");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_UserId",
                table: "NotificationSettings",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }
    }
}
