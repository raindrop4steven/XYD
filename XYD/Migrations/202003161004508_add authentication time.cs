namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addauthenticationtime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_InvoiceInfo", "authenticationTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_InvoiceInfo", "authenticationTime");
        }
    }
}
