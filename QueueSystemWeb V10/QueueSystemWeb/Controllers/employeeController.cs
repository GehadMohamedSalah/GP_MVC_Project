using QueueSystemWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace QueueSystemWeb.Controllers
{
    public class employeeController : Controller
    {
        QueueSystemDBWeb1 db = new QueueSystemDBWeb1();

        //*********************************************show requests*********************************************//


        public ActionResult index()
        {
            /////////////////
            List<Notification> allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var user = db.user.SingleOrDefault(c => c.id == UserID);
            return View(user);
        }

        [HttpGet]
        public ActionResult Showrequests()
        {
            ViewBag.fullname = Session["username"];
            ViewBag.userid = Session["userid"];
            ViewBag.user_id = new SelectList(db.user, "id");
            var zaft = int.Parse(Session["userid"].ToString());
            var empl = db.user.SingleOrDefault(y => y.id == zaft);

            /////////////////
            List<Notification> allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////

            var reqs = db.Request.Where(c => c.empoyee_id == zaft && c.state == "pending" || c.state == "rejected").ToList();
            ViewBag.reqs = reqs;
            return View();

        }

        //*********************************************show requirements uploaded for some request*********************************************//
        [HttpGet]
        public ActionResult ShowRequirements(int id)
        {
            /////////////////
            List<Notification> allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////

            var reqAtts = db.Request_attachement.Where(c => c.request_id == id).ToList();
            ViewBag.reqAtts = reqAtts;
            ViewBag.emp_id = db.Request.SingleOrDefault(c => c.id == id).empoyee_id;
            ViewBag.req_id = id;
            return View();
        }

        [HttpPost]
        public ActionResult ShowRequirements(int reqId, string notes, string state)
        {
            var req = db.Request.SingleOrDefault(c => c.id == reqId);
            req.state = state;
            req.notes = notes;
            req.timeOfChange = DateTime.Now;
            db.SaveChanges();
            string message = "";
            if (req.state == "rejected")
            {
                message = "لقد تم رفض طلبك";
            }
            else
            {
                message = "لقد تم قبول طلبك";
            }
            Notification noti = new Notification
            {
                msg = message,
                seen = false,
                dateTime = DateTime.Now,
                type_noti = "replyrequest",
                emp_id = req.empoyee_id,
                client_id = req.user_id,
                type_noti_id = req.id
            };
            db.Notification.Add(noti);
            db.SaveChanges();
            var emp_id = db.Request.SingleOrDefault(c => c.id == reqId).empoyee_id;
            return RedirectToAction("Showrequests/" + emp_id);
        }



        //*********************************************reserve offline*********************************************//

        [HttpGet]
        public ActionResult Reserve()
        {
            ViewBag.fullname = Session["username"];
            ViewBag.userid = Session["userid"];
            ViewBag.user_id = new SelectList(db.user, "id");
            var zaft = int.Parse(Session["userid"].ToString());

            /////////////////
            List<Notification> allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////

            //var reqs = db.Request.Where(c => c.empoyee_id == zaft).ToList();
            var v = db.Emp_Org_Services.Where(c => c.emp_id == zaft).ToList();
            var org_id = v[0].org_id;

            ReservationOffline app = new ReservationOffline();
            app.services = db.Services_.Where(c => c.org_id == org_id).ToList();
            return View(app);
        }

        [HttpPost]
        public JsonResult Reserve(ReservationOffline app)
        {
            string dateTime = app.date + "T" + app.time;
            DateTime dt = Convert.ToDateTime(dateTime);
            if (dt < DateTime.Now)
            {
                return Json(new { result = 0 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var zaft = int.Parse(Session["userid"].ToString());
                var reqs = db.Emp_Org_Services.Where(c => c.emp_id == zaft).ToList();
                var branch_id = reqs[0].branch_id;
                var user = new user { name = app.name, phone = app.phone };
                db.user.Add(user);
                db.SaveChanges();

                var appointment = new Appointments_
                {
                    state = "pending",
                    time = dt,
                    user_id = user.id,
                    service_id = app.service_id,
                    branch_id = branch_id
                };
                db.Appointments_.Add(appointment);
                db.SaveChanges();
                return Json(new { result = 1 }, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult ShowTime(string date, int service_id)
        {
            var zaft = int.Parse(Session["userid"].ToString());
            var emp = db.user.SingleOrDefault(y => y.id == zaft);
            var reqs = db.Emp_Org_Services.Where(c => c.emp_id == emp.id).ToList();
            var branch_id = reqs[0].branch_id;
            Services_ service = db.Services_.SingleOrDefault(c => c.id == service_id);
            TimeSpan workPeriod = (TimeSpan)service.endtime - (TimeSpan)service.starttime;
            int workPeriodMinutes = (int)workPeriod.TotalMinutes;
            TimeSpan period = (TimeSpan)service.period;
            int servicePeriodMinutes = (int)period.TotalMinutes;
            int numRoles = workPeriodMinutes / servicePeriodMinutes;
            List<TimeSpan> times = new List<TimeSpan>();
            TimeSpan start = (TimeSpan)service.starttime;
            TimeSpan breakStart = (TimeSpan)service.breakStart;
            TimeSpan breakPeriod = (TimeSpan)service.breakPeriod;
            int breakPeriodMinutes = (int)breakPeriod.TotalMinutes;
            int breakRoles = breakPeriodMinutes / servicePeriodMinutes;
            TimeSpan breakEnd = breakStart + breakPeriod;

            for (int i = 0; i <= numRoles - breakRoles; i++)
            {
                if (start == breakStart)
                {
                    start = breakEnd;
                }
                else
                {
                    times.Add(start);
                    start = start + period;
                }

            }



            var service_emps = db.Emp_Org_Services.Where(c => c.service_id == service_id && c.branch_id == branch_id).ToList();

            int emps = service_emps.Count();
            List<TimeSpan> allTimes = new List<TimeSpan>();
            for (int i = 0; i < emps; i++)
            {
                foreach (var x in times)
                {
                    allTimes.Add(x);
                }

            }

            var appointments = db.Appointments_.Where(x => x.service_id == service_id && x.branch_id == branch_id).ToList();
            foreach (var x in appointments)
            {
                string dt = ((DateTime)x.time).ToString("yyyy-MM-dd");
                if (date.Equals(dt))
                {
                    string t = ((DateTime)x.time).ToString("H:mm:ss");
                    TimeSpan ts = TimeSpan.Parse(t);
                    allTimes.Remove(ts);
                }
            }
            List<string> AllTimes = new List<string>();
            foreach (var a in allTimes)
            {
                AllTimes.Add(a.ToString());
            }
            return Json(AllTimes, JsonRequestBehavior.AllowGet);
        }

        public List<string> ShowTime1(string date, int service_id)
        {
            var zaft = int.Parse(Session["userid"].ToString());
            var emp = db.user.SingleOrDefault(y => y.id == zaft);
            var reqs = db.Emp_Org_Services.Where(c => c.emp_id == emp.id).ToList();
            var branch_id = reqs[0].branch_id;
            Services_ service = db.Services_.SingleOrDefault(c => c.id == service_id);
            TimeSpan workPeriod = (TimeSpan)service.endtime - (TimeSpan)service.starttime;
            int workPeriodMinutes = (int)workPeriod.TotalMinutes;
            TimeSpan period = (TimeSpan)service.period;
            int servicePeriodMinutes = (int)period.TotalMinutes;
            int numRoles = workPeriodMinutes / servicePeriodMinutes;
            List<TimeSpan> times = new List<TimeSpan>();
            TimeSpan start = (TimeSpan)service.starttime;
            TimeSpan breakStart = (TimeSpan)service.breakStart;
            TimeSpan breakPeriod = (TimeSpan)service.breakPeriod;
            int breakPeriodMinutes = (int)breakPeriod.TotalMinutes;
            int breakRoles = breakPeriodMinutes / servicePeriodMinutes;
            TimeSpan breakEnd = breakStart + breakPeriod;

            for (int i = 0; i <= numRoles - breakRoles; i++)
            {
                if (start == breakStart)
                {
                    start = breakEnd;
                }
                else
                {
                    times.Add(start);
                    start = start + period;
                }

            }



            var service_emps = db.Emp_Org_Services.Where(c => c.service_id == service_id && c.branch_id == branch_id).ToList();

            int emps = service_emps.Count();
            List<TimeSpan> allTimes = new List<TimeSpan>();
            for (int i = 0; i < emps; i++)
            {
                foreach (var x in times)
                {
                    allTimes.Add(x);
                }

            }

            var appointments = db.Appointments_.Where(x => x.service_id == service_id && x.branch_id == branch_id).ToList();
            foreach (var x in appointments)
            {
                string dt = ((DateTime)x.time).ToString("yyyy-MM-dd");
                if (date.Equals(dt))
                {
                    string t = ((DateTime)x.time).ToString("H:mm:ss");
                    TimeSpan ts = TimeSpan.Parse(t);
                    allTimes.Remove(ts);
                }
            }
            List<string> AllTimes = new List<string>();
            foreach (var a in allTimes)
            {
                AllTimes.Add(a.ToString());
            }
            return AllTimes;
        }


        //*************************************** get all unavailabe dates for service***************************************//
        [HttpGet]
        public ActionResult GetUnAvailableDates(int service_id)
        {
            var zaft = int.Parse(Session["userid"].ToString());
            var emp = db.user.SingleOrDefault(y => y.id == zaft);
            var reqs = db.Emp_Org_Services.Where(c => c.emp_id == emp.id).ToList();
            var branch_id = reqs[0].branch_id;
            List<string> dates = new List<string>();
            var apps = db.Appointments_.Where(c => c.branch_id == branch_id && c.service_id == service_id).ToList();
            foreach (var x in apps)
            {
                string date = ((DateTime)x.time).ToString("yyyy-MM-dd");
                dates.Add(date);
            }
            List<string> datesDistinct = dates.Distinct().ToList();
            List<string> unAvailableDates = new List<string>();
            List<string> availableDates = new List<string>();
            foreach (var x in datesDistinct)
            {
                var allTimes = ShowTime1(x, service_id);
                if (allTimes.Count() == 0)
                {
                    unAvailableDates.Add(x);
                }
                else
                {
                    availableDates.Add(x);
                }
            }

            DateTime startDate = DateTime.Now;
            DateTime endDate = new DateTime(2030, 1, 1);

            TimeSpan diff = endDate - startDate;
            int days = diff.Days;
            for (var i = 0; i <= days; i++)
            {
                var testDate = startDate.AddDays(i);
                switch (testDate.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Friday:
                        unAvailableDates.Add(testDate.ToString("yyyy-MM-dd"));
                        break;
                }
            }
            for(int c = 2019; c < 2030; c++)
            {
                unAvailableDates.Add(c+"-01-25");
                unAvailableDates.Add(c + "-06-30");
            }
            

            return Json(unAvailableDates, JsonRequestBehavior.AllowGet);

        }


        //***************************************get appointments for service*************************************************//
        public List<Appointments_> GetApps(int service_id)
        {
            var allApps = db.Appointments_.Where(c => c.service_id == service_id && c.state.Equals("pending")).ToList();
            List<Appointments_> apps = new List<Appointments_>();
            foreach (var x in allApps)
            {
                var date = ((DateTime)x.time).ToString("MM/dd/yyyy");
                var dateNow = DateTime.Now.ToString("MM/dd/yyyy");
                if (date.Equals(dateNow))
                {
                    apps.Add(x);
                }
            }
            return apps;
        }


        //****************************************************check attend*********************************************************
        [HttpGet]
        public ActionResult CheckAttend()
        {

            ViewBag.fullname = Session["username"];
            ViewBag.userid = Session["userid"];
            ViewBag.user_id = new SelectList(db.user, "id");
            var zaft = int.Parse(Session["userid"].ToString());
            var emp_service = db.Emp_Org_Services.Where(c => c.emp_id == zaft).ToList();

            /////////////////
            List<Notification> allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////

            List<int> service_ids = new List<int>();
            List<string> service_names = new List<string>();
            List<List<Appointments_>> service_apps = new List<List<Appointments_>>();
            foreach (var x in emp_service)
            {
                service_ids.Add((int)x.service_id);
                service_names.Add(x.Services_.name);
                var apps = GetApps((int)x.service_id);
                var appss = apps.OrderBy(c => ((DateTime)c.time).TimeOfDay).ToList();
                service_apps.Add(appss);
            }
            ViewBag.service_apps = service_apps;
            ViewBag.service_ids = service_ids;
            ViewBag.service_names = service_names;

            return View();
        }

        [HttpPost]
        public ActionResult CheckAttend(Attendence att)
        {

            var user = db.user.SingleOrDefault(c => c.id == att.user_id);
            Appointments_ app = db.Appointments_.SingleOrDefault(c => c.id == att.app_id);
            if (user.count >= 3)
            {
                user.block = true;
                app.state = "blocked";
            }
            else
            {
                if (att.check.Equals("0"))
                {
                    app.state = "unatt";
                    user.count = user.count + 1;
                }
                else if (att.check.Equals("1"))
                {
                    app.state = "att";
                }
            }

            db.SaveChanges();

            return RedirectToAction("CheckAttend");
        }


        //*****************************************************show blocked people*****************************************************//
        public ActionResult showblock()
        {
            /////////////////
            List<Notification> allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////
            ViewBag.fullname = Session["username"];
            ViewBag.userid = Session["userid"];
            ViewBag.user_id = new SelectList(db.user, "id");
            var zaft = int.Parse(Session["userid"].ToString());
            var emp_branch = db.Emp_Org_Services.Where(c => c.emp_id == zaft).ToList()[0].branch_id;
            var allblockedusers = db.user.Where(x 
                => x.person_id == 4 && x.block == true).ToList();
            var apps = db.Appointments_.Where(c => c.branch_id == emp_branch);
            List<user> ourblockedusers = new List<user>();
            foreach (user u in allblockedusers)
            {
                foreach (var a in apps)
                {
                    if (u.id == a.user_id && !ourblockedusers.Contains(u))
                    {
                        ourblockedusers.Add(u);
                    }
                }
            }
            ViewBag.users = ourblockedusers;
            ViewBag.numberRow = ourblockedusers.Count();
            return View();
        }


        //****************************************************un block user ****************************************************//
        public ActionResult unblock(int id)
        {
            var blockman = db.user.SingleOrDefault(c => c.id == id);
            blockman.block = false;
            blockman.count = 0;
            db.SaveChanges();
            db.user.Remove(blockman);
            return RedirectToAction("showblock");
        }


        //****************************************************Notification****************************************************//
        public List<Notification> Get_Notification()
        {
            var zaft = int.Parse(Session["userid"].ToString());
            var emp = db.user.SingleOrDefault(y => y.id == zaft);
            List<Notification> allNotis = db.Notification.Where(c => (c.emp_id == emp.id && c.type_noti == "request") || c.type_noti == "complaint" || c.type_noti == "inquire").ToList();
            if (allNotis == null)
            {
                return null;
            }
            List<Notification> allNotis1 = new List<Notification>();
            allNotis1 = allNotis.OrderByDescending(x => x.dateTime).ToList();
            return allNotis1;
        }

        public ActionResult Get_All_Notification()
        {
            /////////////////
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////
            var allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            return View(allNotis);
        }

        public List<Notification> Get_UnSeen_Notification()
        {
            var allNotis = Get_Notification();
            List<Notification> unSeenNotis = new List<Notification>();
            foreach (var x in allNotis)
            {
                if (x.seen == false)
                {
                    unSeenNotis.Add(x);
                }
            }
            return unSeenNotis;
        }

        public ActionResult ShowNotiReferece(int noti_id)
        {
            var noti = db.Notification.SingleOrDefault(c => c.Id == noti_id);
            noti.seen = true;
            db.SaveChanges();
            if (noti.type_noti.Equals("request"))
            {
                return RedirectToAction("ShowRequirements/" + noti.type_noti_id);
            }
            else
            {
                return RedirectToAction("ReplayComplainOrInquire");
            }
        }


        //*********************************************replay any message*********************************************//
        [HttpGet]
        public ActionResult ReplayComplainOrInquire()
        {
            /////////////////
            List<Notification> allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////
            ViewBag.fullname = Session["username"];
            ViewBag.userid = Session["userid"];
            ViewBag.user_id = new SelectList(db.user, "id");
            var zaft = int.Parse(Session["userid"].ToString());
            int branch_id = (int)db.Emp_Org_Services.Where(c => c.emp_id == zaft).ToList()[0].branch_id;
            var mesg = db.Messages.Where(c => c.status == false && c.to == branch_id).ToList();
            return View(mesg);
        }
        [HttpPost]
        public JsonResult ReplayComplainOrInquire(Messages[] complain_inquire)
        {
            ViewBag.fullname = Session["username"];
            ViewBag.userid = Session["userid"];
            ViewBag.user_id = new SelectList(db.user, "id");
            var zaft = int.Parse(Session["userid"].ToString());

            string result = "خطأ! لم يتم ارسال الرسالة";
            if (complain_inquire != null)
            {

                foreach (Messages item in complain_inquire)
                {
                    var m = db.Messages.SingleOrDefault(c => c.Id == item.Id);
                    if (m != null)
                    {
                        m.emp_id = zaft;
                        m.replay = item.replay;
                        m.status = true;
                        var from_mail = db.user.SingleOrDefault(c => c.id == m.from).mail;
                        var org_name = db.Branch.SingleOrDefault(c => c.id == m.to).organization.name;
                        var replay = "";
                        if (m.type == "inquire")
                        {
                            replay = "الرد ع استعلامك : " + " ( " + m.msg + " ) " + "من منظمة " + " : " + org_name + "</br>" + m.replay;

                        }
                        if (m.type == "complaint")
                        {
                            replay = "الرد ع شكوتك : " + " ( " + m.msg + " ) " + "من منظمة " + " : " + org_name + "</br>" + m.replay;

                        }
                        if (m.type == "request")
                        {
                            replay = "الرد ع طلبك : " + " ( " + m.msg + " ) " + "من منظمة " + " : " + org_name + "</br>" + m.replay;

                        }
                        //to send mail
                        var SenderEmail = new MailAddress("queuewebsite@gmail.com", "منظمة الحجز");
                        var SenderPassword = "web_agmm&app_am";
                        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                        client.EnableSsl = true;
                        client.Timeout = 100000;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(SenderEmail.Address, SenderPassword);
                        MailMessage msg = new MailMessage(SenderEmail.Address, from_mail, m.type, replay);
                        msg.IsBodyHtml = true;
                        msg.BodyEncoding = UTF8Encoding.UTF8;
                        client.Send(msg);
                        db.SaveChanges();
                    }
                }
                result = "تم الارسال بنجاح";
            }
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        //*****************************************edit profile for any actor*****************************************//
        [HttpGet]
        public ActionResult EditProfile()
        {
            /////////////////
            List<Notification> allNotis = Get_Notification();
            ViewBag.allNotis = allNotis;
            var countUnseenNotis = Get_UnSeen_Notification().Count();
            ViewBag.countUnseenNotis = countUnseenNotis;
            /////////////////
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var user = db.user.SingleOrDefault(c => c.id == UserID);
            return View(user);
        }

        [HttpPost]
        public ActionResult EditProfile(int id, string name, string phone, string mail, string username, string password)
        {
            var result = 0;
            var users = db.user.Where(c => c.username == username).ToList();
            var u = db.user.SingleOrDefault(c => c.id == id);
            users.Remove(u);
            if (users.Count() != 0)
            {
                result = 0;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var zaft = int.Parse(Session["UserID"].ToString());
                    var user = db.user.SingleOrDefault(c => c.id == id);
                    user.name = name;
                    user.phone = phone;
                    user.mail = mail;
                    user.username = username;
                    user.password = password;
                    db.SaveChanges();
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }

            return Json(new { result, JsonRequestBehavior.AllowGet });
        }
    }
    }