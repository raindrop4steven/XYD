namespace DeptOA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addsubflowrelation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DEP_SubflowRelation",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        OriginMessageID = c.String(),
                        SubflowMessageID = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DEP_SubflowRelation");
        }
    }
}
