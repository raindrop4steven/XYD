namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addbannertoconfig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_System_Config", "Banners", c => c.String());
            DropTable("dbo.XYD_Banner");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.XYD_Banner",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AttID = c.Int(nullable: false),
                        order = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.XYD_System_Config", "Banners");
        }
    }
}
