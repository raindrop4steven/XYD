namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateusercompanyinfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_UserCompanyInfo", "InternStartDate", c => c.DateTime());
            AddColumn("dbo.XYD_UserCompanyInfo", "InternEndDate", c => c.DateTime());
            AddColumn("dbo.XYD_UserCompanyInfo", "Trial", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserCompanyInfo", "BankName", c => c.String());
            DropColumn("dbo.XYD_UserCompanyInfo", "TrialDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_UserCompanyInfo", "TrialDate", c => c.DateTime());
            DropColumn("dbo.XYD_UserCompanyInfo", "BankName");
            DropColumn("dbo.XYD_UserCompanyInfo", "Trial");
            DropColumn("dbo.XYD_UserCompanyInfo", "InternEndDate");
            DropColumn("dbo.XYD_UserCompanyInfo", "InternStartDate");
        }
    }
}
