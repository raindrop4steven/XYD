namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateuserinfotable2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.XYD_UserCompanyInfo", "EmployeeDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.XYD_UserCompanyInfo", "EmployeeDate", c => c.DateTime(nullable: false));
        }
    }
}
