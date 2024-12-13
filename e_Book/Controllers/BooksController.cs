﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eBookLibrary.Models;
using e_Book.Models;
using System.Web.Security;

namespace e_Book.Controllers
{
    public class BooksController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        // GET: Books
        [AllowAnonymous]
        public ActionResult Index()
        {
            //ViewBag.Role = GetUserRole(); // שליפת התפקיד
            return View(db.Books.ToList());
        }



        // GET: Books/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Books/Create
        public ActionResult Create()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {

                return RedirectToAction("Login", "Account");
            }
            var newBook = new Book
            {
                AgeRestriction = "All Ages" // Default value for AgeRestriction
            };

            return View(newBook);
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookId,Title,Author,Publisher,PriceBuy,PriceBorrow,AvailableCopies,Format,YearPublished,IsBorrowable,AgeRestriction,ImageUrl,Synopsis")] Book book)
        {
            // בדיקה אם מחיר ההשאלה קטן ממחיר הקנייה
            if (book.PriceBorrow >= book.PriceBuy)
            {
                ModelState.AddModelError("PriceBorrow", "מחיר ההשאלה חייב להיות קטן ממחיר הקנייה.");
            }

            if (ModelState.IsValid)
            {
                // בדיקה אם כבר קיים ספר עם אותו שם, מחבר ומוציא לאור
                var existingBook = db.Books.FirstOrDefault(b => b.Title == book.Title && b.Author == book.Author && b.Publisher == book.Publisher);

                if (existingBook != null)
                {
                    // אם הספר כבר קיים עם אותם פרטים
                    TempData["Error"] = "ספר זה כבר קיים במערכת עם אותו שם, מחבר ומוציא לאור.";
                    return View(book);
                }

                // אם התנאים תקינים, הוספת הספר
                db.Books.Add(book);
                db.SaveChanges();
                TempData["Success"] = "הספר נוסף בהצלחה!";
                return RedirectToAction("Index");
            }

            // במקרה של שגיאה במודל
            return View(book);
        }



        // GET: Books/Edit/5
        public ActionResult Edit(int? id)
        {

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookId,Title,Author,Publisher,PriceBuy,PriceBorrow,PreviousPrice,AvailableCopies,Format,Genre,Popularity,YearPublished,IsBorrowable,DiscountEndDate,AgeRestriction,ImageUrl,Synopsis")] Book book)
        {
            if (ModelState.IsValid)
            {
                // בדיקה אם מחיר ההשאלה קטן ממחיר הקנייה
                if (book.PriceBorrow >= book.PriceBuy)
                {
                    ModelState.AddModelError("PriceBorrow", "מחיר ההשאלה חייב להיות קטן ממחיר הקנייה.");
                    return View(book); // חזרה לטופס אם יש בעיה
                }

                // שליפת הספר המקורי ממסד הנתונים
                var originalBook = db.Books.AsNoTracking().FirstOrDefault(b => b.BookId == book.BookId);

                if (originalBook != null && book.PriceBuy < originalBook.PriceBuy)
                {
                    // עדכון מחיר קודם
                    book.PreviousPrice = originalBook.PriceBuy;
                    // הגדרת תאריך סיום הנחה לשבוע מהיום
                    book.DiscountEndDate = DateTime.Now.AddDays(7);
                }

                // עדכון הספר במסד הנתונים
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "הספר עודכן בהצלחה!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "שגיאה בעדכון הספר.";
            return View(book);
        }



        // GET: Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [AllowAnonymous]
        public ActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var books = db.Books
                .Where(b => b.Title.Contains(query) || b.Author.Contains(query) || b.Publisher.Contains(query))
                .ToList();

            return View("Index", books); // השתמשי בתצוגת Index כדי להציג את התוצאות
        }

        [AllowAnonymous]
        public ActionResult Filter(decimal? minPrice, decimal? maxPrice, string format)
        {
            var books = db.Books.AsQueryable();

            if (minPrice.HasValue)
            {
                books = books.Where(b => b.PriceBuy >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.PriceBuy <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(format))
            {
                books = books.Where(b => b.Format == format);
            }

            return View("Index", books.ToList()); // הצגת התוצאות בתצוגת Index
        }

        [AllowAnonymous]
        public ActionResult Sort(string sortBy)
        {
            var books = db.Books.AsQueryable();

            switch (sortBy)
            {
                case "price_asc":
                    books = books.OrderBy(b => b.PriceBuy);
                    break;
                case "price_desc":
                    books = books.OrderByDescending(b => b.PriceBuy);
                    break;
                case "year":
                    books = books.OrderByDescending(b => b.YearPublished);
                    break;
                case "year_asc":
                    books = books.OrderBy(b => b.YearPublished); 
                    break;
                default:
                    books = books.OrderBy(b => b.Title);
                    break;
            }

            return View("Index", books.ToList());
        }

        [AllowAnonymous]
        public ActionResult FilterDiscountedBooks()
        {
            var discountedBooks = db.Books
                .Where(b => b.PreviousPrice.HasValue && b.DiscountEndDate.HasValue && b.DiscountEndDate > DateTime.Now)
                .ToList();

            return View("Index", discountedBooks); // מציג את הספרים בתצוגת Index
        }


        private string GetUserRole()
        {
            if (Session["Role"] != null)
            {
                return Session["Role"].ToString();
            }

            if (User.Identity.IsAuthenticated)
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                FormsAuthenticationTicket ticket = identity.Ticket;
                return ticket.UserData; // התפקיד מאוחסן ב-UserData
            }

            return null; // משתמש לא מחובר
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePrice(int bookId, decimal newPrice, DateTime? discountEndDate = null)
        {
            var book = db.Books.Find(bookId);
            if (book == null)
            {
                TempData["Error"] = "הספר לא נמצא.";
                return RedirectToAction("Index");
            }

            if (newPrice < book.PriceBuy)
            {
                book.PreviousPrice = book.PriceBuy; // שמירת המחיר הקודם
                book.PriceBuy = newPrice;
                book.DiscountEndDate = discountEndDate ?? DateTime.Now.AddDays(7); // ברירת מחדל: שבוע
            }
            else
            {
                book.PriceBuy = newPrice;
                book.PreviousPrice = null;
                book.DiscountEndDate = null;
            }

            db.SaveChanges();
            TempData["Success"] = "מחיר הספר עודכן בהצלחה.";
            return RedirectToAction("Index");
        }





    }
}
