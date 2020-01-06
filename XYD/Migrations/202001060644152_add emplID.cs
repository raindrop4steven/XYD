namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addemplID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Serial_Record", "EmplID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Serial_Record", "EmplID");
        }
    }
}
