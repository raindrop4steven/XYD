namespace XYD.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class createalltables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.XYD_Asset",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ModelName = c.String(),
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
                "dbo.XYD_Att",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Path = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Attence",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplNo = c.String(),
                        EmplName = c.String(),
                        Day = c.String(),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        DeviceID = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Audit_Record",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        NodeName = c.String(),
                        EmplID = c.String(),
                        MessageID = c.String(),
                        Operation = c.String(),
                        Opinion = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Award",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        Name = c.String(),
                        Attachment = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_BackupMoney",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        EmplID = c.String(),
                        EmplName = c.String(),
                        DeptID = c.String(),
                        DeptName = c.String(),
                        Type = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaybackTime = c.DateTime(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_BizTrip",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        EmplID = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
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
                "dbo.XYD_Express",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SenderId = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_InvoiceDetail",
                c => new
                    {
                        invoiceDataCode = c.String(nullable: false, maxLength: 128),
                        invoiceNumber = c.String(nullable: false, maxLength: 128),
                        lineNum = c.String(nullable: false, maxLength: 128),
                        goodserviceName = c.String(),
                        model = c.String(),
                        unit = c.String(),
                        number = c.String(),
                        price = c.String(),
                        sum = c.String(),
                        taxRate = c.String(),
                        tax = c.String(),
                        isBillLine = c.String(),
                        zeroTaxRateSign = c.String(),
                        zeroTaxRateSignName = c.String(),
                        createdTime = c.DateTime(nullable: false),
                        createdBy = c.String(),
                        updatedTime = c.DateTime(nullable: false),
                        updatedBy = c.String(),
                    })
                .PrimaryKey(t => new { t.invoiceDataCode, t.invoiceNumber, t.lineNum })
                .ForeignKey("dbo.XYD_InvoiceInfo", t => new { t.invoiceDataCode, t.invoiceNumber }, cascadeDelete: true)
                .Index(t => new { t.invoiceDataCode, t.invoiceNumber });
            
            CreateTable(
                "dbo.XYD_InvoiceInfo",
                c => new
                    {
                        invoiceDataCode = c.String(nullable: false, maxLength: 128),
                        invoiceNumber = c.String(nullable: false, maxLength: 128),
                        voucherType = c.String(),
                        deptNo = c.String(),
                        invoiceTypeName = c.String(),
                        invoiceTypeCode = c.String(),
                        billingTime = c.String(),
                        checkCode = c.String(),
                        taxDiskCode = c.String(),
                        purchaserName = c.String(),
                        taxpayerNumber = c.String(),
                        taxpayerBankAccount = c.String(),
                        taxpayerAddressOrId = c.String(),
                        salesName = c.String(),
                        salesTaxpayerNum = c.String(),
                        salesTaxpayerBankAccount = c.String(),
                        salesTaxpayerAddress = c.String(),
                        totalTaxSum = c.String(),
                        totalTaxNum = c.String(),
                        totalAmount = c.String(),
                        invoiceRemarks = c.String(),
                        isBillMark = c.String(),
                        voidMark = c.String(),
                        goodsClerk = c.String(),
                        tollSign = c.String(),
                        tollSignName = c.String(),
                        express = c.String(),
                        authenticationTime = c.DateTime(),
                        createdTime = c.DateTime(nullable: false),
                        createdBy = c.String(),
                        updatedTime = c.DateTime(nullable: false),
                        updatedBy = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.invoiceDataCode, t.invoiceNumber });
            
            CreateTable(
                "dbo.XYD_Leave_Record",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        MessageID = c.String(),
                        Category = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Reason = c.String(),
                        Status = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_MettingBook",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        Area = c.String(),
                        MeetingRoom = c.String(),
                        Name = c.String(),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        Agreed = c.Boolean(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Serial_No",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Year = c.Int(nullable: false),
                        Number = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Serial_Record",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        MessageID = c.String(),
                        EmplID = c.String(),
                        WorkflowID = c.String(),
                        Sn = c.String(),
                        Used = c.Boolean(nullable: false),
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
                        StartWorkTime = c.String(),
                        EndWorkTime = c.String(),
                        RestStartTime = c.String(),
                        RestEndTime = c.String(),
                        RestDays = c.Int(nullable: false),
                        Allowance = c.Single(nullable: false),
                        Banners = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_UserCompanyInfo",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        EmployeeDate = c.DateTime(),
                        InternStartDate = c.DateTime(),
                        InternEndDate = c.DateTime(),
                        Trial = c.Int(nullable: false),
                        TrialSalary = c.Single(),
                        ContractDate = c.DateTime(),
                        FormalDate = c.DateTime(),
                        FormalSalary = c.Single(),
                        HousingFundNo = c.String(),
                        SocialInsuranceNo = c.String(),
                        SocialInsuranceStartDate = c.DateTime(),
                        SocialInsuranceTotalMonth = c.Int(nullable: false),
                        BankName = c.String(),
                        BankNo = c.String(),
                        ContinueCount = c.Int(nullable: false),
                        RealWorkMonth = c.Int(nullable: false),
                        RestDays = c.Int(nullable: false),
                        ManualCaculate = c.Boolean(nullable: false),
                        UsedRestDays = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_UserInfo",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmplID = c.String(),
                        BirthPlace = c.String(),
                        Nation = c.String(),
                        TopDegree = c.String(),
                        Marriage = c.Boolean(nullable: false),
                        CredNo = c.String(),
                        CredFront = c.Int(nullable: false),
                        CredBack = c.Int(nullable: false),
                        PassportNo = c.String(),
                        PassportFront = c.Int(nullable: false),
                        PassportBack = c.Int(nullable: false),
                        ExitEntryNo = c.String(),
                        ExitEntryFront = c.Int(nullable: false),
                        ExitEntryBack = c.Int(nullable: false),
                        TaiNo = c.String(),
                        TaiFront = c.Int(nullable: false),
                        TaiEnd = c.Int(nullable: false),
                        DoorNo = c.String(),
                        Residence = c.String(),
                        CurrentAddress = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Vendor",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.XYD_Voucher",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MessageID = c.String(),
                        InvoiceDataCode = c.String(),
                        InvoiceNumber = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        Sn = c.String(),
                        VoucherCode = c.String(),
                        VoucherName = c.String(),
                        DeptNo = c.String(),
                        CustomerNo = c.String(),
                        VendorNo = c.String(),
                        TotalTaxNum = c.String(),
                        TotalTaxFreeNum = c.String(),
                        TotalAmount = c.String(),
                        Extras = c.String(),
                        User = c.String(),
                        ApplyUser = c.String(),
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
            DropForeignKey("dbo.XYD_InvoiceDetail", new[] { "invoiceDataCode", "invoiceNumber" }, "dbo.XYD_InvoiceInfo");
            DropIndex("dbo.XYD_InvoiceDetail", new[] { "invoiceDataCode", "invoiceNumber" });
            DropTable("dbo.XYD_WorkExperience");
            DropTable("dbo.XYD_Voucher");
            DropTable("dbo.XYD_Vendor");
            DropTable("dbo.XYD_UserInfo");
            DropTable("dbo.XYD_UserCompanyInfo");
            DropTable("dbo.XYD_System_Config");
            DropTable("dbo.XYD_Serial_Record");
            DropTable("dbo.XYD_Serial_No");
            DropTable("dbo.XYD_MettingBook");
            DropTable("dbo.XYD_Leave_Record");
            DropTable("dbo.XYD_InvoiceInfo");
            DropTable("dbo.XYD_InvoiceDetail");
            DropTable("dbo.XYD_Express");
            DropTable("dbo.XYD_Education");
            DropTable("dbo.XYD_Contact");
            DropTable("dbo.XYD_CarRecord");
            DropTable("dbo.XYD_BizTrip");
            DropTable("dbo.XYD_BackupMoney");
            DropTable("dbo.XYD_Award");
            DropTable("dbo.XYD_Audit_Record");
            DropTable("dbo.XYD_Attence");
            DropTable("dbo.XYD_Att");
            DropTable("dbo.XYD_Asset_Record");
            DropTable("dbo.XYD_Asset_Category");
            DropTable("dbo.XYD_Asset");
        }
    }
}
