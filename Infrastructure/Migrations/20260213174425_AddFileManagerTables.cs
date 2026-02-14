using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileManagerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys only if they exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_cashtransactions_users_CreatorUserId'
                    ) THEN
                        ALTER TABLE cashtransactions DROP CONSTRAINT ""FK_cashtransactions_users_CreatorUserId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_cashtransactions_users_UpdaterUserId'
                    ) THEN
                        ALTER TABLE cashtransactions DROP CONSTRAINT ""FK_cashtransactions_users_UpdaterUserId"";
                    END IF;
                END $$;
            ");

            // Drop indexes only if they exist
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_cashtransactions_CreatorUserId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_cashtransactions_UpdaterUserId\";");

            // Drop old columns only if they exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'cashtransactions' AND column_name = 'CreatorUserId'
                    ) THEN
                        ALTER TABLE cashtransactions DROP COLUMN ""CreatorUserId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'cashtransactions' AND column_name = 'UpdaterUserId'
                    ) THEN
                        ALTER TABLE cashtransactions DROP COLUMN ""UpdaterUserId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "umrahtrips",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            // Add roomtype only if it doesn't already exist (was added by AddRoomFieldsToUmrahPilgrim)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'umrahpilgrims' AND column_name = 'roomtype'
                    ) THEN
                        ALTER TABLE umrahpilgrims ADD roomtype integer;
                    END IF;
                END $$;
            ");

            // Add sharedroomnumber only if it doesn't already exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'umrahpilgrims' AND column_name = 'sharedroomnumber'
                    ) THEN
                        ALTER TABLE umrahpilgrims ADD sharedroomnumber character varying(20);
                    END IF;
                END $$;
            ");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "umrahpackages",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "trips",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "tripbookings",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "suppliers",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "salesinvoices",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "purchaseinvoices",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "journalentries",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "flightbookings",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "customers",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "cashtransactions",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "cashboxes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "accounts",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            // Create active_sessions only if it doesn't already exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS active_sessions (
                    id integer GENERATED BY DEFAULT AS IDENTITY,
                    session_id character varying(100) NOT NULL,
                    user_id integer NOT NULL,
                    username character varying(100) NOT NULL,
                    machine_name character varying(200) NOT NULL,
                    ip_address character varying(50) NOT NULL,
                    login_time timestamp with time zone NOT NULL,
                    last_activity_time timestamp with time zone NOT NULL,
                    logout_time timestamp with time zone,
                    is_active boolean NOT NULL,
                    CONSTRAINT ""PK_active_sessions"" PRIMARY KEY (id)
                );
            ");

            // Create filefolders only if it doesn't already exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS filefolders (
                    folderid integer GENERATED BY DEFAULT AS IDENTITY,
                    foldername character varying(200) NOT NULL,
                    description character varying(500),
                    color character varying(50),
                    icon character varying(50),
                    parentfolderid integer,
                    displayorder integer NOT NULL,
                    issystem boolean NOT NULL,
                    createdat timestamp with time zone NOT NULL,
                    createdby integer NOT NULL,
                    updatedat timestamp with time zone,
                    updatedby integer,
                    CONSTRAINT ""PK_filefolders"" PRIMARY KEY (folderid),
                    CONSTRAINT ""FK_filefolders_filefolders_parentfolderid"" FOREIGN KEY (parentfolderid)
                        REFERENCES filefolders (folderid) ON DELETE RESTRICT,
                    CONSTRAINT ""FK_filefolders_users_createdby"" FOREIGN KEY (createdby)
                        REFERENCES users (userid) ON DELETE RESTRICT
                );
            ");

            // Create filedocuments only if it doesn't already exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS filedocuments (
                    documentid integer GENERATED BY DEFAULT AS IDENTITY,
                    folderid integer NOT NULL,
                    filename character varying(300) NOT NULL,
                    originalfilename character varying(300) NOT NULL,
                    filepath character varying(500) NOT NULL,
                    fileextension character varying(100),
                    filesize bigint NOT NULL,
                    documenttype integer NOT NULL,
                    mimetype character varying(100),
                    description character varying(1000),
                    tags character varying(500),
                    isfavorite boolean NOT NULL,
                    downloadcount integer NOT NULL,
                    uploadedat timestamp with time zone NOT NULL,
                    uploadedby integer NOT NULL,
                    updatedat timestamp with time zone,
                    updatedby integer,
                    CONSTRAINT ""PK_filedocuments"" PRIMARY KEY (documentid),
                    CONSTRAINT ""FK_filedocuments_filefolders_folderid"" FOREIGN KEY (folderid)
                        REFERENCES filefolders (folderid) ON DELETE CASCADE,
                    CONSTRAINT ""FK_filedocuments_users_uploadedby"" FOREIGN KEY (uploadedby)
                        REFERENCES users (userid) ON DELETE RESTRICT
                );
            ");

            // Create indexes only if they don't already exist
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_cashtransactions_createdby\" ON cashtransactions (createdby);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_cashtransactions_UpdatedBy\" ON cashtransactions (\"UpdatedBy\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_active_sessions_is_active\" ON active_sessions (is_active);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_active_sessions_session_id\" ON active_sessions (session_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_active_sessions_user_id\" ON active_sessions (user_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_filedocuments_documenttype\" ON filedocuments (documenttype);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_filedocuments_folderid\" ON filedocuments (folderid);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_filedocuments_isfavorite\" ON filedocuments (isfavorite);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_filedocuments_uploadedby\" ON filedocuments (uploadedby);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_filefolders_createdby\" ON filefolders (createdby);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_filefolders_parentfolderid\" ON filefolders (parentfolderid);");

            // Add foreign keys only if they don't already exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_cashtransactions_users_UpdatedBy'
                    ) THEN
                        ALTER TABLE cashtransactions
                            ADD CONSTRAINT ""FK_cashtransactions_users_UpdatedBy""
                            FOREIGN KEY (""UpdatedBy"") REFERENCES users (userid) ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_cashtransactions_users_createdby'
                    ) THEN
                        ALTER TABLE cashtransactions
                            ADD CONSTRAINT ""FK_cashtransactions_users_createdby""
                            FOREIGN KEY (createdby) REFERENCES users (userid) ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cashtransactions_users_UpdatedBy",
                table: "cashtransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_cashtransactions_users_createdby",
                table: "cashtransactions");

            migrationBuilder.DropTable(
                name: "active_sessions");

            migrationBuilder.DropTable(
                name: "filedocuments");

            migrationBuilder.DropTable(
                name: "filefolders");

            migrationBuilder.DropIndex(
                name: "IX_cashtransactions_createdby",
                table: "cashtransactions");

            migrationBuilder.DropIndex(
                name: "IX_cashtransactions_UpdatedBy",
                table: "cashtransactions");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "umrahtrips");

            // Only drop if exists (to handle cases where it was added by AddRoomFieldsToUmrahPilgrim)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'umrahpilgrims' AND column_name = 'roomtype'
                    ) THEN
                        ALTER TABLE umrahpilgrims DROP COLUMN roomtype;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'umrahpilgrims' AND column_name = 'sharedroomnumber'
                    ) THEN
                        ALTER TABLE umrahpilgrims DROP COLUMN sharedroomnumber;
                    END IF;
                END $$;
            ");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "umrahpackages");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "tripbookings");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "salesinvoices");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "purchaseinvoices");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "journalentries");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "flightbookings");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "cashtransactions");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "cashboxes");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "accounts");

            migrationBuilder.AddColumn<int>(
                name: "CreatorUserId",
                table: "cashtransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdaterUserId",
                table: "cashtransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cashtransactions_CreatorUserId",
                table: "cashtransactions",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_cashtransactions_UpdaterUserId",
                table: "cashtransactions",
                column: "UpdaterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_cashtransactions_users_CreatorUserId",
                table: "cashtransactions",
                column: "CreatorUserId",
                principalTable: "users",
                principalColumn: "userid");

            migrationBuilder.AddForeignKey(
                name: "FK_cashtransactions_users_UpdaterUserId",
                table: "cashtransactions",
                column: "UpdaterUserId",
                principalTable: "users",
                principalColumn: "userid");
        }
    }
}
