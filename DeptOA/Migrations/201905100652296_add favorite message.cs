namespace DeptOA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfavoritemessage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DEP_FavoriteMessage",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        EmplID = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DEP_FavoriteMessage");
        }
    }
}
