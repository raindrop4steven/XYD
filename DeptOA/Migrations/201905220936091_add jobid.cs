namespace DeptOA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addjobid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DEP_MessageAlarm", "JobID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DEP_MessageAlarm", "JobID");
        }
    }
}
