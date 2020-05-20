namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addreststarttimeandendtime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_System_Config", "RestStartTime", c => c.String());
            AddColumn("dbo.XYD_System_Config", "RestEndTime", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_System_Config", "RestEndTime");
            DropColumn("dbo.XYD_System_Config", "RestStartTime");
        }
    }
}