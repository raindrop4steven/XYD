namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changemeettingtimetonullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.XYD_MettingBook", "StartTime", c => c.DateTime());
            AlterColumn("dbo.XYD_MettingBook", "EndTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.XYD_MettingBook", "EndTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.XYD_MettingBook", "StartTime", c => c.DateTime(nullable: false));
        }
    }
}
