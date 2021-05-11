namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addassetsavemodel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Asset_Save",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Sn = c.String(),
                        Ids = c.String(),
                        Operator = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_Asset_Save");
        }
    }
}
