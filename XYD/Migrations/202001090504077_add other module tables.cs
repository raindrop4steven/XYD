namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addothermoduletables : DbMigration
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
                        Category = c.Int(nullable: false),
                        Memo = c.String(),
                        Status = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Asset_Record",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AssetID = c.Int(nullable: false),
                        Operation = c.String(),
                        EmplName = c.String(),
                        DeptName = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Att",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Path = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Award",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        School = c.String(),
                        Name = c.String(),
                        Attachment = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Banner",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Iamge = c.String(),
                        order = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Contact",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        Name = c.String(),
                        Contact = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Education",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        School = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Level = c.String(),
                        Major = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_System_Config",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Area = c.String(),
                        Key = c.String(),
                        StartWorkTime = c.String(),
                        EndWorkTime = c.String(),
                        RestDays = c.Int(nullable: false),
                        Allowance = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_WorkExperience",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        CompanyName = c.String(),
                        JobName = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        JobContent = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.XYD_WorkExperience");
            DropTable("dbo.XYD_System_Config");
            DropTable("dbo.XYD_Education");
            DropTable("dbo.XYD_Contact");
            DropTable("dbo.XYD_Banner");
            DropTable("dbo.XYD_Award");
            DropTable("dbo.XYD_Att");
            DropTable("dbo.XYD_Asset_Record");
            DropTable("dbo.XYD_Asset");
        }
    }
}
