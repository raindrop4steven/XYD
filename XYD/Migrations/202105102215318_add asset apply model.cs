namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addassetapplymodel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_AssetApply",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        mid = c.String(),
                        area = c.String(),
                        assets = c.String(),
                        sn = c.String(),
                        used = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_AssetApply");
        }
    }
}
