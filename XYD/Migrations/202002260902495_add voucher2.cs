namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addvoucher2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Voucher",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        VoucherWord = c.String(),
                        Brief = c.String(),
                        Category = c.String(),
                        BorrowMoney = c.String(),
                        LeanMoney = c.String(),
                        Count = c.String(),
                        DeptCode = c.String(),
                        PersonCode = c.String(),
                        CustomerCode = c.String(),
                        VendorCode = c.String(),
                        CashCode = c.String(),
                        ExchangeRate = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_Voucher");
        }
    }
}
