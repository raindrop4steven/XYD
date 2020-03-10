namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rmdeptrelation : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.XYD_Dept_Relation");
        }
        
        public override void Down()
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
    }
}
