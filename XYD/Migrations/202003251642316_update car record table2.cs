namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatecarrecordtable2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.XYD_CarRecord", "StartMiles", c => c.Int(nullable: false));
            AlterColumn("dbo.XYD_CarRecord", "EndMiles", c => c.Int(nullable: false));
            AlterColumn("dbo.XYD_CarRecord", "Miles", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.XYD_CarRecord", "Miles", c => c.Single(nullable: false));
            AlterColumn("dbo.XYD_CarRecord", "EndMiles", c => c.Single(nullable: false));
            AlterColumn("dbo.XYD_CarRecord", "StartMiles", c => c.Single(nullable: false));
        }
    }
}
