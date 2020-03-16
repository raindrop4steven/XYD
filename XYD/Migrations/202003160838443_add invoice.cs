namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addinvoice : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_InvoiceDetail",
                c => new
                    {
                        invoiceDataCode = c.String(nullable: false, maxLength: 128),
                        invoiceNumber = c.String(nullable: false, maxLength: 128),
                        lineNum = c.String(nullable: false, maxLength: 128),
                        goodserviceName = c.String(),
                        model = c.String(),
                        unit = c.String(),
                        number = c.String(),
                        price = c.String(),
                        sum = c.String(),
                        taxRate = c.String(),
                        tax = c.String(),
                        isBillLine = c.String(),
                        zeroTaxRateSign = c.String(),
                        zeroTaxRateSignName = c.String(),
                        createdTime = c.DateTime(nullable: false),
                        createdBy = c.String(),
                        updatedTime = c.DateTime(nullable: false),
                        updatedBy = c.String(),
                    })
                .PrimaryKey(t => new { t.invoiceDataCode, t.invoiceNumber, t.lineNum })
                .ForeignKey("dbo.XYD_InvoiceInfo", t => new { t.invoiceDataCode, t.invoiceNumber }, cascadeDelete: true)
                .Index(t => new { t.invoiceDataCode, t.invoiceNumber });
            
            CreateTable(
                "dbo.XYD_InvoiceInfo",
                c => new
                    {
                        invoiceDataCode = c.String(nullable: false, maxLength: 128),
                        invoiceNumber = c.String(nullable: false, maxLength: 128),
                        voucherType = c.String(),
                        invoiceTypeName = c.String(),
                        invoiceTypeCode = c.String(),
                        billingTime = c.String(),
                        checkCode = c.String(),
                        taxDiskCode = c.String(),
                        purchaserName = c.String(),
                        taxpayerNumber = c.String(),
                        taxpayerBankAccount = c.String(),
                        taxpayerAddressOrId = c.String(),
                        salesName = c.String(),
                        salesTaxpayerNum = c.String(),
                        salesTaxpayerBankAccount = c.String(),
                        salesTaxpayerAddress = c.String(),
                        totalTaxSum = c.String(),
                        totalTaxNum = c.String(),
                        totalAmount = c.String(),
                        invoiceRemarks = c.String(),
                        isBillMark = c.String(),
                        voidMark = c.String(),
                        goodsClerk = c.String(),
                        tollSign = c.String(),
                        tollSignName = c.String(),
                        createdTime = c.DateTime(nullable: false),
                        createdBy = c.String(),
                        updatedTime = c.DateTime(nullable: false),
                        updatedBy = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.invoiceDataCode, t.invoiceNumber });
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.XYD_InvoiceDetail", new[] { "invoiceDataCode", "invoiceNumber" }, "dbo.XYD_InvoiceInfo");
            DropIndex("dbo.XYD_InvoiceDetail", new[] { "invoiceDataCode", "invoiceNumber" });
            DropTable("dbo.XYD_InvoiceInfo");
            DropTable("dbo.XYD_InvoiceDetail");
        }
    }
}
