namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrestdays2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_UserCompanyInfo", "UsedRestDays", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_UserCompanyInfo", "UsedRestDays");
        }
    }
}
