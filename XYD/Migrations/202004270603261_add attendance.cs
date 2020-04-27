namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addattendance : DbMigration
    {
        public override void Up()
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
            DropTable("dbo.XYD_Attence");
        }
    }
}
