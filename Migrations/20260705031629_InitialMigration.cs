using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObligatorioApiario.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apiarios",
                columns: table => new
                {
                    ID_Apiario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Ubicacion = table.Column<string>(type: "text", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    Zona = table.Column<string>(type: "text", nullable: false),
                    Latitud = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    Longitud = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    Cant_Colmenas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apiarios", x => x.ID_Apiario);
                });

            migrationBuilder.CreateTable(
                name: "Apicultores",
                columns: table => new
                {
                    CIApicultor = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Cargo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apicultores", x => x.CIApicultor);
                    table.CheckConstraint("CHK_Apicultor_Cargo", "\"Cargo\" IN ('Administrador de Apiario', 'Apicultor de Campo')");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colmenas",
                columns: table => new
                {
                    ID_Colmena = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identificador = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Cant_Bastidores = table.Column<int>(type: "integer", nullable: false),
                    Cant_Abejas_Estimada = table.Column<int>(type: "integer", nullable: false),
                    Fortaleza_Abejas = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colmenas", x => x.ID_Colmena);
                    table.CheckConstraint("CHK_Fortaleza_Abejas", "\"Fortaleza_Abejas\" IN ('Fuerte', 'Media', 'Débil')");
                });

            migrationBuilder.CreateTable(
                name: "Cosechas",
                columns: table => new
                {
                    ID_Cosecha = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Observaciones = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Identificador = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cosechas", x => x.ID_Cosecha);
                });

            migrationBuilder.CreateTable(
                name: "Herramientas",
                columns: table => new
                {
                    ID_Herramienta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Herramientas", x => x.ID_Herramienta);
                });

            migrationBuilder.CreateTable(
                name: "Reinas",
                columns: table => new
                {
                    ID_Reina = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identificador = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Nivel_Prod = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reinas", x => x.ID_Reina);
                });

            migrationBuilder.CreateTable(
                name: "Tratamientos",
                columns: table => new
                {
                    ID_Tratamiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Fecha_Inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cant_Dosis = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tratamientos", x => x.ID_Tratamiento);
                });

            migrationBuilder.CreateTable(
                name: "ApicultorTelefonos",
                columns: table => new
                {
                    CIApicultor = table.Column<int>(type: "integer", nullable: false),
                    Telefono = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApicultorTelefonos", x => new { x.CIApicultor, x.Telefono });
                    table.ForeignKey(
                        name: "FK_ApicultorTelefonos_Apicultores_CIApicultor",
                        column: x => x.CIApicultor,
                        principalTable: "Apicultores",
                        principalColumn: "CIApicultor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Barriles",
                columns: table => new
                {
                    ID_Barril = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cantidad_Miel = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CIApicultor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barriles", x => x.ID_Barril);
                    table.ForeignKey(
                        name: "FK_Barriles_Apicultores_CIApicultor",
                        column: x => x.CIApicultor,
                        principalTable: "Apicultores",
                        principalColumn: "CIApicultor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    ID_Visita = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Temporada = table.Column<string>(type: "text", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: false),
                    CIApicultor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.ID_Visita);
                    table.ForeignKey(
                        name: "FK_Visitas_Apicultores_CIApicultor",
                        column: x => x.CIApicultor,
                        principalTable: "Apicultores",
                        principalColumn: "CIApicultor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tiene",
                columns: table => new
                {
                    ID_Apiario = table.Column<int>(type: "integer", nullable: false),
                    ID_Colmena = table.Column<int>(type: "integer", nullable: false),
                    Fecha_Instalacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sector = table.Column<string>(type: "text", nullable: false),
                    Fila = table.Column<string>(type: "text", nullable: false),
                    Columna = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tiene", x => new { x.ID_Apiario, x.ID_Colmena });
                    table.ForeignKey(
                        name: "FK_Tiene_Apiarios_ID_Apiario",
                        column: x => x.ID_Apiario,
                        principalTable: "Apiarios",
                        principalColumn: "ID_Apiario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tiene_Colmenas_ID_Colmena",
                        column: x => x.ID_Colmena,
                        principalTable: "Colmenas",
                        principalColumn: "ID_Colmena",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Realiza",
                columns: table => new
                {
                    ID_Cosecha = table.Column<int>(type: "integer", nullable: false),
                    ID_Apiario = table.Column<int>(type: "integer", nullable: false),
                    Fecha_Cosecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo_Miel = table.Column<string>(type: "text", nullable: false),
                    Cant_Barriles = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Realiza", x => new { x.ID_Cosecha, x.ID_Apiario });
                    table.ForeignKey(
                        name: "FK_Realiza_Apiarios_ID_Apiario",
                        column: x => x.ID_Apiario,
                        principalTable: "Apiarios",
                        principalColumn: "ID_Apiario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Realiza_Cosechas_ID_Cosecha",
                        column: x => x.ID_Cosecha,
                        principalTable: "Cosechas",
                        principalColumn: "ID_Cosecha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asigna",
                columns: table => new
                {
                    ID_Colmena = table.Column<int>(type: "integer", nullable: false),
                    ID_Reina = table.Column<int>(type: "integer", nullable: false),
                    Fecha_Asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asigna", x => new { x.ID_Colmena, x.ID_Reina });
                    table.ForeignKey(
                        name: "FK_Asigna_Colmenas_ID_Colmena",
                        column: x => x.ID_Colmena,
                        principalTable: "Colmenas",
                        principalColumn: "ID_Colmena",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asigna_Reinas_ID_Reina",
                        column: x => x.ID_Reina,
                        principalTable: "Reinas",
                        principalColumn: "ID_Reina",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Aplica",
                columns: table => new
                {
                    ID_Colmena = table.Column<int>(type: "integer", nullable: false),
                    ID_Tratamiento = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aplica", x => new { x.ID_Colmena, x.ID_Tratamiento });
                    table.ForeignKey(
                        name: "FK_Aplica_Colmenas_ID_Colmena",
                        column: x => x.ID_Colmena,
                        principalTable: "Colmenas",
                        principalColumn: "ID_Colmena",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Aplica_Tratamientos_ID_Tratamiento",
                        column: x => x.ID_Tratamiento,
                        principalTable: "Tratamientos",
                        principalColumn: "ID_Tratamiento",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Genera",
                columns: table => new
                {
                    ID_Barril = table.Column<int>(type: "integer", nullable: false),
                    ID_Cosecha = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genera", x => new { x.ID_Barril, x.ID_Cosecha });
                    table.ForeignKey(
                        name: "FK_Genera_Barriles_ID_Barril",
                        column: x => x.ID_Barril,
                        principalTable: "Barriles",
                        principalColumn: "ID_Barril",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Genera_Cosechas_ID_Cosecha",
                        column: x => x.ID_Cosecha,
                        principalTable: "Cosechas",
                        principalColumn: "ID_Cosecha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Planifica",
                columns: table => new
                {
                    ID_Visita = table.Column<int>(type: "integer", nullable: false),
                    ID_Herramienta = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planifica", x => new { x.ID_Visita, x.ID_Herramienta });
                    table.ForeignKey(
                        name: "FK_Planifica_Herramientas_ID_Herramienta",
                        column: x => x.ID_Herramienta,
                        principalTable: "Herramientas",
                        principalColumn: "ID_Herramienta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Planifica_Visitas_ID_Visita",
                        column: x => x.ID_Visita,
                        principalTable: "Visitas",
                        principalColumn: "ID_Visita",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Revisa",
                columns: table => new
                {
                    ID_Visita = table.Column<int>(type: "integer", nullable: false),
                    ID_Apiario = table.Column<int>(type: "integer", nullable: false),
                    H_Salida = table.Column<TimeSpan>(type: "interval", nullable: false),
                    H_Llegada = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Clima = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisa", x => new { x.ID_Visita, x.ID_Apiario });
                    table.ForeignKey(
                        name: "FK_Revisa_Apiarios_ID_Apiario",
                        column: x => x.ID_Apiario,
                        principalTable: "Apiarios",
                        principalColumn: "ID_Apiario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Revisa_Visitas_ID_Visita",
                        column: x => x.ID_Visita,
                        principalTable: "Visitas",
                        principalColumn: "ID_Visita",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aplica_ID_Tratamiento",
                table: "Aplica",
                column: "ID_Tratamiento");

            migrationBuilder.CreateIndex(
                name: "IX_Asigna_ID_Reina",
                table: "Asigna",
                column: "ID_Reina");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Barriles_CIApicultor",
                table: "Barriles",
                column: "CIApicultor");

            migrationBuilder.CreateIndex(
                name: "IX_Genera_ID_Cosecha",
                table: "Genera",
                column: "ID_Cosecha");

            migrationBuilder.CreateIndex(
                name: "IX_Planifica_ID_Herramienta",
                table: "Planifica",
                column: "ID_Herramienta");

            migrationBuilder.CreateIndex(
                name: "IX_Realiza_ID_Apiario",
                table: "Realiza",
                column: "ID_Apiario");

            migrationBuilder.CreateIndex(
                name: "IX_Revisa_ID_Apiario",
                table: "Revisa",
                column: "ID_Apiario");

            migrationBuilder.CreateIndex(
                name: "IX_Tiene_ID_Colmena",
                table: "Tiene",
                column: "ID_Colmena");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_CIApicultor",
                table: "Visitas",
                column: "CIApicultor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApicultorTelefonos");

            migrationBuilder.DropTable(
                name: "Aplica");

            migrationBuilder.DropTable(
                name: "Asigna");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Genera");

            migrationBuilder.DropTable(
                name: "Planifica");

            migrationBuilder.DropTable(
                name: "Realiza");

            migrationBuilder.DropTable(
                name: "Revisa");

            migrationBuilder.DropTable(
                name: "Tiene");

            migrationBuilder.DropTable(
                name: "Tratamientos");

            migrationBuilder.DropTable(
                name: "Reinas");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Barriles");

            migrationBuilder.DropTable(
                name: "Herramientas");

            migrationBuilder.DropTable(
                name: "Cosechas");

            migrationBuilder.DropTable(
                name: "Visitas");

            migrationBuilder.DropTable(
                name: "Apiarios");

            migrationBuilder.DropTable(
                name: "Colmenas");

            migrationBuilder.DropTable(
                name: "Apicultores");
        }
    }
}
