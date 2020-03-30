namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addWapper : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderWappers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        OrderID = c.Int(),
                        WapperID = c.Int(),
                        CountPrice = c.Double(nullable: false),
                        Count = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Orders", t => t.OrderID)
                .ForeignKey("dbo.Wappers", t => t.WapperID)
                .Index(t => t.OrderID)
                .Index(t => t.WapperID);
            
            CreateTable(
                "dbo.Wappers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Spec = c.String(maxLength: 100),
                        Description = c.String(maxLength: 200),
                        Price = c.Double(nullable: false),
                        Picture = c.String(maxLength: 500),
                        Point = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Orders", "WapperCountPrice", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderWappers", "WapperID", "dbo.Wappers");
            DropForeignKey("dbo.OrderWappers", "OrderID", "dbo.Orders");
            DropIndex("dbo.OrderWappers", new[] { "WapperID" });
            DropIndex("dbo.OrderWappers", new[] { "OrderID" });
            DropColumn("dbo.Orders", "WapperCountPrice");
            DropTable("dbo.Wappers");
            DropTable("dbo.OrderWappers");
        }
    }
}
