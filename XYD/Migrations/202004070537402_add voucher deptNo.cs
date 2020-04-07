namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addvoucherdeptNo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Voucher", "DeptNo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Voucher", "DeptNo");
        }
    }
}
