namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DispatchingMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DUsers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Password = c.String(maxLength: 50),
                        Phone = c.String(maxLength: 50),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Terminals",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Phone = c.String(maxLength: 50),
                        CreateTime = c.DateTime(nullable: false),
                        Address = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Goods",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Price = c.Double(nullable: false),
                        Picture = c.String(maxLength: 50),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(),
                        CountPrice = c.Double(nullable: false),
                        CategoryCount = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DUsers", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.OrderGoods",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        OrderID = c.Int(),
                        GoodsID = c.Int(),
                        CountPrice = c.Double(nullable: false),
                        Count = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Goods", t => t.GoodsID)
                .ForeignKey("dbo.Orders", t => t.OrderID)
                .Index(t => t.OrderID)
                .Index(t => t.GoodsID);
            
            CreateTable(
                "dbo.TerminalDUsers",
                c => new
                    {
                        Terminal_ID = c.Int(nullable: false),
                        DUser_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Terminal_ID, t.DUser_ID })
                .ForeignKey("dbo.Terminals", t => t.Terminal_ID, cascadeDelete: true)
                .ForeignKey("dbo.DUsers", t => t.DUser_ID, cascadeDelete: true)
                .Index(t => t.Terminal_ID)
                .Index(t => t.DUser_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderGoods", "OrderID", "dbo.Orders");
            DropForeignKey("dbo.OrderGoods", "GoodsID", "dbo.Goods");
            DropForeignKey("dbo.Orders", "UserID", "dbo.DUsers");
            DropForeignKey("dbo.TerminalDUsers", "DUser_ID", "dbo.DUsers");
            DropForeignKey("dbo.TerminalDUsers", "Terminal_ID", "dbo.Terminals");
            DropIndex("dbo.TerminalDUsers", new[] { "DUser_ID" });
            DropIndex("dbo.TerminalDUsers", new[] { "Terminal_ID" });
            DropIndex("dbo.OrderGoods", new[] { "GoodsID" });
            DropIndex("dbo.OrderGoods", new[] { "OrderID" });
            DropIndex("dbo.Orders", new[] { "UserID" });
            DropTable("dbo.TerminalDUsers");
            DropTable("dbo.OrderGoods");
            DropTable("dbo.Orders");
            DropTable("dbo.Goods");
            DropTable("dbo.Terminals");
            DropTable("dbo.DUsers");
        }
    }
}
