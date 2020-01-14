namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addleavestatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Leave_Record", "Status", c => c.String());
            DropColumn("dbo.XYD_Leave_Record", "Approved");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Leave_Record", "Approved", c => c.Boolean(nullable: false));
            DropColumn("dbo.XYD_Leave_Record", "Status");
        }
    }
}
