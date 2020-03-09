namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addbackupmoney : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_BackupMoney",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        EmplID = c.String(),
                        EmplName = c.String(),
                        DeptName = c.String(),
                        Type = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaybackTime = c.DateTime(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_BackupMoney");
        }
    }
}
