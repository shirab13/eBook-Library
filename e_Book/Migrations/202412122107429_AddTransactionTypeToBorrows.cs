namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTransactionTypeToBorrows : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Borrows", "TransactionType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Borrows", "TransactionType");
        }
    }
}
