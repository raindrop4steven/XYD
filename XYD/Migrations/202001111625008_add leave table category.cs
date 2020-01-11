namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addleavetablecategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Leave_Record", "Category", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Leave_Record", "Category");
        }
    }
}
