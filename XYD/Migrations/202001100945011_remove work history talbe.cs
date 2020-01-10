namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeworkhistorytalbe : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.XYD_WorkflowHistory");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.XYD_WorkflowHistory",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        MessageID = c.String(),
                        Operation = c.String(),
                        Opinion = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
