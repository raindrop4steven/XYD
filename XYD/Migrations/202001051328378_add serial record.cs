namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addserialrecord : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Serial_Record",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        MessageID = c.String(),
                        WorkflowID = c.String(),
                        Sn = c.String(),
                        Used = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_Serial_Record");
        }
    }
}
