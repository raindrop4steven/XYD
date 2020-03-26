namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateassettableremovestatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Asset", "Count", c => c.Int(nullable: false));
            DropColumn("dbo.XYD_Asset", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Asset", "Status", c => c.String());
            DropColumn("dbo.XYD_Asset", "Count");
        }
    }
}
