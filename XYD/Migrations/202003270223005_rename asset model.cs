namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renameassetmodel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Asset", "ModelName", c => c.String());
            DropColumn("dbo.XYD_Asset", "Model");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Asset", "Model", c => c.String());
            DropColumn("dbo.XYD_Asset", "ModelName");
        }
    }
}
