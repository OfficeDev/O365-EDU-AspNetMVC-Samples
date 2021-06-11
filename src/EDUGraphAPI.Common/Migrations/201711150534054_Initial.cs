namespace EDUGraphAPI.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClassroomSeatingArrangements",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    O365UserId = c.String(maxLength: 4000),
                    Position = c.Int(nullable: false),
                    ClassId = c.String(maxLength: 4000),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.DataSyncRecords",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TenantId = c.String(maxLength: 4000),
                    Query = c.String(maxLength: 4000),
                    DeltaLink = c.String(maxLength: 4000),
                    Updated = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Organizations",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TenantId = c.String(maxLength: 4000),
                    Name = c.String(maxLength: 4000),
                    Created = c.DateTime(nullable: false),
                    IsAdminConsented = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.AspNetRoles",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 4000),
                    Name = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");

            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                {
                    UserId = c.String(nullable: false, maxLength: 4000),
                    RoleId = c.String(nullable: false, maxLength: 4000),
                })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);

            CreateTable(
                "dbo.AspNetUsers",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 4000),
                    FirstName = c.String(maxLength: 4000),
                    LastName = c.String(maxLength: 4000),
                    OrganizationId = c.Int(),
                    O365UserId = c.String(maxLength: 4000),
                    O365Email = c.String(maxLength: 4000),
                    JobTitle = c.String(maxLength: 4000),
                    Department = c.String(maxLength: 4000),
                    MobilePhone = c.String(maxLength: 4000),
                    FavoriteColor = c.String(maxLength: 4000),
                    Email = c.String(maxLength: 256),
                    EmailConfirmed = c.Boolean(nullable: false),
                    PasswordHash = c.String(maxLength: 4000),
                    SecurityStamp = c.String(maxLength: 4000),
                    PhoneNumber = c.String(maxLength: 4000),
                    PhoneNumberConfirmed = c.Boolean(nullable: false),
                    TwoFactorEnabled = c.Boolean(nullable: false),
                    LockoutEndDateUtc = c.DateTime(),
                    LockoutEnabled = c.Boolean(nullable: false),
                    AccessFailedCount = c.Int(nullable: false),
                    UserName = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .Index(t => t.OrganizationId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");

            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(nullable: false, maxLength: 4000),
                    ClaimType = c.String(maxLength: 4000),
                    ClaimValue = c.String(maxLength: 4000),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                {
                    LoginProvider = c.String(nullable: false, maxLength: 4000),
                    ProviderKey = c.String(nullable: false, maxLength: 4000),
                    UserId = c.String(nullable: false, maxLength: 4000),
                })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.UserTokenCaches",
                c => new
                {
                    UserTokenCacheId = c.Int(nullable: false, identity: true),
                    webUserUniqueId = c.String(maxLength: 4000),
                    cacheBits = c.Binary(),
                    LastWrite = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.UserTokenCacheId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "OrganizationId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.UserTokenCaches");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Organizations");
            DropTable("dbo.DataSyncRecords");
            DropTable("dbo.ClassroomSeatingArrangements");
        }
    }
}
