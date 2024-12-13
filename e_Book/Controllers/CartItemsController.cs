using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eBookLibrary.Models;
using e_Book.Models;

namespace e_Book.Controllers
{
    public class CartItemsController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        // GET: CartItems
        public ActionResult Index()
        {
            var userId = GetLoggedInUserId(); // פונקציה שמחזירה את מזהה המשתמש המחובר
            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = db.CartItems
                              .Include(c => c.Book)
                              .Where(c => c.UserId == userId) // סינון לפי המשתמש המחובר
                              .ToList();

            return View(cartItems);
        }
        [HttpPost]
        public ActionResult GetBookPrice(int cartItemId, string transactionType)
        {
            try
            {
                var cartItem = db.CartItems.Include(c => c.Book).FirstOrDefault(c => c.CartItemId == cartItemId);
                if (cartItem == null)
                {
                    return Json(new { success = false, message = "הפריט לא נמצא בעגלה." });
                }

                if (transactionType == "buy")
                {
                    cartItem.TransactionType = "buy";
                }
                else if (transactionType == "borrow")
                {
                    cartItem.TransactionType = "borrow";
                }
                db.SaveChanges();

                var grandTotal = db.CartItems.Where(c => c.UserId == cartItem.UserId)
                                .Sum(c => (c.TransactionType == "buy" ? c.Book.PriceBuy : c.Book.PriceBorrow) * c.Quantity);

                return Json(new
                {
                    success = true,
                    price = (cartItem.TransactionType == "buy" ? cartItem.Book.PriceBuy : cartItem.Book.PriceBorrow).ToString("C"),
                    total = ((cartItem.TransactionType == "buy" ? cartItem.Book.PriceBuy : cartItem.Book.PriceBorrow) * cartItem.Quantity).ToString("C"),
                    grandTotal = grandTotal.ToString("C")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"שגיאה: {ex.Message}" });
            }
        }


        // פונקציה לדוגמה לקבלת מזהה המשתמש המחובר
        private int GetLoggedInUserId()
        {
            var userEmail = User.Identity.Name; // נניח ששם המשתמש הוא ה-Email
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            return user?.UserId ?? 0; // אם המשתמש לא נמצא, נחזיר 0
        }

        ///helloo
        // GET: CartItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            return View(cartItem);
        }

        // GET: CartItems/Create
        public ActionResult Create()
        {
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title");
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name");
            return View();
        }

        // POST: CartItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CartItemId,UserId,BookId,Quantity")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                db.CartItems.Add(cartItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", cartItem.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", cartItem.UserId);
            return View(cartItem);
        }

        // GET: CartItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", cartItem.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", cartItem.UserId);
            return View(cartItem);
        }

        // POST: CartItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CartItemId,UserId,BookId,Quantity")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cartItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", cartItem.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", cartItem.UserId);
            return View(cartItem);
        }
       
        // GET: CartItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            return View(cartItem);
        }

        // POST: CartItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CartItem cartItem = db.CartItems.Find(id);
            db.CartItems.Remove(cartItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Checkout()
        {
            var userId = GetLoggedInUserId(); // בדיקת מזהה המשתמש המחובר
            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = db.CartItems
                              .Include(c => c.Book)
                              .Where(c => c.UserId == userId)
                              .ToList();

            return View(cartItems);
        }

        // הוספת ספר לעגלה
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddToCart(int bookId)
        {
            var userId = GetLoggedInUserId();

            var existingItem = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.BookId == bookId);

            if (existingItem != null)
            {
                return Json(new { success = false, message = "הספר כבר נמצא בעגלת הקניות שלך." });
            }

            var newItem = new CartItem
            {
                UserId = userId,
                BookId = bookId,
                Quantity = 1,
                TransactionType = "buy"
            };

            db.CartItems.Add(newItem);
            db.SaveChanges();

            return Json(new { success = true, message = "הספר נוסף לעגלת הקניות בהצלחה!" });
        }




        [HttpPost]
        public ActionResult EditQuantity(int cartItemId, int delta)
        {
            var cartItem = db.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                // אם הכמות כבר 1 והמשתמש מנסה להגדיל, אין שינוי
                if (delta > 0 && cartItem.Quantity >= 1)
                {
                    return Json(new { success = false, message = "לא ניתן להגדיל את הכמות מעל 1." });
                }

                cartItem.Quantity += delta;

                // מחיקת הפריט אם הכמות קטנה או שווה ל-0
                if (cartItem.Quantity <= 0)
                {
                    db.CartItems.Remove(cartItem);
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "הפריט לא נמצא בעגלה." });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
                db = null; // וודא שהאובייקט לא נגיש עוד לאחר Dispose
            }
            base.Dispose(disposing);
        }


    }
}
