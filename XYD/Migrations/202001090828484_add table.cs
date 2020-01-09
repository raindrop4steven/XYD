namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Asset_Category",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.XYD_Award", "School");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Award", "School", c => c.String());
            DropTable("dbo.XYD_Asset_Category");
        }
    }
}
