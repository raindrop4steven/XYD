namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateasset : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Asset", "Model", c => c.String());
            AddColumn("dbo.XYD_Asset", "Unit", c => c.String());
            AddColumn("dbo.XYD_Asset", "UnitPrice", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.XYD_Asset", "Category", c => c.String());
            DropColumn("dbo.XYD_Asset", "Memo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Asset", "Memo", c => c.String());
            AlterColumn("dbo.XYD_Asset", "Category", c => c.Int(nullable: false));
            DropColumn("dbo.XYD_Asset", "UnitPrice");
            DropColumn("dbo.XYD_Asset", "Unit");
            DropColumn("dbo.XYD_Asset", "Model");
        }
    }
}
