namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrestdays : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_UserCompanyInfo", "RestDays", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserCompanyInfo", "ManualCaculate", c => c.Boolean(nullable: false));
            AddColumn("dbo.XYD_UserCompanyInfo", "UsedRestDays", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_UserCompanyInfo", "UsedRestDays");
            DropColumn("dbo.XYD_UserCompanyInfo", "ManualCaculate");
            DropColumn("dbo.XYD_UserCompanyInfo", "RestDays");
        }
    }
}
