using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Back.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "defect_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status_desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_defect_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "info",
                columns: table => new
                {
                    info_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    defect_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    defect_description = table.Column<string>(type: "text", nullable: false),
                    prioriyty = table.Column<short>(type: "smallint", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_change_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_info", x => x.info_id);
                });

            migrationBuilder.CreateTable(
                name: "project_status",
                columns: table => new
                {
                    project_status_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_status_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    project_status_description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_status", x => x.project_status_id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_status_id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project", x => x.project_id);
                    table.ForeignKey(
                        name: "FK_project_project_status_project_status_id",
                        column: x => x.project_status_id,
                        principalTable: "project_status",
                        principalColumn: "project_status_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    Login = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Fio = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "defect",
                columns: table => new
                {
                    defect_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    info_id = table.Column<int>(type: "integer", nullable: false),
                    responsible_id = table.Column<int>(type: "integer", nullable: true),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_defect", x => x.defect_id);
                    table.ForeignKey(
                        name: "FK_defect_defect_status_status_id",
                        column: x => x.status_id,
                        principalTable: "defect_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_defect_info_info_id",
                        column: x => x.info_id,
                        principalTable: "info",
                        principalColumn: "info_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_defect_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_defect_user_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_defect_user_responsible_id",
                        column: x => x.responsible_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "comment",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    defect_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    comment_text = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_comment_defect_defect_id",
                        column: x => x.defect_id,
                        principalTable: "defect",
                        principalColumn: "defect_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comment_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "defect_attachment",
                columns: table => new
                {
                    attachment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    defect_id = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    upload_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    uploaded_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_defect_attachment", x => x.attachment_id);
                    table.ForeignKey(
                        name: "FK_defect_attachment_defect_defect_id",
                        column: x => x.defect_id,
                        principalTable: "defect",
                        principalColumn: "defect_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_defect_attachment_user_uploaded_by_id",
                        column: x => x.uploaded_by_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "defect_history",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    defect_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    old_value = table.Column<string>(type: "text", nullable: false),
                    new_value = table.Column<string>(type: "text", nullable: false),
                    change_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_defect_history", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_defect_history_defect_defect_id",
                        column: x => x.defect_id,
                        principalTable: "defect",
                        principalColumn: "defect_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_defect_history_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "defect_status",
                columns: new[] { "id", "status_desc", "status_name" },
                values: new object[,]
                {
                    { 1, "New defect", "New" },
                    { 2, "Defect in progress", "In Progress" },
                    { 3, "Defect under review", "Under Review" },
                    { 4, "Defect closed", "Closed" },
                    { 5, "Defect cancelled", "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "project_status",
                columns: new[] { "project_status_id", "project_status_description", "project_status_name" },
                values: new object[,]
                {
                    { 1, "Active project", "Active" },
                    { 2, "Completed project", "Completed" },
                    { 3, "Project on hold", "On Hold" }
                });

            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "role_id", "role_name" },
                values: new object[,]
                {
                    { 1, "Observer" },
                    { 2, "Engineer" },
                    { 3, "Manager" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_comment_defect_id",
                table: "comment",
                column: "defect_id");

            migrationBuilder.CreateIndex(
                name: "IX_comment_user_id",
                table: "comment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_defect_created_by_id",
                table: "defect",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_defect_info_id",
                table: "defect",
                column: "info_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_defect_project_id",
                table: "defect",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_defect_responsible_id",
                table: "defect",
                column: "responsible_id");

            migrationBuilder.CreateIndex(
                name: "IX_defect_status_id",
                table: "defect",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_defect_attachment_defect_id",
                table: "defect_attachment",
                column: "defect_id");

            migrationBuilder.CreateIndex(
                name: "IX_defect_attachment_uploaded_by_id",
                table: "defect_attachment",
                column: "uploaded_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_defect_history_defect_id",
                table: "defect_history",
                column: "defect_id");

            migrationBuilder.CreateIndex(
                name: "IX_defect_history_user_id",
                table: "defect_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_project_status_id",
                table: "project",
                column: "project_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_id",
                table: "user",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comment");

            migrationBuilder.DropTable(
                name: "defect_attachment");

            migrationBuilder.DropTable(
                name: "defect_history");

            migrationBuilder.DropTable(
                name: "defect");

            migrationBuilder.DropTable(
                name: "defect_status");

            migrationBuilder.DropTable(
                name: "info");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "project_status");

            migrationBuilder.DropTable(
                name: "role");
        }
    }
}
