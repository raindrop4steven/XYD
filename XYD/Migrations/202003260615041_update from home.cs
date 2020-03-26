namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatefromhome : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Asset",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Sn = c.String(),
                        Name = c.String(),
                        Model = c.String(),
                        Count = c.Int(nullable: false),
                        Unit = c.String(),
                        UnitPrice = c.Decimal(precision: 18, scale: 2),
                        Category = c.String(),
                        Area = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
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
            
            CreateTable(
                "dbo.XYD_Asset_Record",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AssetID = c.Int(nullable: false),
                        Count = c.Int(nullable: false),
                        Operation = c.String(),
                        EmplName = c.String(),
                        DeptName = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_CarRecord",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        ApplyUserID = c.String(),
                        ApplyDept = c.String(),
                        ApplyUser = c.String(),
                        ApplyDate = c.DateTime(nullable: false),
                        Reason = c.String(),
                        Location = c.String(),
                        DriverID = c.String(),
                        CarNo = c.String(),
                        StartMiles = c.Int(nullable: false),
                        EndMiles = c.Int(nullable: false),
                        Miles = c.Int(nullable: false),
                        Status = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
        }
        
        public override void Down()
        {
            
        }
    }
}
