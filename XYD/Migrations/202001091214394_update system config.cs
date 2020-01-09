namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatesystemconfig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_System_Config", "Config", c => c.String());
            DropColumn("dbo.XYD_System_Config", "Key");
            DropColumn("dbo.XYD_System_Config", "StartWorkTime");
            DropColumn("dbo.XYD_System_Config", "EndWorkTime");
            DropColumn("dbo.XYD_System_Config", "RestDays");
            DropColumn("dbo.XYD_System_Config", "Allowance");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_System_Config", "Allowance", c => c.Single(nullable: false));
            AddColumn("dbo.XYD_System_Config", "RestDays", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_System_Config", "EndWorkTime", c => c.String());
            AddColumn("dbo.XYD_System_Config", "StartWorkTime", c => c.String());
            AddColumn("dbo.XYD_System_Config", "Key", c => c.String());
            DropColumn("dbo.XYD_System_Config", "Config");
        }
    }
}
