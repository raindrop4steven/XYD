namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatecarrecordtable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_CarRecord", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_CarRecord", "Status");
        }
    }
}
