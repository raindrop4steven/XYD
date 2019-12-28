namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addnewopinion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DEP_Opinion",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        MessageID = c.String(),
                        NodeKey = c.String(),
                        Opinion = c.String(),
                        order = c.Int(nullable: false),
                        CreateTime = c.DateTime(),
                        UpdatedTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DEP_Opinion");
        }
    }
}
