namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rollbacktooriginconfigtable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_System_Config", "StartWorkTime", c => c.String());
            AddColumn("dbo.XYD_System_Config", "EndWorkTime", c => c.String());
            AddColumn("dbo.XYD_System_Config", "RestDays", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_System_Config", "Allowance", c => c.Single(nullable: false));
            DropColumn("dbo.XYD_System_Config", "Config");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_System_Config", "Config", c => c.String());
            DropColumn("dbo.XYD_System_Config", "Allowance");
            DropColumn("dbo.XYD_System_Config", "RestDays");
            DropColumn("dbo.XYD_System_Config", "EndWorkTime");
            DropColumn("dbo.XYD_System_Config", "StartWorkTime");
        }
    }
}
