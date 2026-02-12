using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    accountid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accountcode = table.Column<string>(type: "text", nullable: false),
                    accountname = table.Column<string>(type: "text", nullable: false),
                    accountnameen = table.Column<string>(type: "text", nullable: true),
                    parentaccountid = table.Column<int>(type: "integer", nullable: true),
                    accounttype = table.Column<string>(type: "text", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    isparent = table.Column<bool>(type: "boolean", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    openingbalance = table.Column<decimal>(type: "numeric", nullable: false),
                    currentbalance = table.Column<decimal>(type: "numeric", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.accountid);
                    table.ForeignKey(
                        name: "FK_accounts_accounts_parentaccountid",
                        column: x => x.parentaccountid,
                        principalTable: "accounts",
                        principalColumn: "accountid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bankaccounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bankname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    accountnumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    accounttype = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    branch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modifiedby = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bankaccounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cashboxes",
                columns: table => new
                {
                    cashboxid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cashboxcode = table.Column<string>(type: "text", nullable: false),
                    cashboxname = table.Column<string>(type: "text", nullable: false),
                    cashboxtype = table.Column<string>(type: "text", nullable: false),
                    accountnumber = table.Column<string>(type: "text", nullable: true),
                    iban = table.Column<string>(type: "text", nullable: true),
                    accountid = table.Column<int>(type: "integer", nullable: true),
                    bankname = table.Column<string>(type: "text", nullable: true),
                    openingbalance = table.Column<decimal>(type: "numeric", nullable: false),
                    currentbalance = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cashboxes", x => x.cashboxid);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    currencyid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    currencycode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    currencyname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    exchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    isbasecurrency = table.Column<bool>(type: "boolean", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.currencyid);
                });

            migrationBuilder.CreateTable(
                name: "currencyexchangerates",
                columns: table => new
                {
                    exchangerateid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fromcurrency = table.Column<string>(type: "text", nullable: false),
                    tocurrency = table.Column<string>(type: "text", nullable: false),
                    exchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    effectivedate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modifiedby = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencyexchangerates", x => x.exchangerateid);
                });

            migrationBuilder.CreateTable(
                name: "database_backups",
                columns: table => new
                {
                    backup_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    backup_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    backup_file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    backup_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_auto_backup = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    restore_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_database_backups", x => x.backup_id);
                });

            migrationBuilder.CreateTable(
                name: "fiscalyearsettings",
                columns: table => new
                {
                    fiscalyearid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fiscalyearstart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fiscalyearend = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    iscurrentyear = table.Column<bool>(type: "boolean", nullable: false),
                    isclosed = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    modifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modifiedby = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscalyearsettings", x => x.fiscalyearid);
                });

            migrationBuilder.CreateTable(
                name: "flightbookings",
                columns: table => new
                {
                    flightbookingid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bookingnumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    issuancedate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    traveldate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    clientname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    clientroute = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    supplier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ticketstatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    paymentmethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sellingprice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    netprice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ticketcount = table.Column<int>(type: "integer", nullable: false),
                    ticketnumbers = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    mobilenumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flightbookings", x => x.flightbookingid);
                });

            migrationBuilder.CreateTable(
                name: "journalentries",
                columns: table => new
                {
                    journalentryid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrynumber = table.Column<string>(type: "text", nullable: false),
                    entrydate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    entrytype = table.Column<string>(type: "text", nullable: false),
                    referencetype = table.Column<string>(type: "text", nullable: true),
                    referenceid = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    totaldebit = table.Column<decimal>(type: "numeric", nullable: false),
                    totalcredit = table.Column<decimal>(type: "numeric", nullable: false),
                    isposted = table.Column<bool>(type: "boolean", nullable: false),
                    postedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journalentries", x => x.journalentryid);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    permissionid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    permissionname = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.permissionid);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    roleid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rolename = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.roleid);
                });

            migrationBuilder.CreateTable(
                name: "servicetypes",
                columns: table => new
                {
                    servicetypeid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    servicetypename = table.Column<string>(type: "text", nullable: false),
                    servicetypenameen = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicetypes", x => x.servicetypeid);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    customerid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customercode = table.Column<string>(type: "text", nullable: false),
                    customername = table.Column<string>(type: "text", nullable: false),
                    customernameen = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    mobile = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    taxnumber = table.Column<string>(type: "text", nullable: true),
                    creditlimit = table.Column<decimal>(type: "numeric", nullable: false),
                    paymenttermdays = table.Column<int>(type: "integer", nullable: false),
                    accountid = table.Column<int>(type: "integer", nullable: true),
                    openingbalance = table.Column<decimal>(type: "numeric", nullable: false),
                    currentbalance = table.Column<decimal>(type: "numeric", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.customerid);
                    table.ForeignKey(
                        name: "FK_customers_accounts_accountid",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "accountid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    supplierid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    suppliercode = table.Column<string>(type: "text", nullable: false),
                    suppliername = table.Column<string>(type: "text", nullable: false),
                    suppliernameen = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    mobile = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    taxnumber = table.Column<string>(type: "text", nullable: true),
                    creditlimit = table.Column<decimal>(type: "numeric", nullable: false),
                    paymenttermdays = table.Column<int>(type: "integer", nullable: false),
                    accountid = table.Column<int>(type: "integer", nullable: true),
                    openingbalance = table.Column<decimal>(type: "numeric", nullable: false),
                    currentbalance = table.Column<decimal>(type: "numeric", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.supplierid);
                    table.ForeignKey(
                        name: "FK_suppliers_accounts_accountid",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "accountid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "banktransfers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sourcebankaccountid = table.Column<int>(type: "integer", nullable: true),
                    sourcecashboxid = table.Column<int>(type: "integer", nullable: true),
                    destinationbankaccountid = table.Column<int>(type: "integer", nullable: true),
                    destinationcashboxid = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    transfertype = table.Column<string>(type: "text", nullable: false),
                    transferdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    referencenumber = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banktransfers", x => x.id);
                    table.ForeignKey(
                        name: "FK_banktransfers_bankaccounts_destinationbankaccountid",
                        column: x => x.destinationbankaccountid,
                        principalTable: "bankaccounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_banktransfers_bankaccounts_sourcebankaccountid",
                        column: x => x.sourcebankaccountid,
                        principalTable: "bankaccounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_banktransfers_cashboxes_destinationcashboxid",
                        column: x => x.destinationcashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_banktransfers_cashboxes_sourcecashboxid",
                        column: x => x.sourcecashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    paymentid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paymentnumber = table.Column<string>(type: "text", nullable: false),
                    paymentdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paymenttype = table.Column<string>(type: "text", nullable: false),
                    cashboxid = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    currencyid = table.Column<int>(type: "integer", nullable: true),
                    exchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    paymentmethod = table.Column<string>(type: "text", nullable: true),
                    referencetype = table.Column<string>(type: "text", nullable: true),
                    referenceid = table.Column<int>(type: "integer", nullable: true),
                    checknumber = table.Column<string>(type: "text", nullable: true),
                    checkdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bankname = table.Column<string>(type: "text", nullable: true),
                    journalentryid = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.paymentid);
                    table.ForeignKey(
                        name: "FK_payments_cashboxes_cashboxid",
                        column: x => x.cashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_currencies_currencyid",
                        column: x => x.currencyid,
                        principalTable: "currencies",
                        principalColumn: "currencyid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "journalentrylines",
                columns: table => new
                {
                    lineid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    journalentryid = table.Column<int>(type: "integer", nullable: false),
                    accountid = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    debitamount = table.Column<decimal>(type: "numeric", nullable: false),
                    creditamount = table.Column<decimal>(type: "numeric", nullable: false),
                    lineorder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journalentrylines", x => x.lineid);
                    table.ForeignKey(
                        name: "FK_journalentrylines_accounts_accountid",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "accountid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_journalentrylines_journalentries_journalentryid",
                        column: x => x.journalentryid,
                        principalTable: "journalentries",
                        principalColumn: "journalentryid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rolepermissions",
                columns: table => new
                {
                    rolepermissionid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    roleid = table.Column<int>(type: "integer", nullable: false),
                    permissionid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rolepermissions", x => x.rolepermissionid);
                    table.ForeignKey(
                        name: "FK_rolepermissions_permissions_permissionid",
                        column: x => x.permissionid,
                        principalTable: "permissions",
                        principalColumn: "permissionid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rolepermissions_roles_roleid",
                        column: x => x.roleid,
                        principalTable: "roles",
                        principalColumn: "roleid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    passwordhash = table.Column<string>(type: "text", nullable: false),
                    fullname = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    roleid = table.Column<int>(type: "integer", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userid);
                    table.ForeignKey(
                        name: "FK_users_roles_roleid",
                        column: x => x.roleid,
                        principalTable: "roles",
                        principalColumn: "roleid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "companysettings",
                columns: table => new
                {
                    settingid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    companyname = table.Column<string>(type: "text", nullable: false),
                    companynameen = table.Column<string>(type: "text", nullable: false),
                    logopath = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    mobile = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "text", nullable: true),
                    taxregistrationnumber = table.Column<string>(type: "text", nullable: true),
                    commercialregistrationnumber = table.Column<string>(type: "text", nullable: true),
                    bankname = table.Column<string>(type: "text", nullable: true),
                    bankaccountnumber = table.Column<string>(type: "text", nullable: true),
                    bankiban = table.Column<string>(type: "text", nullable: true),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lastmodifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lastmodifiedbyuserid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companysettings", x => x.settingid);
                    table.ForeignKey(
                        name: "FK_companysettings_users_lastmodifiedbyuserid",
                        column: x => x.lastmodifiedbyuserid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoicesettings",
                columns: table => new
                {
                    settingid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enabletax = table.Column<bool>(type: "boolean", nullable: false),
                    defaulttaxrate = table.Column<decimal>(type: "numeric", nullable: false),
                    taxlabel = table.Column<string>(type: "text", nullable: true),
                    autonumbering = table.Column<bool>(type: "boolean", nullable: false),
                    salesinvoiceprefix = table.Column<string>(type: "text", nullable: true),
                    purchaseinvoiceprefix = table.Column<string>(type: "text", nullable: true),
                    nextsalesinvoicenumber = table.Column<int>(type: "integer", nullable: false),
                    nextpurchaseinvoicenumber = table.Column<int>(type: "integer", nullable: false),
                    invoicenumberlength = table.Column<int>(type: "integer", nullable: false),
                    invoicefootertext = table.Column<string>(type: "text", nullable: true),
                    paymentterms = table.Column<string>(type: "text", nullable: true),
                    bankdetails = table.Column<string>(type: "text", nullable: true),
                    notestemplate = table.Column<string>(type: "text", nullable: true),
                    showcompanylogo = table.Column<bool>(type: "boolean", nullable: false),
                    showtaxnumber = table.Column<bool>(type: "boolean", nullable: false),
                    showbankdetails = table.Column<bool>(type: "boolean", nullable: false),
                    showpaymentterms = table.Column<bool>(type: "boolean", nullable: false),
                    papersize = table.Column<string>(type: "text", nullable: true),
                    printincolor = table.Column<bool>(type: "boolean", nullable: false),
                    printduplicatecopy = table.Column<bool>(type: "boolean", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lastmodifieddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lastmodifiedbyuserid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoicesettings", x => x.settingid);
                    table.ForeignKey(
                        name: "FK_invoicesettings_users_lastmodifiedbyuserid",
                        column: x => x.lastmodifiedbyuserid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    tripid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripnumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tripcode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tripname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    destination = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    triptype = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    startdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    enddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    totalcapacity = table.Column<int>(type: "integer", nullable: false),
                    adultcount = table.Column<int>(type: "integer", nullable: false),
                    childcount = table.Column<int>(type: "integer", nullable: false),
                    guidecost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    drivertip = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bookedseats = table.Column<int>(type: "integer", nullable: false),
                    sellingpriceperperson = table.Column<decimal>(type: "numeric", nullable: false),
                    totalcost = table.Column<decimal>(type: "numeric", nullable: false),
                    currencyid = table.Column<int>(type: "integer", nullable: false),
                    exchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    ispublished = table.Column<bool>(type: "boolean", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedby = table.Column<int>(type: "integer", nullable: true),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.tripid);
                    table.ForeignKey(
                        name: "FK_trips_currencies_currencyid",
                        column: x => x.currencyid,
                        principalTable: "currencies",
                        principalColumn: "currencyid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trips_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trips_users_updatedby",
                        column: x => x.updatedby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "umrahpackages",
                columns: table => new
                {
                    umrahpackageid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    packagenumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tripname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    numberofpersons = table.Column<int>(type: "integer", nullable: false),
                    roomtype = table.Column<int>(type: "integer", nullable: false),
                    makkahhotel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    makkahnights = table.Column<int>(type: "integer", nullable: false),
                    madinahhotel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    madinahnights = table.Column<int>(type: "integer", nullable: false),
                    transportmethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sellingprice = table.Column<decimal>(type: "numeric", nullable: false),
                    visapricesar = table.Column<decimal>(type: "numeric", nullable: false),
                    sarexchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    accommodationtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    barcodeprice = table.Column<decimal>(type: "numeric", nullable: false),
                    flightprice = table.Column<decimal>(type: "numeric", nullable: false),
                    fasttrainpricesar = table.Column<decimal>(type: "numeric", nullable: false),
                    brokername = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    supervisorname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    commission = table.Column<decimal>(type: "numeric", nullable: false),
                    supervisorexpenses = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedby = table.Column<int>(type: "integer", nullable: true),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_umrahpackages", x => x.umrahpackageid);
                    table.ForeignKey(
                        name: "FK_umrahpackages_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_umrahpackages_users_updatedby",
                        column: x => x.updatedby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "umrahtrips",
                columns: table => new
                {
                    umrahtripid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripnumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tripname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    startdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    enddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    totalpilgrims = table.Column<int>(type: "integer", nullable: false),
                    roomtype = table.Column<int>(type: "integer", nullable: false),
                    makkahhotel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    makkahnights = table.Column<int>(type: "integer", nullable: false),
                    madinahhotel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    madinahnights = table.Column<int>(type: "integer", nullable: false),
                    transportmethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    priceperperson = table.Column<decimal>(type: "numeric", nullable: false),
                    visapricesar = table.Column<decimal>(type: "numeric", nullable: false),
                    sarexchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    accommodationcost = table.Column<decimal>(type: "numeric", nullable: false),
                    barcodecost = table.Column<decimal>(type: "numeric", nullable: false),
                    flightcost = table.Column<decimal>(type: "numeric", nullable: false),
                    fasttrainpricesar = table.Column<decimal>(type: "numeric", nullable: false),
                    brokername = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    supervisorname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    brokercommission = table.Column<decimal>(type: "numeric", nullable: false),
                    supervisorexpenses = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedby = table.Column<int>(type: "integer", nullable: true),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_umrahtrips", x => x.umrahtripid);
                    table.ForeignKey(
                        name: "FK_umrahtrips_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_umrahtrips_users_updatedby",
                        column: x => x.updatedby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tripguides",
                columns: table => new
                {
                    tripguideid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripid = table.Column<int>(type: "integer", nullable: false),
                    guidename = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    languages = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    basefee = table.Column<decimal>(type: "numeric", nullable: false),
                    commissionpercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    commissionamount = table.Column<decimal>(type: "numeric", nullable: false),
                    drivertip = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripguides", x => x.tripguideid);
                    table.ForeignKey(
                        name: "FK_tripguides_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tripoptionaltours",
                columns: table => new
                {
                    tripoptionaltourid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripid = table.Column<int>(type: "integer", nullable: false),
                    tourname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    sellingprice = table.Column<decimal>(type: "numeric", nullable: false),
                    purchaseprice = table.Column<decimal>(type: "numeric", nullable: false),
                    guidecommission = table.Column<decimal>(type: "numeric", nullable: false),
                    salesrepcommission = table.Column<decimal>(type: "numeric", nullable: false),
                    participantscount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripoptionaltours", x => x.tripoptionaltourid);
                    table.ForeignKey(
                        name: "FK_tripoptionaltours_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tripprograms",
                columns: table => new
                {
                    tripprogramid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripid = table.Column<int>(type: "integer", nullable: false),
                    daynumber = table.Column<int>(type: "integer", nullable: false),
                    daytitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    daydate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activities = table.Column<string>(type: "text", nullable: false),
                    visits = table.Column<string>(type: "text", nullable: false),
                    mealsincluded = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    visitscost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    guidecost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    participantscount = table.Column<int>(type: "integer", nullable: false),
                    drivertip = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bookingtype = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripprograms", x => x.tripprogramid);
                    table.ForeignKey(
                        name: "FK_tripprograms_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "umrahpilgrims",
                columns: table => new
                {
                    umrahpilgrimid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pilgrimnumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    umrahpackageid = table.Column<int>(type: "integer", nullable: false),
                    fullname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phonenumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    identitynumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    age = table.Column<int>(type: "integer", nullable: true),
                    totalamount = table.Column<decimal>(type: "numeric", nullable: false),
                    paidamount = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    registeredat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UmrahTripId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_umrahpilgrims", x => x.umrahpilgrimid);
                    table.ForeignKey(
                        name: "FK_umrahpilgrims_umrahpackages_umrahpackageid",
                        column: x => x.umrahpackageid,
                        principalTable: "umrahpackages",
                        principalColumn: "umrahpackageid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_umrahpilgrims_umrahtrips_UmrahTripId",
                        column: x => x.UmrahTripId,
                        principalTable: "umrahtrips",
                        principalColumn: "umrahtripid");
                    table.ForeignKey(
                        name: "FK_umrahpilgrims_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "umrahpayments",
                columns: table => new
                {
                    umrahpaymentid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paymentnumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    umrahpilgrimid = table.Column<int>(type: "integer", nullable: false),
                    paymentdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    paymentmethod = table.Column<int>(type: "integer", nullable: false),
                    referencenumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UmrahPackageId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_umrahpayments", x => x.umrahpaymentid);
                    table.ForeignKey(
                        name: "FK_umrahpayments_umrahpackages_UmrahPackageId",
                        column: x => x.UmrahPackageId,
                        principalTable: "umrahpackages",
                        principalColumn: "umrahpackageid");
                    table.ForeignKey(
                        name: "FK_umrahpayments_umrahpilgrims_umrahpilgrimid",
                        column: x => x.umrahpilgrimid,
                        principalTable: "umrahpilgrims",
                        principalColumn: "umrahpilgrimid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_umrahpayments_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cashtransactions",
                columns: table => new
                {
                    transactionid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vouchernumber = table.Column<string>(type: "text", nullable: false),
                    transactiontype = table.Column<int>(type: "integer", nullable: false),
                    cashboxid = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransactionCurrency = table.Column<string>(type: "text", nullable: false),
                    ExchangeRateUsed = table.Column<decimal>(type: "numeric", nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    transactiondate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    partyname = table.Column<string>(type: "text", nullable: true),
                    paymentmethod = table.Column<int>(type: "integer", nullable: false),
                    instapaycommission = table.Column<decimal>(type: "numeric", nullable: true),
                    referencenumber = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    balancebefore = table.Column<decimal>(type: "numeric", nullable: false),
                    balanceafter = table.Column<decimal>(type: "numeric", nullable: false),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    reservationid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cashtransactions", x => x.transactionid);
                    table.ForeignKey(
                        name: "FK_cashtransactions_cashboxes_cashboxid",
                        column: x => x.cashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    reservationid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reservationnumber = table.Column<string>(type: "text", nullable: false),
                    reservationdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    servicetypeid = table.Column<int>(type: "integer", nullable: false),
                    servicedescription = table.Column<string>(type: "text", nullable: true),
                    traveldate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    returndate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    numberofpeople = table.Column<int>(type: "integer", nullable: false),
                    sellingprice = table.Column<decimal>(type: "numeric", nullable: false),
                    costprice = table.Column<decimal>(type: "numeric", nullable: false),
                    currencyid = table.Column<int>(type: "integer", nullable: true),
                    exchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    supplierid = table.Column<int>(type: "integer", nullable: true),
                    suppliercost = table.Column<decimal>(type: "numeric", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cashboxid = table.Column<int>(type: "integer", nullable: true),
                    cashtransactionid = table.Column<int>(type: "integer", nullable: true),
                    tripid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.reservationid);
                    table.ForeignKey(
                        name: "FK_reservations_cashboxes_cashboxid",
                        column: x => x.cashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_cashtransactions_cashtransactionid",
                        column: x => x.cashtransactionid,
                        principalTable: "cashtransactions",
                        principalColumn: "transactionid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_currencies_currencyid",
                        column: x => x.currencyid,
                        principalTable: "currencies",
                        principalColumn: "currencyid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_customers_customerid",
                        column: x => x.customerid,
                        principalTable: "customers",
                        principalColumn: "customerid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_servicetypes_servicetypeid",
                        column: x => x.servicetypeid,
                        principalTable: "servicetypes",
                        principalColumn: "servicetypeid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_suppliers_supplierid",
                        column: x => x.supplierid,
                        principalTable: "suppliers",
                        principalColumn: "supplierid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchaseinvoices",
                columns: table => new
                {
                    purchaseinvoiceid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoicenumber = table.Column<string>(type: "text", nullable: false),
                    invoicedate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    supplierid = table.Column<int>(type: "integer", nullable: false),
                    reservationid = table.Column<int>(type: "integer", nullable: true),
                    subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    taxrate = table.Column<decimal>(type: "numeric", nullable: false),
                    taxamount = table.Column<decimal>(type: "numeric", nullable: false),
                    totalamount = table.Column<decimal>(type: "numeric", nullable: false),
                    paidamount = table.Column<decimal>(type: "numeric", nullable: false),
                    currencyid = table.Column<int>(type: "integer", nullable: true),
                    exchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    journalentryid = table.Column<int>(type: "integer", nullable: true),
                    cashboxid = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchaseinvoices", x => x.purchaseinvoiceid);
                    table.ForeignKey(
                        name: "FK_purchaseinvoices_cashboxes_cashboxid",
                        column: x => x.cashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchaseinvoices_currencies_currencyid",
                        column: x => x.currencyid,
                        principalTable: "currencies",
                        principalColumn: "currencyid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchaseinvoices_journalentries_journalentryid",
                        column: x => x.journalentryid,
                        principalTable: "journalentries",
                        principalColumn: "journalentryid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchaseinvoices_reservations_reservationid",
                        column: x => x.reservationid,
                        principalTable: "reservations",
                        principalColumn: "reservationid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchaseinvoices_suppliers_supplierid",
                        column: x => x.supplierid,
                        principalTable: "suppliers",
                        principalColumn: "supplierid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "salesinvoices",
                columns: table => new
                {
                    salesinvoiceid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoicenumber = table.Column<string>(type: "text", nullable: false),
                    invoicedate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    reservationid = table.Column<int>(type: "integer", nullable: true),
                    subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    taxrate = table.Column<decimal>(type: "numeric", nullable: false),
                    taxamount = table.Column<decimal>(type: "numeric", nullable: false),
                    totalamount = table.Column<decimal>(type: "numeric", nullable: false),
                    paidamount = table.Column<decimal>(type: "numeric", nullable: false),
                    currencyid = table.Column<int>(type: "integer", nullable: true),
                    exchangerate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    journalentryid = table.Column<int>(type: "integer", nullable: true),
                    cashboxid = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salesinvoices", x => x.salesinvoiceid);
                    table.ForeignKey(
                        name: "FK_salesinvoices_cashboxes_cashboxid",
                        column: x => x.cashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_salesinvoices_currencies_currencyid",
                        column: x => x.currencyid,
                        principalTable: "currencies",
                        principalColumn: "currencyid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_salesinvoices_customers_customerid",
                        column: x => x.customerid,
                        principalTable: "customers",
                        principalColumn: "customerid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_salesinvoices_journalentries_journalentryid",
                        column: x => x.journalentryid,
                        principalTable: "journalentries",
                        principalColumn: "journalentryid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_salesinvoices_reservations_reservationid",
                        column: x => x.reservationid,
                        principalTable: "reservations",
                        principalColumn: "reservationid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchaseinvoiceitems",
                columns: table => new
                {
                    itemid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchaseinvoiceid = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    unitprice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchaseinvoiceitems", x => x.itemid);
                    table.ForeignKey(
                        name: "FK_purchaseinvoiceitems_purchaseinvoices_purchaseinvoiceid",
                        column: x => x.purchaseinvoiceid,
                        principalTable: "purchaseinvoices",
                        principalColumn: "purchaseinvoiceid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tripaccommodations",
                columns: table => new
                {
                    tripaccommodationid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripid = table.Column<int>(type: "integer", nullable: false),
                    accommodationtype = table.Column<int>(type: "integer", nullable: false),
                    hotelname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: true),
                    CruiseLevel = table.Column<int>(type: "integer", nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    numberofrooms = table.Column<int>(type: "integer", nullable: false),
                    roomtype = table.Column<int>(type: "integer", nullable: false),
                    numberofnights = table.Column<int>(type: "integer", nullable: false),
                    participantscount = table.Column<int>(type: "integer", nullable: false),
                    checkindate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checkoutdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    costperroompernight = table.Column<decimal>(type: "numeric", nullable: false),
                    guidecost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    drivertip = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    mealplan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    supplierid = table.Column<int>(type: "integer", nullable: true),
                    purchaseinvoiceid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripaccommodations", x => x.tripaccommodationid);
                    table.ForeignKey(
                        name: "FK_tripaccommodations_purchaseinvoices_purchaseinvoiceid",
                        column: x => x.purchaseinvoiceid,
                        principalTable: "purchaseinvoices",
                        principalColumn: "purchaseinvoiceid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripaccommodations_suppliers_supplierid",
                        column: x => x.supplierid,
                        principalTable: "suppliers",
                        principalColumn: "supplierid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripaccommodations_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tripexpenses",
                columns: table => new
                {
                    tripexpenseid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripid = table.Column<int>(type: "integer", nullable: false),
                    expensetype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    expensedate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paymentmethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiptnumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    supplierid = table.Column<int>(type: "integer", nullable: true),
                    purchaseinvoiceid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripexpenses", x => x.tripexpenseid);
                    table.ForeignKey(
                        name: "FK_tripexpenses_purchaseinvoices_purchaseinvoiceid",
                        column: x => x.purchaseinvoiceid,
                        principalTable: "purchaseinvoices",
                        principalColumn: "purchaseinvoiceid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripexpenses_suppliers_supplierid",
                        column: x => x.supplierid,
                        principalTable: "suppliers",
                        principalColumn: "supplierid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripexpenses_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tripsuppliers",
                columns: table => new
                {
                    tripsupplierid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripid = table.Column<int>(type: "integer", nullable: false),
                    supplierid = table.Column<int>(type: "integer", nullable: false),
                    supplierrole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    totalcost = table.Column<decimal>(type: "numeric", nullable: false),
                    purchaseinvoiceid = table.Column<int>(type: "integer", nullable: true),
                    paymentstatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    paidamount = table.Column<decimal>(type: "numeric", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripsuppliers", x => x.tripsupplierid);
                    table.ForeignKey(
                        name: "FK_tripsuppliers_purchaseinvoices_purchaseinvoiceid",
                        column: x => x.purchaseinvoiceid,
                        principalTable: "purchaseinvoices",
                        principalColumn: "purchaseinvoiceid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripsuppliers_suppliers_supplierid",
                        column: x => x.supplierid,
                        principalTable: "suppliers",
                        principalColumn: "supplierid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripsuppliers_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "triptransportations",
                columns: table => new
                {
                    triptransportationid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripid = table.Column<int>(type: "integer", nullable: false),
                    transportationtype = table.Column<int>(type: "integer", nullable: false),
                    vehiclemodel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    numberofvehicles = table.Column<int>(type: "integer", nullable: false),
                    seatspervehicle = table.Column<int>(type: "integer", nullable: false),
                    costpervehicle = table.Column<decimal>(type: "numeric", nullable: false),
                    suppliername = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    drivername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    driverphone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    transportdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    route = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    participantscount = table.Column<int>(type: "integer", nullable: false),
                    tourleadertip = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    drivertip = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    supplierid = table.Column<int>(type: "integer", nullable: true),
                    purchaseinvoiceid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_triptransportations", x => x.triptransportationid);
                    table.ForeignKey(
                        name: "FK_triptransportations_purchaseinvoices_purchaseinvoiceid",
                        column: x => x.purchaseinvoiceid,
                        principalTable: "purchaseinvoices",
                        principalColumn: "purchaseinvoiceid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_triptransportations_suppliers_supplierid",
                        column: x => x.supplierid,
                        principalTable: "suppliers",
                        principalColumn: "supplierid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_triptransportations_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoicepayments",
                columns: table => new
                {
                    paymentid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    salesinvoiceid = table.Column<int>(type: "integer", nullable: true),
                    purchaseinvoiceid = table.Column<int>(type: "integer", nullable: true),
                    cashboxid = table.Column<int>(type: "integer", nullable: false),
                    cashtransactionid = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    paymentdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paymentmethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    referencenumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoicepayments", x => x.paymentid);
                    table.ForeignKey(
                        name: "FK_invoicepayments_cashboxes_cashboxid",
                        column: x => x.cashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoicepayments_cashtransactions_cashtransactionid",
                        column: x => x.cashtransactionid,
                        principalTable: "cashtransactions",
                        principalColumn: "transactionid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoicepayments_purchaseinvoices_purchaseinvoiceid",
                        column: x => x.purchaseinvoiceid,
                        principalTable: "purchaseinvoices",
                        principalColumn: "purchaseinvoiceid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoicepayments_salesinvoices_salesinvoiceid",
                        column: x => x.salesinvoiceid,
                        principalTable: "salesinvoices",
                        principalColumn: "salesinvoiceid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "salesinvoiceitems",
                columns: table => new
                {
                    itemid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    salesinvoiceid = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    unitprice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salesinvoiceitems", x => x.itemid);
                    table.ForeignKey(
                        name: "FK_salesinvoiceitems_salesinvoices_salesinvoiceid",
                        column: x => x.salesinvoiceid,
                        principalTable: "salesinvoices",
                        principalColumn: "salesinvoiceid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tripbookings",
                columns: table => new
                {
                    tripbookingid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripid = table.Column<int>(type: "integer", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    bookingnumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bookingdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    numberofpersons = table.Column<int>(type: "integer", nullable: false),
                    priceperperson = table.Column<decimal>(type: "numeric", nullable: false),
                    paidamount = table.Column<decimal>(type: "numeric", nullable: false),
                    paymentstatus = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    specialrequests = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    salesinvoiceid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripbookings", x => x.tripbookingid);
                    table.ForeignKey(
                        name: "FK_tripbookings_customers_customerid",
                        column: x => x.customerid,
                        principalTable: "customers",
                        principalColumn: "customerid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripbookings_salesinvoices_salesinvoiceid",
                        column: x => x.salesinvoiceid,
                        principalTable: "salesinvoices",
                        principalColumn: "salesinvoiceid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripbookings_trips_tripid",
                        column: x => x.tripid,
                        principalTable: "trips",
                        principalColumn: "tripid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripbookings_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tripbookingpayments",
                columns: table => new
                {
                    tripbookingpaymentid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripbookingid = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    paymentdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paymentmethod = table.Column<int>(type: "integer", nullable: false),
                    referencenumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cashboxid = table.Column<int>(type: "integer", nullable: false),
                    cashtransactionid = table.Column<int>(type: "integer", nullable: true),
                    instapaycommission = table.Column<decimal>(type: "numeric", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripbookingpayments", x => x.tripbookingpaymentid);
                    table.ForeignKey(
                        name: "FK_tripbookingpayments_cashboxes_cashboxid",
                        column: x => x.cashboxid,
                        principalTable: "cashboxes",
                        principalColumn: "cashboxid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripbookingpayments_cashtransactions_cashtransactionid",
                        column: x => x.cashtransactionid,
                        principalTable: "cashtransactions",
                        principalColumn: "transactionid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tripbookingpayments_tripbookings_tripbookingid",
                        column: x => x.tripbookingid,
                        principalTable: "tripbookings",
                        principalColumn: "tripbookingid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tripbookingpayments_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tripoptionaltourbookings",
                columns: table => new
                {
                    tripoptionaltourbookingid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tripbookingid = table.Column<int>(type: "integer", nullable: false),
                    tripoptionaltourid = table.Column<int>(type: "integer", nullable: false),
                    numberofparticipants = table.Column<int>(type: "integer", nullable: false),
                    priceperperson = table.Column<decimal>(type: "numeric", nullable: false),
                    bookingdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tripoptionaltourbookings", x => x.tripoptionaltourbookingid);
                    table.ForeignKey(
                        name: "FK_tripoptionaltourbookings_tripbookings_tripbookingid",
                        column: x => x.tripbookingid,
                        principalTable: "tripbookings",
                        principalColumn: "tripbookingid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tripoptionaltourbookings_tripoptionaltours_tripoptionaltour~",
                        column: x => x.tripoptionaltourid,
                        principalTable: "tripoptionaltours",
                        principalColumn: "tripoptionaltourid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_parentaccountid",
                table: "accounts",
                column: "parentaccountid");

            migrationBuilder.CreateIndex(
                name: "IX_bankaccounts_accountnumber",
                table: "bankaccounts",
                column: "accountnumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_banktransfers_destinationbankaccountid",
                table: "banktransfers",
                column: "destinationbankaccountid");

            migrationBuilder.CreateIndex(
                name: "IX_banktransfers_destinationcashboxid",
                table: "banktransfers",
                column: "destinationcashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_banktransfers_sourcebankaccountid",
                table: "banktransfers",
                column: "sourcebankaccountid");

            migrationBuilder.CreateIndex(
                name: "IX_banktransfers_sourcecashboxid",
                table: "banktransfers",
                column: "sourcecashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_cashtransactions_cashboxid",
                table: "cashtransactions",
                column: "cashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_cashtransactions_reservationid",
                table: "cashtransactions",
                column: "reservationid");

            migrationBuilder.CreateIndex(
                name: "IX_companysettings_lastmodifiedbyuserid",
                table: "companysettings",
                column: "lastmodifiedbyuserid");

            migrationBuilder.CreateIndex(
                name: "IX_customers_accountid",
                table: "customers",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_invoicepayments_cashboxid",
                table: "invoicepayments",
                column: "cashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_invoicepayments_cashtransactionid",
                table: "invoicepayments",
                column: "cashtransactionid");

            migrationBuilder.CreateIndex(
                name: "IX_invoicepayments_purchaseinvoiceid",
                table: "invoicepayments",
                column: "purchaseinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_invoicepayments_salesinvoiceid",
                table: "invoicepayments",
                column: "salesinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_invoicesettings_lastmodifiedbyuserid",
                table: "invoicesettings",
                column: "lastmodifiedbyuserid");

            migrationBuilder.CreateIndex(
                name: "IX_journalentrylines_accountid",
                table: "journalentrylines",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_journalentrylines_journalentryid",
                table: "journalentrylines",
                column: "journalentryid");

            migrationBuilder.CreateIndex(
                name: "IX_payments_cashboxid",
                table: "payments",
                column: "cashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_payments_currencyid",
                table: "payments",
                column: "currencyid");

            migrationBuilder.CreateIndex(
                name: "IX_purchaseinvoiceitems_purchaseinvoiceid",
                table: "purchaseinvoiceitems",
                column: "purchaseinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_purchaseinvoices_cashboxid",
                table: "purchaseinvoices",
                column: "cashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_purchaseinvoices_currencyid",
                table: "purchaseinvoices",
                column: "currencyid");

            migrationBuilder.CreateIndex(
                name: "IX_purchaseinvoices_journalentryid",
                table: "purchaseinvoices",
                column: "journalentryid");

            migrationBuilder.CreateIndex(
                name: "IX_purchaseinvoices_reservationid",
                table: "purchaseinvoices",
                column: "reservationid");

            migrationBuilder.CreateIndex(
                name: "IX_purchaseinvoices_supplierid",
                table: "purchaseinvoices",
                column: "supplierid");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_cashboxid",
                table: "reservations",
                column: "cashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_cashtransactionid",
                table: "reservations",
                column: "cashtransactionid");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_createdby",
                table: "reservations",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_currencyid",
                table: "reservations",
                column: "currencyid");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_customerid",
                table: "reservations",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_servicetypeid",
                table: "reservations",
                column: "servicetypeid");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_supplierid",
                table: "reservations",
                column: "supplierid");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_tripid",
                table: "reservations",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_rolepermissions_permissionid",
                table: "rolepermissions",
                column: "permissionid");

            migrationBuilder.CreateIndex(
                name: "IX_rolepermissions_roleid",
                table: "rolepermissions",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "IX_salesinvoiceitems_salesinvoiceid",
                table: "salesinvoiceitems",
                column: "salesinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_salesinvoices_cashboxid",
                table: "salesinvoices",
                column: "cashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_salesinvoices_currencyid",
                table: "salesinvoices",
                column: "currencyid");

            migrationBuilder.CreateIndex(
                name: "IX_salesinvoices_customerid",
                table: "salesinvoices",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "IX_salesinvoices_journalentryid",
                table: "salesinvoices",
                column: "journalentryid");

            migrationBuilder.CreateIndex(
                name: "IX_salesinvoices_reservationid",
                table: "salesinvoices",
                column: "reservationid");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_accountid",
                table: "suppliers",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_tripaccommodations_purchaseinvoiceid",
                table: "tripaccommodations",
                column: "purchaseinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_tripaccommodations_supplierid",
                table: "tripaccommodations",
                column: "supplierid");

            migrationBuilder.CreateIndex(
                name: "IX_tripaccommodations_tripid",
                table: "tripaccommodations",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_tripbookingpayments_cashboxid",
                table: "tripbookingpayments",
                column: "cashboxid");

            migrationBuilder.CreateIndex(
                name: "IX_tripbookingpayments_cashtransactionid",
                table: "tripbookingpayments",
                column: "cashtransactionid");

            migrationBuilder.CreateIndex(
                name: "IX_tripbookingpayments_createdby",
                table: "tripbookingpayments",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "IX_tripbookingpayments_tripbookingid",
                table: "tripbookingpayments",
                column: "tripbookingid");

            migrationBuilder.CreateIndex(
                name: "IX_tripbookings_createdby",
                table: "tripbookings",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "IX_tripbookings_customerid",
                table: "tripbookings",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "IX_tripbookings_salesinvoiceid",
                table: "tripbookings",
                column: "salesinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_tripbookings_tripid",
                table: "tripbookings",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_tripexpenses_purchaseinvoiceid",
                table: "tripexpenses",
                column: "purchaseinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_tripexpenses_supplierid",
                table: "tripexpenses",
                column: "supplierid");

            migrationBuilder.CreateIndex(
                name: "IX_tripexpenses_tripid",
                table: "tripexpenses",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_tripguides_tripid",
                table: "tripguides",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_tripoptionaltourbookings_tripbookingid",
                table: "tripoptionaltourbookings",
                column: "tripbookingid");

            migrationBuilder.CreateIndex(
                name: "IX_tripoptionaltourbookings_tripoptionaltourid",
                table: "tripoptionaltourbookings",
                column: "tripoptionaltourid");

            migrationBuilder.CreateIndex(
                name: "IX_tripoptionaltours_tripid",
                table: "tripoptionaltours",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_tripprograms_tripid",
                table: "tripprograms",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_trips_createdby",
                table: "trips",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "IX_trips_currencyid",
                table: "trips",
                column: "currencyid");

            migrationBuilder.CreateIndex(
                name: "IX_trips_updatedby",
                table: "trips",
                column: "updatedby");

            migrationBuilder.CreateIndex(
                name: "IX_tripsuppliers_purchaseinvoiceid",
                table: "tripsuppliers",
                column: "purchaseinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_tripsuppliers_supplierid",
                table: "tripsuppliers",
                column: "supplierid");

            migrationBuilder.CreateIndex(
                name: "IX_tripsuppliers_tripid",
                table: "tripsuppliers",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_triptransportations_purchaseinvoiceid",
                table: "triptransportations",
                column: "purchaseinvoiceid");

            migrationBuilder.CreateIndex(
                name: "IX_triptransportations_supplierid",
                table: "triptransportations",
                column: "supplierid");

            migrationBuilder.CreateIndex(
                name: "IX_triptransportations_tripid",
                table: "triptransportations",
                column: "tripid");

            migrationBuilder.CreateIndex(
                name: "IX_umrahpackages_createdby",
                table: "umrahpackages",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "IX_umrahpackages_updatedby",
                table: "umrahpackages",
                column: "updatedby");

            migrationBuilder.CreateIndex(
                name: "IX_umrahpayments_createdby",
                table: "umrahpayments",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "IX_umrahpayments_UmrahPackageId",
                table: "umrahpayments",
                column: "UmrahPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_umrahpayments_umrahpilgrimid",
                table: "umrahpayments",
                column: "umrahpilgrimid");

            migrationBuilder.CreateIndex(
                name: "IX_umrahpilgrims_createdby",
                table: "umrahpilgrims",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "IX_umrahpilgrims_umrahpackageid",
                table: "umrahpilgrims",
                column: "umrahpackageid");

            migrationBuilder.CreateIndex(
                name: "IX_umrahpilgrims_UmrahTripId",
                table: "umrahpilgrims",
                column: "UmrahTripId");

            migrationBuilder.CreateIndex(
                name: "IX_umrahtrips_createdby",
                table: "umrahtrips",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "IX_umrahtrips_updatedby",
                table: "umrahtrips",
                column: "updatedby");

            migrationBuilder.CreateIndex(
                name: "IX_users_roleid",
                table: "users",
                column: "roleid");

            migrationBuilder.AddForeignKey(
                name: "FK_cashtransactions_reservations_reservationid",
                table: "cashtransactions",
                column: "reservationid",
                principalTable: "reservations",
                principalColumn: "reservationid",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cashtransactions_cashboxes_cashboxid",
                table: "cashtransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_reservations_cashboxes_cashboxid",
                table: "reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_cashtransactions_reservations_reservationid",
                table: "cashtransactions");

            migrationBuilder.DropTable(
                name: "banktransfers");

            migrationBuilder.DropTable(
                name: "companysettings");

            migrationBuilder.DropTable(
                name: "currencyexchangerates");

            migrationBuilder.DropTable(
                name: "database_backups");

            migrationBuilder.DropTable(
                name: "fiscalyearsettings");

            migrationBuilder.DropTable(
                name: "flightbookings");

            migrationBuilder.DropTable(
                name: "invoicepayments");

            migrationBuilder.DropTable(
                name: "invoicesettings");

            migrationBuilder.DropTable(
                name: "journalentrylines");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "purchaseinvoiceitems");

            migrationBuilder.DropTable(
                name: "rolepermissions");

            migrationBuilder.DropTable(
                name: "salesinvoiceitems");

            migrationBuilder.DropTable(
                name: "tripaccommodations");

            migrationBuilder.DropTable(
                name: "tripbookingpayments");

            migrationBuilder.DropTable(
                name: "tripexpenses");

            migrationBuilder.DropTable(
                name: "tripguides");

            migrationBuilder.DropTable(
                name: "tripoptionaltourbookings");

            migrationBuilder.DropTable(
                name: "tripprograms");

            migrationBuilder.DropTable(
                name: "tripsuppliers");

            migrationBuilder.DropTable(
                name: "triptransportations");

            migrationBuilder.DropTable(
                name: "umrahpayments");

            migrationBuilder.DropTable(
                name: "bankaccounts");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "tripbookings");

            migrationBuilder.DropTable(
                name: "tripoptionaltours");

            migrationBuilder.DropTable(
                name: "purchaseinvoices");

            migrationBuilder.DropTable(
                name: "umrahpilgrims");

            migrationBuilder.DropTable(
                name: "salesinvoices");

            migrationBuilder.DropTable(
                name: "umrahpackages");

            migrationBuilder.DropTable(
                name: "umrahtrips");

            migrationBuilder.DropTable(
                name: "journalentries");

            migrationBuilder.DropTable(
                name: "cashboxes");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "cashtransactions");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "servicetypes");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
