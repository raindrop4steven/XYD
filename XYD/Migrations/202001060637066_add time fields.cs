namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtimefields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Serial_Record", "CreateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.XYD_Serial_Record", "UpdateTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Serial_Record", "UpdateTime");
            DropColumn("dbo.XYD_Serial_Record", "CreateTime");
        }
    }
}
