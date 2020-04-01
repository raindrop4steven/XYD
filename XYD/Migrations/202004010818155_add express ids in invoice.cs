namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addexpressidsininvoice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_InvoiceInfo", "express", c => c.String());
            DropColumn("dbo.XYD_Express", "InvoiceId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Express", "InvoiceId", c => c.Int(nullable: false));
            DropColumn("dbo.XYD_InvoiceInfo", "express");
        }
    }
}
