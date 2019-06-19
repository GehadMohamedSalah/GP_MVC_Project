using QueueSystemWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace QueueSystemWeb.Controllers
{
    public class HomeController : Controller
    {
        QueueSystemDBWeb1 db = new QueueSystemDBWeb1();
 
        

      

        public ActionResult Login()
        {
            return View();
        }
       
        //************************************************login function************************************************************/
        //login
        [HttpPost]
        public JsonResult Login(user users)
        {
            int result = 0;
            var user = db.user.SingleOrDefault(c => c.username.Equals(users.username) && c.password.Equals(users.password));
            if(user == null)
            {
                //user doesn't exist
                result = 0;  
            }
            else
            {
                if(user.block == false)
                {
                    //login
                    Session["UserID"] = user.id.ToString();
                    Session["UserName"] = user.name.ToString();
                    result = 1;
                }
                else
                {
                    //user blocked
                    result = -1;
                }
            }
            

            return Json(new { result, JsonRequestBehavior.AllowGet });

            

        }

        //************************************************profile function************************************************************/
        //profile
        public ActionResult profile()
        {

             ViewBag.fullname = Session["UserName"];
             ViewBag.userid = Session["UserID"];
             ViewBag.user_id = new SelectList(db.user, "Id");
             int UserID = Convert.ToInt32(Session["UserID"]);
             var user = db.user.SingleOrDefault(c => c.id == UserID);
             if (user.person_id==1)
            {
                return RedirectToAction("index", "admin");
            }
            else if (user.person_id == 2)
            {
                return RedirectToAction("index", "manager");
            }
            else if (user.person_id == 3)
            {
                return RedirectToAction("index", "employee");
            }
            else 
            {
                return RedirectToAction("index", "Home");
            }
        }

        //***********************************************************logout************************************************************/
        public ActionResult Logout()
        {
            HttpContext.Session.RemoveAll();//This will remove all session from application
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }

        ////*****************************************edit profile for any actor*****************************************//
        //[HttpGet]
        //public ActionResult EditProfile()
        //{
        //    ViewBag.fullname = Session["UserName"];
        //    ViewBag.userid = Session["UserID"];
        //    ViewBag.user_id = new SelectList(db.user, "Id");
        //    int UserID = Convert.ToInt32(Session["UserID"]);
        //    var user = db.user.SingleOrDefault(c => c.id == UserID);
        //    return View(user);
        //}

        //[HttpPost]
        //public ActionResult EditProfile(int id, string name, string phone, string mail, string username, string password)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var zaft = int.Parse(Session["UserID"].ToString());
        //        var user = db.user.SingleOrDefault(c => c.id == id);
        //        user.name = name;
        //        user.phone = phone;
        //        user.mail = mail;
        //        user.username = username;
        //        user.password = password;
        //        db.SaveChanges();
        //        return RedirectToAction("showorg");
        //    }
        //    return View();
        //}

        //*****************************************forget password *****************************************//
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {
            //Verify Email ID
            //Generate Reset password link 
            //Send Email 
            string message = "";
            bool status = false;

            using (QueueSystemDBWeb1 dc = new QueueSystemDBWeb1())
            {
                var account = dc.user.Where(a => a.mail == Email).FirstOrDefault();
                if (account != null)
                {
                    //Send email for reset password
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLinkEmail(account.mail, resetCode, "ResetPassword");
                    account.username = resetCode;
                    Session["code"] = resetCode;
                    //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 
                    //in our model class in part 1
                    dc.Configuration.ValidateOnSaveEnabled = false;
                    dc.SaveChanges();
                    message = "رابط استرجاع الرقم السري تم ارساله للبريد الخاص بك";
                }
                else
                {
                    message = "الحساب غير موجود";
                }
            }
            ViewBag.Message = message;
            return RedirectToAction("ResetPassword");
        }



        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var code = activationCode;
            var verifyUrl = "/Home/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("", "Aya saad");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = ""; // Replace with actual password

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "تم انشاء الحساب بنجاح";
                body = "<br/><br/>تم انشاء الحساب بنجاح" +
                    " . من فضلك اضغط على الرابط الاسفل لتأكيد الحساب" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi,<br/>br/>لقد تلقينا طلبًا لإعادة تعيين كلمة مرور حسابك. يرجى النقر على الرابط أدناه لإعادة تعيين كلمة المرور الخاصة بك" +
                    "<br/><br/><a href=" + link + ">Reset Password link</a>" +
                    "<br/><a href=" + code + ">" + code + "</a>";

            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            //var user = db.user.Where(a => a.username == (string)Session["code"]).ToList();
            string id = (string)Session["code"];

            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (QueueSystemDBWeb1 dc = new QueueSystemDBWeb1())
            {
                var user = dc.user.Where(a => a.username == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (QueueSystemDBWeb1 dc = new QueueSystemDBWeb1())
                {
                    var user = dc.user.Where(a => a.username == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.password = model.NewPassword;
                        user.username = "";
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();
                        message = "تم تغيير الرقم السري بنجاح";
                    }
                }
            }
            else
            {
                message = "خطأ";
            }
            ViewBag.Message = message;
            return View(model);
        }
}

}