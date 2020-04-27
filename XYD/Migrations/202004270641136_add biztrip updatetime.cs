namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addbiztripupdatetime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_BizTrip", "CreateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.XYD_BizTrip", "UpdateTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.XYD_BizTrip", "CrateTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_BizTrip", "CrateTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.XYD_BizTrip", "UpdateTime");
            DropColumn("dbo.XYD_BizTrip", "CreateTime");
        }
    }
}
