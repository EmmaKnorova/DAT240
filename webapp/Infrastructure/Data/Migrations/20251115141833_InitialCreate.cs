using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                concurrency_stamp = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_asp_net_roles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                address = table.Column<string>(type: "text", nullable: false),
                city = table.Column<string>(type: "text", nullable: false),
                postal_code = table.Column<string>(type: "text", nullable: false),
                user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: true),
                security_stamp = table.Column<string>(type: "text", nullable: true),
                concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                access_failed_count = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_asp_net_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "food_items",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                cook_time = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_food_items", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "login_input_model",
            columns: table => new
            {
                email = table.Column<string>(type: "text", nullable: false),
                password = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
            });

        migrationBuilder.CreateTable(
            name: "register_input_model",
            columns: table => new
            {
                user_name = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                email = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                confirm_password = table.Column<string>(type: "text", nullable: false),
                address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                postal_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                role = table.Column<string>(type: "text", nullable: false),
                phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
            });

        migrationBuilder.CreateTable(
            name: "shopping_carts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_shopping_carts", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "token_infos",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                refresh_token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_token_infos", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "token_model",
            columns: table => new
            {
                access_token = table.Column<string>(type: "text", nullable: false),
                refresh_token = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                claim_type = table.Column<string>(type: "text", nullable: true),
                claim_value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                table.ForeignKey(
                    name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                    column: x => x.role_id,
                    principalTable: "AspNetRoles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                claim_type = table.Column<string>(type: "text", nullable: true),
                claim_value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                table.ForeignKey(
                    name: "fk_asp_net_user_claims_asp_net_users_user_id",
                    column: x => x.user_id,
                    principalTable: "AspNetUsers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                login_provider = table.Column<string>(type: "text", nullable: false),
                provider_key = table.Column<string>(type: "text", nullable: false),
                provider_display_name = table.Column<string>(type: "text", nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                table.ForeignKey(
                    name: "fk_asp_net_user_logins_asp_net_users_user_id",
                    column: x => x.user_id,
                    principalTable: "AspNetUsers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                table.ForeignKey(
                    name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                    column: x => x.role_id,
                    principalTable: "AspNetRoles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_asp_net_user_roles_asp_net_users_user_id",
                    column: x => x.user_id,
                    principalTable: "AspNetUsers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                login_provider = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                table.ForeignKey(
                    name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                    column: x => x.user_id,
                    principalTable: "AspNetUsers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                location_building = table.Column<string>(type: "text", nullable: false),
                location_room_number = table.Column<string>(type: "text", nullable: false),
                location_notes = table.Column<string>(type: "text", nullable: true),
                notes = table.Column<string>(type: "text", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_orders", x => x.id);
                table.ForeignKey(
                    name: "fk_orders_user_customer_id",
                    column: x => x.customer_id,
                    principalTable: "AspNetUsers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "cart_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sku = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                cart_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_cart_items", x => x.id);
                table.ForeignKey(
                    name: "fk_cart_items_shopping_carts_cart_id",
                    column: x => x.cart_id,
                    principalTable: "shopping_carts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "order_line",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                food_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                food_item_name = table.Column<string>(type: "text", nullable: false),
                amount = table.Column<int>(type: "integer", nullable: false),
                price = table.Column<decimal>(type: "numeric", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_order_line", x => x.id);
                table.ForeignKey(
                    name: "fk_order_line_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "id");
            });

        migrationBuilder.CreateIndex(
            name: "ix_asp_net_role_claims_role_id",
            table: "AspNetRoleClaims",
            column: "role_id");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "normalized_name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_asp_net_user_claims_user_id",
            table: "AspNetUserClaims",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_asp_net_user_logins_user_id",
            table: "AspNetUserLogins",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_asp_net_user_roles_role_id",
            table: "AspNetUserRoles",
            column: "role_id");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "normalized_email");

        migrationBuilder.CreateIndex(
            name: "ix_asp_net_users_email",
            table: "AspNetUsers",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "normalized_user_name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_cart_items_cart_id",
            table: "cart_items",
            column: "cart_id");

        migrationBuilder.CreateIndex(
            name: "ix_order_line_order_id",
            table: "order_line",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_orders_customer_id",
            table: "orders",
            column: "customer_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
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
            name: "cart_items");

        migrationBuilder.DropTable(
            name: "food_items");

        migrationBuilder.DropTable(
            name: "login_input_model");

        migrationBuilder.DropTable(
            name: "order_line");

        migrationBuilder.DropTable(
            name: "register_input_model");

        migrationBuilder.DropTable(
            name: "token_infos");

        migrationBuilder.DropTable(
            name: "token_model");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "shopping_carts");

        migrationBuilder.DropTable(
            name: "orders");

        migrationBuilder.DropTable(
            name: "AspNetUsers");
    }
}
