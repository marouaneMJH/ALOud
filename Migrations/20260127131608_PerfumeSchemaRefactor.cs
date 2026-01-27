using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ALOud.Migrations
{
    /// <inheritdoc />
    public partial class PerfumeSchemaRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Occasions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occasions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perfumes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Intensity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Longevity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sillage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GenderProfile = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PriceRange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perfumes_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfumeAccords",
                columns: table => new
                {
                    PerfumeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Intensity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfumeAccords", x => new { x.PerfumeId, x.AccordId });
                    table.ForeignKey(
                        name: "FK_PerfumeAccords_Accords_AccordId",
                        column: x => x.AccordId,
                        principalTable: "Accords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfumeAccords_Perfumes_PerfumeId",
                        column: x => x.PerfumeId,
                        principalTable: "Perfumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfumeFamilies",
                columns: table => new
                {
                    PerfumeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfumeFamilies", x => new { x.PerfumeId, x.FamilyId });
                    table.ForeignKey(
                        name: "FK_PerfumeFamilies_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfumeFamilies_Perfumes_PerfumeId",
                        column: x => x.PerfumeId,
                        principalTable: "Perfumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfumeNotes",
                columns: table => new
                {
                    PerfumeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfumeNotes", x => new { x.PerfumeId, x.NoteId });
                    table.ForeignKey(
                        name: "FK_PerfumeNotes_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfumeNotes_Perfumes_PerfumeId",
                        column: x => x.PerfumeId,
                        principalTable: "Perfumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfumeOccasions",
                columns: table => new
                {
                    PerfumeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccasionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfumeOccasions", x => new { x.PerfumeId, x.OccasionId });
                    table.ForeignKey(
                        name: "FK_PerfumeOccasions_Occasions_OccasionId",
                        column: x => x.OccasionId,
                        principalTable: "Occasions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfumeOccasions_Perfumes_PerfumeId",
                        column: x => x.PerfumeId,
                        principalTable: "Perfumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfumeSeasons",
                columns: table => new
                {
                    PerfumeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfumeSeasons", x => new { x.PerfumeId, x.SeasonId });
                    table.ForeignKey(
                        name: "FK_PerfumeSeasons_Perfumes_PerfumeId",
                        column: x => x.PerfumeId,
                        principalTable: "Perfumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfumeSeasons_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfumeTags",
                columns: table => new
                {
                    PerfumeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfumeTags", x => new { x.PerfumeId, x.TagId });
                    table.ForeignKey(
                        name: "FK_PerfumeTags_Perfumes_PerfumeId",
                        column: x => x.PerfumeId,
                        principalTable: "Perfumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfumeTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accords_Name",
                table: "Accords",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Families_Name",
                table: "Families",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Name",
                table: "Notes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Occasions_Name",
                table: "Occasions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfumeAccords_AccordId",
                table: "PerfumeAccords",
                column: "AccordId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfumeFamilies_FamilyId",
                table: "PerfumeFamilies",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfumeNotes_NoteId",
                table: "PerfumeNotes",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfumeOccasions_OccasionId",
                table: "PerfumeOccasions",
                column: "OccasionId");

            migrationBuilder.CreateIndex(
                name: "IX_Perfumes_BrandId",
                table: "Perfumes",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfumeSeasons_SeasonId",
                table: "PerfumeSeasons",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfumeTags_TagId",
                table: "PerfumeTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_Name",
                table: "Seasons",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfumeAccords");

            migrationBuilder.DropTable(
                name: "PerfumeFamilies");

            migrationBuilder.DropTable(
                name: "PerfumeNotes");

            migrationBuilder.DropTable(
                name: "PerfumeOccasions");

            migrationBuilder.DropTable(
                name: "PerfumeSeasons");

            migrationBuilder.DropTable(
                name: "PerfumeTags");

            migrationBuilder.DropTable(
                name: "Accords");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Occasions");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Perfumes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
