namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addnodenameinauditrecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Audit_Record", "NodeName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Audit_Record", "NodeName");
        }
    }
}
