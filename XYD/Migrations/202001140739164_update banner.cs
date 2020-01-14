namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatebanner : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Banner", "AttID", c => c.String());
            DropColumn("dbo.XYD_Banner", "Iamge");
        }
        
        public override void Down()
        {
            AddColumn("dbo.XYD_Banner", "Iamge", c => c.String());
            DropColumn("dbo.XYD_Banner", "AttID");
        }
    }
}
