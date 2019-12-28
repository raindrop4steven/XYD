namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatemessagealarmmodel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DEP_MessageAlarm", "AlarmDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DEP_MessageAlarm", "AlarmDate", c => c.DateTime(nullable: false));
        }
    }
}
