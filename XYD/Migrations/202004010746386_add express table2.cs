namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addexpresstable2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.XYD_Express", "SenderName");
            DropColumn("dbo.XYD_Express", "ReceiverName");
            DropColumn("dbo.XYD_Express", "SendDate");
            DropColumn("dbo.XYD_Express", "ExpressNo");
            DropColumn("dbo.XYD_Express", "SettleWay");
            DropColumn("dbo.XYD_Express", "InvoiceNo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Express", "InvoiceNo", c => c.String());
            AddColumn("dbo.XYD_Express", "SettleWay", c => c.String());
            AddColumn("dbo.XYD_Express", "ExpressNo", c => c.String());
            AddColumn("dbo.XYD_Express", "SendDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.XYD_Express", "ReceiverName", c => c.String());
            AddColumn("dbo.XYD_Express", "SenderName", c => c.String());
        }
    }
}
