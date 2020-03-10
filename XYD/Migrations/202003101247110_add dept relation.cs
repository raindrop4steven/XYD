namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adddeptrelation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Dept_Relation",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DeptID = c.String(),
                        cDepCode = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_Dept_Relation");
        }
    }
}
