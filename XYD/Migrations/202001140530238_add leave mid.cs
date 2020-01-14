namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addleavemid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Leave_Record", "MessageID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Leave_Record", "MessageID");
        }
    }
}
