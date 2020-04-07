namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrealmonthtousercompanyinfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_UserCompanyInfo", "RealWorkMonth", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_UserCompanyInfo", "RealWorkMonth");
        }
    }
}
