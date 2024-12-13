namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTransactionTypeToCartItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartItems", "TransactionType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartItems", "TransactionType");
        }
    }
}
