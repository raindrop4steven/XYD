namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteuseddays : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.XYD_UserCompanyInfo", "UsedRestDays");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_UserCompanyInfo", "UsedRestDays", c => c.Int(nullable: false));
        }
    }
}
