namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class 添加用户基本信息 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_UserInfo",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        CredNo = c.String(),
                        BankNo = c.String(),
                        DoorNo = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_UserInfo");
        }
    }
}
