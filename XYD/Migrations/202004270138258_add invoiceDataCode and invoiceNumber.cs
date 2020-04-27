namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addinvoiceDataCodeandinvoiceNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_UserCompanyInfo", "TrialDate", c => c.DateTime());
            AddColumn("dbo.XYD_Voucher", "InvoiceDataCode", c => c.String());
            AddColumn("dbo.XYD_Voucher", "InvoiceNumber", c => c.String());
            DropColumn("dbo.XYD_UserCompanyInfo", "InternStartDate");
            DropColumn("dbo.XYD_UserCompanyInfo", "InternEndDate");
            DropColumn("dbo.XYD_UserCompanyInfo", "Trial");
            DropColumn("dbo.XYD_UserCompanyInfo", "BankName");
            DropTable("dbo.XYD_Attence");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.XYD_Attence",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplNo = c.String(),
                        EmplName = c.String(),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        DeviceID = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.XYD_UserCompanyInfo", "BankName", c => c.String());
            AddColumn("dbo.XYD_UserCompanyInfo", "Trial", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserCompanyInfo", "InternEndDate", c => c.DateTime());
            AddColumn("dbo.XYD_UserCompanyInfo", "InternStartDate", c => c.DateTime());
            DropColumn("dbo.XYD_Voucher", "InvoiceNumber");
            DropColumn("dbo.XYD_Voucher", "InvoiceDataCode");
            DropColumn("dbo.XYD_UserCompanyInfo", "TrialDate");
        }
    }
}
