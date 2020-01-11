namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addleavetableapproved : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Leave_Record", "Approved", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Leave_Record", "Approved");
        }
    }
}
