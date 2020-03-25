namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcodeinassetcategory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Asset_Category",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Name = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
        }
        
        public override void Down()
        {
            
        }
    }
}
