namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removecustomattmodel : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.DEP_Att");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DEP_Att",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Path = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
