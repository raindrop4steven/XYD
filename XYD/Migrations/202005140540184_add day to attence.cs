namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adddaytoattence : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.XYD_Attence", "Day", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.XYD_Attence", "Day");
        }
    }
}
