namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class refractorvouchermodel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Voucher", "Sn", c => c.String());
            AddColumn("dbo.XYD_Voucher", "VoucherCode", c => c.String());
            AddColumn("dbo.XYD_Voucher", "VoucherName", c => c.String());
            AddColumn("dbo.XYD_Voucher", "TotalAmount", c => c.String());
            AddColumn("dbo.XYD_Voucher", "Extras", c => c.String());
            AddColumn("dbo.XYD_Voucher", "User", c => c.String());
            AddColumn("dbo.XYD_Voucher", "ApplyUser", c => c.String());
            DropColumn("dbo.XYD_Voucher", "VoucherWord");
            DropColumn("dbo.XYD_Voucher", "Brief");
            DropColumn("dbo.XYD_Voucher", "Category");
            DropColumn("dbo.XYD_Voucher", "BorrowMoney");
            DropColumn("dbo.XYD_Voucher", "LeanMoney");
            DropColumn("dbo.XYD_Voucher", "Count");
            DropColumn("dbo.XYD_Voucher", "DeptCode");
            DropColumn("dbo.XYD_Voucher", "PersonCode");
            DropColumn("dbo.XYD_Voucher", "CustomerCode");
            DropColumn("dbo.XYD_Voucher", "VendorCode");
            DropColumn("dbo.XYD_Voucher", "CashCode");
            DropColumn("dbo.XYD_Voucher", "ExchangeRate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Voucher", "ExchangeRate", c => c.String());
            AddColumn("dbo.XYD_Voucher", "CashCode", c => c.String());
            AddColumn("dbo.XYD_Voucher", "VendorCode", c => c.String());
            AddColumn("dbo.XYD_Voucher", "CustomerCode", c => c.String());
            AddColumn("dbo.XYD_Voucher", "PersonCode", c => c.String());
            AddColumn("dbo.XYD_Voucher", "DeptCode", c => c.String());
            AddColumn("dbo.XYD_Voucher", "Count", c => c.String());
            AddColumn("dbo.XYD_Voucher", "LeanMoney", c => c.String());
            AddColumn("dbo.XYD_Voucher", "BorrowMoney", c => c.String());
            AddColumn("dbo.XYD_Voucher", "Category", c => c.String());
            AddColumn("dbo.XYD_Voucher", "Brief", c => c.String());
            AddColumn("dbo.XYD_Voucher", "VoucherWord", c => c.String());
            DropColumn("dbo.XYD_Voucher", "ApplyUser");
            DropColumn("dbo.XYD_Voucher", "User");
            DropColumn("dbo.XYD_Voucher", "Extras");
            DropColumn("dbo.XYD_Voucher", "TotalAmount");
            DropColumn("dbo.XYD_Voucher", "VoucherName");
            DropColumn("dbo.XYD_Voucher", "VoucherCode");
            DropColumn("dbo.XYD_Voucher", "Sn");
        }
    }
}
