namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addareatoasset : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Asset", "Area", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Asset", "Area");
        }
    }
}
