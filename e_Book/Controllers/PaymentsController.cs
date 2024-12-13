using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using eBookLibrary.Models;
using e_Book.Models;
using System.Net.Mail; // עבור שליחת מיילים
using System.Text; // אם תשתמש ב-StringBuilder למייל
using System.Collections.Generic; // אם תעבוד עם רשימות
using System.Data.Entity; // עבור LINQ ב-Entity Framework

namespace e_Book.Controllers
{
    public class PaymentController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();
        private bool IsCreditCardValid(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || !cardNumber.All(char.IsDigit))
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(cardNumber[i].ToString());
                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                        n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return (sum % 10 == 0);
        }
        private string GetUserEmailById(int userId)
        {
            using (var db = new LibraryDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == userId);
                return user?.Email ?? throw new Exception("המשתמש לא נמצא.");
            }
        }
        private void AddCartItemsToUserLibrary(int userId)
        {
            using (var db = new LibraryDbContext())
            {
                // שליפת הפריטים מהעגלה של המשתמש
                var cartItems = db.CartItems.Where(c => c.UserId == userId).ToList();

                foreach (var item in cartItems)
                {
                    // הוספת הפריטים לטבלת Borrows
                    db.Borrows.Add(new Borrow
                    {
                        UserId = userId,
                        BookId = item.BookId,
                        BorrowDate = DateTime.Now,
                        DueDate = item.TransactionType == "borrow"
                                  ? (DateTime?)DateTime.Now.AddDays(30) // המרה מפורשת ל- DateTime?
                                  : null, // אין תאריך החזרה בקנייה
                        IsReturned = false, // האם הספר הוחזר (תמיד false בקנייה)
                        TransactionType = item.TransactionType // קנייה או השאלה
                    });
                }

                // מחיקת כל הפריטים מהעגלה לאחר ההוספה לספרייה
                db.CartItems.RemoveRange(cartItems);
                db.SaveChanges();
            }
        }


        private void SendEmailConfirmation(string email, decimal totalAmount)
        {
            try
            {
                var fromAddress = new MailAddress("your-email@example.com", "Library Service");
                var toAddress = new MailAddress(email);
                const string fromPassword = "your-email-password";
                string subject = "אישור תשלום";
                string body = $"שלום,\n\nתשלום בסך {totalAmount:C} התקבל בהצלחה.\n\nתודה שבחרת בנו!";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // כאן ניתן להוסיף טיפול בשגיאות אם המייל לא נשלח
                throw new Exception("שליחת המייל נכשלה: " + ex.Message);
            }
        }

        // עיבוד תשלום בכרטיס אשראי
        [HttpPost]
        public ActionResult ProcessPayment(string paymentMethod, string cardNumber, decimal totalAmount)
        {
            try
            {
                // שלב 1: בדיקת כרטיס אשראי
                if (paymentMethod == "creditCard" && !IsCreditCardValid(cardNumber))
                {
                    TempData["Error"] = "מספר כרטיס האשראי אינו חוקי.";
                    return RedirectToAction("Checkout", "CartItems");
                }

                // שלב 2: שליפת מייל המשתמש
                var userId = GetLoggedInUserId();
                var email = GetUserEmailById(userId);

                // שלב 3: הוספת פריטים לספריית המשתמש
                AddCartItemsToUserLibrary(userId);

                // שלב 4: שליחת מייל אישור
                SendEmailConfirmation(email, totalAmount);

                TempData["Success"] = "התשלום בוצע בהצלחה! אישור נשלח למייל.";
                return RedirectToAction("Index", "CartItems");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"שגיאה בתהליך התשלום: {ex.Message}";
                return RedirectToAction("Checkout", "CartItems");
            }
        }

        public ActionResult RedirectToPaypal(decimal totalAmount)
        {
            // הגדרת URL ל-PayPal עם פרטי תשלום
            var paypalUrl = $"https://www.paypal.com/cgi-bin/webscr?cmd=_xclick&business=your-email@example.com&amount={totalAmount}&currency_code=ILS&item_name=Payment+for+Cart";
            return Redirect(paypalUrl);
        }

        // הפניה לדף PayPal
        [HttpPost]
        public ActionResult RedirectToPaypal()
        {
            // הפניה לדף PayPal
            return Redirect("https://www.paypal.com/signin");
        }

        // דף הצלחה
        public ActionResult Success()
        {
            ViewBag.Message = "תשלום התקבל בהצלחה!";
            return View();
        }
        private int GetLoggedInUserId()
        {
            var userEmail = User.Identity.Name; // נניח ששם המשתמש הוא ה-Email
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            return user?.UserId ?? 0; // אם המשתמש לא נמצא, נחזיר 0
        }
        public ActionResult Checkout()
        {
            var userId = GetLoggedInUserId(); // ודא שהמשתמש מחובר
            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Login", "Account");
            }

            // שליפת פרטי העגלה של המשתמש
            var cartItems = db.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();
            return View(cartItems);
        }

        // דף כישלון
        public ActionResult Failure()
        {
            ViewBag.Message = "אירעה שגיאה בעיבוד התשלום.";
            return View();
        }
    }
}
