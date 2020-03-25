namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateuserinfotable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_UserCompanyInfo",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        EmployeeDate = c.DateTime(nullable: false),
                        TrialDate = c.DateTime(),
                        TrialSalary = c.Single(),
                        ContractDate = c.DateTime(),
                        FormalDate = c.DateTime(),
                        FormalSalary = c.Single(),
                        HousingFundNo = c.String(),
                        SocialInsuranceNo = c.String(),
                        SocialInsuranceStartDate = c.DateTime(),
                        SocialInsuranceTotalMonth = c.Int(nullable: false),
                        BankNo = c.String(),
                        ContinueCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_UserInfo",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        BirthPlace = c.String(),
                        Nation = c.String(),
                        TopDegree = c.String(),
                        Marriage = c.Boolean(nullable: false),
                        CredNo = c.String(),
                        PassportNo = c.String(),
                        ExitEntryNo = c.String(),
                        DoorNo = c.String(),
                        Residence = c.String(),
                        CurrentAddress = c.String(),
                    })
                .PrimaryKey(t => t.ID);
        }
        
        public override void Down()
        {
            
        }
    }
}
