namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addinvoicedeptNo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_InvoiceInfo", "deptNo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_InvoiceInfo", "deptNo");
        }
    }
}
