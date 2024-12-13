namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Books",
                c => new
                    {
                        BookId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Author = c.String(),
                        Publisher = c.String(),
                        Genre = c.String(),
                        Popularity = c.Int(nullable: false),
                        PriceBuy = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PreviousPrice = c.Decimal(precision: 18, scale: 2),
                        DiscountEndDate = c.DateTime(),
                        PriceBorrow = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvailableCopies = c.Int(nullable: false),
                        Format = c.String(),
                        YearPublished = c.Int(nullable: false),
                        IsBorrowable = c.Boolean(nullable: false),
                        Synopsis = c.String(),
                        ImageUrl = c.String(),
                        AgeRestriction = c.String(),
                    })
                .PrimaryKey(t => t.BookId);
            
            CreateTable(
                "dbo.Borrows",
                c => new
                    {
                        BorrowId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        BorrowDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        IsReturned = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BorrowId)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.BookId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        Role = c.String(),
                        IdentityCard = c.String(),
                        Age = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.CartItems",
                c => new
                    {
                        CartItemId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CartItemId)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.BookId);
            
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        FeedbackId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(),
                        Content = c.String(),
                        Rating = c.Int(nullable: false),
                        FeedbackDate = c.DateTime(nullable: false),
                        IsPurchaseFeedback = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.FeedbackId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.String(),
                        PaymentDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.BookId);
            
            CreateTable(
                "dbo.Purchases",
                c => new
                    {
                        PurchaseId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        PurchasePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PurchaseDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PurchaseId);
            
            CreateTable(
                "dbo.WaitingLists",
                c => new
                    {
                        WaitingListId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        BookId = c.Int(nullable: false),
                        AddedDate = c.DateTime(nullable: false),
                        Position = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.WaitingListId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Payments", "BookId", "dbo.Books");
            DropForeignKey("dbo.CartItems", "UserId", "dbo.Users");
            DropForeignKey("dbo.CartItems", "BookId", "dbo.Books");
            DropForeignKey("dbo.Borrows", "UserId", "dbo.Users");
            DropForeignKey("dbo.Borrows", "BookId", "dbo.Books");
            DropIndex("dbo.Payments", new[] { "BookId" });
            DropIndex("dbo.Payments", new[] { "UserId" });
            DropIndex("dbo.CartItems", new[] { "BookId" });
            DropIndex("dbo.CartItems", new[] { "UserId" });
            DropIndex("dbo.Borrows", new[] { "BookId" });
            DropIndex("dbo.Borrows", new[] { "UserId" });
            DropTable("dbo.WaitingLists");
            DropTable("dbo.Purchases");
            DropTable("dbo.Payments");
            DropTable("dbo.Feedbacks");
            DropTable("dbo.CartItems");
            DropTable("dbo.Users");
            DropTable("dbo.Borrows");
            DropTable("dbo.Books");
        }
    }
}
