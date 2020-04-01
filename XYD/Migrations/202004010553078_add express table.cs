namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addexpresstable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Express",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        InvoiceId = c.Int(nullable: false),
                        SenderName = c.String(),
                        SenderId = c.String(),
                        ReceiverName = c.String(),
                        SendDate = c.DateTime(nullable: false),
                        ExpressNo = c.String(),
                        SettleWay = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InvoiceNo = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
        }
        
        public override void Down()
        {
            
        }
    }
}
