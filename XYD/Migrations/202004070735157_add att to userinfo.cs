namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addatttouserinfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_UserInfo", "CredFront", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserInfo", "CredBack", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserInfo", "PassportFront", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserInfo", "PassportBack", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserInfo", "ExitEntryFront", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserInfo", "ExitEntryBack", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserInfo", "TaiNo", c => c.String());
            AddColumn("dbo.XYD_UserInfo", "TaiFront", c => c.Int(nullable: false));
            AddColumn("dbo.XYD_UserInfo", "TaiEnd", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_UserInfo", "TaiEnd");
            DropColumn("dbo.XYD_UserInfo", "TaiFront");
            DropColumn("dbo.XYD_UserInfo", "TaiNo");
            DropColumn("dbo.XYD_UserInfo", "ExitEntryBack");
            DropColumn("dbo.XYD_UserInfo", "ExitEntryFront");
            DropColumn("dbo.XYD_UserInfo", "PassportBack");
            DropColumn("dbo.XYD_UserInfo", "PassportFront");
            DropColumn("dbo.XYD_UserInfo", "CredBack");
            DropColumn("dbo.XYD_UserInfo", "CredFront");
        }
    }
}
