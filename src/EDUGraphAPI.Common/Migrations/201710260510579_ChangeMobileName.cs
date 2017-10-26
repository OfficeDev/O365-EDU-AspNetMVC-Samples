namespace EDUGraphAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeMobileName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "MobilePhone", c => c.String(maxLength: 4000));
            DropColumn("dbo.AspNetUsers", "Mobile");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Mobile", c => c.String(maxLength: 4000));
            DropColumn("dbo.AspNetUsers", "MobilePhone");
        }
    }
}
