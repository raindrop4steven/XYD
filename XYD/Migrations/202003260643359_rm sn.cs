namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rmsn : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.XYD_Asset", "Sn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Asset", "Sn", c => c.String());
        }
    }
}
