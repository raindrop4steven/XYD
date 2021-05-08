namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addholidaytype : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Holiday", "Type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Holiday", "Type");
        }
    }
}
