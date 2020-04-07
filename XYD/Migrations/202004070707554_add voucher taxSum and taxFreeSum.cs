namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addvouchertaxSumandtaxFreeSum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Voucher", "TotalTaxNum", c => c.String());
            AddColumn("dbo.XYD_Voucher", "TotalTaxFreeNum", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Voucher", "TotalTaxFreeNum");
            DropColumn("dbo.XYD_Voucher", "TotalTaxNum");
        }
    }
}
