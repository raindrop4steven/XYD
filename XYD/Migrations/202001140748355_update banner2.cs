namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatebanner2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.XYD_Banner", "AttID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.XYD_Banner", "AttID", c => c.String());
        }
    }
}
