namespace e_Book.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeDueDateNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Borrows", "DueDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Borrows", "DueDate", c => c.DateTime(nullable: false));
        }
    }
}
