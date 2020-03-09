namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addbackupmoneydeptIdstatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_BackupMoney", "DeptID", c => c.String());
            AddColumn("dbo.XYD_BackupMoney", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_BackupMoney", "Status");
            DropColumn("dbo.XYD_BackupMoney", "DeptID");
        }
    }
}
