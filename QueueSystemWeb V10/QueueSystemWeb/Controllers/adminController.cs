using CrystalDecisions.CrystalReports.Engine;
using QueueSystemWeb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QueueSystemWeb.Controllers
{
    public class adminController : Controller
    {
        QueueSystemDBWeb1 db = new QueueSystemDBWeb1();
        // GET: admin


        public ActionResult index()
        {
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var user = db.user.SingleOrDefault(c => c.id == UserID);
            return View(user);
        }
        //**********************************************Add service get***********************************************//
        public ActionResult AddServ()
        {
            return View();
        }

        //**********************************************Add service post***********************************************//
        public JsonResult SaveOrder(Services_[] order)
        {
            string result = "خطأ! الطلب لم يكتمل !!";
            if (order != null)
            {
                foreach (Services_ item in order)
                {
                    item.org_id = (int)Session["orggg_id"];
                    int x = 0;
                    db.Services_.Add(item);
                }
                db.SaveChanges();
                result = "تم بنجاح اضافة الخدمات";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //**********************************************Show organization***********************************************//

        public ActionResult showorg()
        {
            var organizations = db.organization.ToList();
            List<organization> organization2 = new List<organization>();
            List<Branch> branches = new List<Branch>();
            if (organizations.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                foreach (organization x in organizations)
                {
                    organization2.Add(x);
                    var b = db.Branch.Where(c => c.organization_id == x.id).ToList();
                    branches.Add(b[0]);
                }
                ViewBag.organization2 = organization2;
                ViewBag.numberRow = organization2.Count();
                ViewBag.branches = branches;
                return View();
            }
        }

        //**********************************************show branches***********************************************//

        public ActionResult showbranch(int id)
        {
            var organization = db.organization.SingleOrDefault(y => y.id == id);
            ViewBag.organization = organization;

            var Branch = db.Branch.ToList();
            List<Branch> Branch2 = new List<Branch>();

            if (Branch.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                foreach (Branch x in Branch)
                {
                    if (x.organization_id == id)
                    {
                        Branch2.Add(x);
                    }
                }

                ViewBag.Branch2 = Branch2;
                ViewBag.numberRow = Branch2.Count();
                return View();
            }
            
        }

        //**********************************************show servise***********************************************//

        public ActionResult showservise(int id)
        {
            var organization = db.organization.SingleOrDefault(y => y.id == id);
            ViewBag.organization = organization;

            var serv = db.Services_.ToList();
            List<Services_> serv2 = new List<Services_>();
            List<List<Requirments_Doc>> all_docs = new List<List<Requirments_Doc>>();
            if (serv.Equals(null))
            {
                return HttpNotFound();
            }
            else
            { 
                foreach (Services_ x in serv)
                {
                    if (x.org_id == id)
                    {
                        serv2.Add(x);
                    }
                }
                foreach(var i in serv2)
                {
                   var docs = db.Requirments_Doc.Where(c => c.Service_id == i.id).ToList();
                    all_docs.Add(docs);
                    docs = null;
                }
                ViewBag.all_docs = all_docs;
                ViewBag.serv2 = serv2;
                ViewBag.numberRow = serv2.Count();
                return View();
            }
        }

        //**********************************************show blocked managers***********************************************//

        //show manger blocked 
        public ActionResult showmanBlocked()
        {
            var users = db.user.Where(x => x.person_id == 2).ToList();
            List<user> user = new List<user>();
            if (users.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                foreach (user x in users)
                {
                    if (x.block == true)
                    {
                        user.Add(x);
                    }
                }
                ViewBag.users = user;
                ViewBag.numberRow = user.Count();
                return View();
            }
        }

        //**********************************************show unblocked managers***********************************************//

        public ActionResult showmanUNBlocked()
        {
            var users = db.user.Where(x => x.person_id == 2).ToList();
            List<user> user = new List<user>();
            if (users.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                foreach (user x in users)
                {
                    if (x.block == false)
                    {
                        user.Add(x);
                    }
                }
                ViewBag.users = user;
                ViewBag.numberRow = user.Count();
                return View();
            }
        }


        //***************************************************** delete organization***********************************************/
        public ActionResult deleteorg(int id)
        {
            var org = db.organization.SingleOrDefault(c => c.id == id);
            var services = db.Services_.Where(c => c.org_id == org.id).ToList();
            foreach(var x in services)
            {
                db.Services_.Remove(x);
            }
            var delorg = db.organization.Remove(org);
            db.SaveChanges();
            return RedirectToAction("showorg");
        }

        //***************************************************** delete branch***********************************************/

        public ActionResult deletebranch(int id)
        {
            
            var branch = db.Branch.SingleOrDefault(c => c.id == id);
            var org_id = db.organization.SingleOrDefault(c => c.id == branch.organization_id).id;
            var delbranch = db.Branch.Remove(branch);
            db.SaveChanges();
            return RedirectToAction("showbranch/"+ org_id);
        }

        //delete service 
        public ActionResult deleteservice(int id)
        {
            var serve = db.Services_.SingleOrDefault(c => c.id == id);
            var org_id = db.organization.SingleOrDefault(c => c.id == serve.org_id).id;
            var delbranch = db.Services_.Remove(serve);
            db.SaveChanges();
            return RedirectToAction("showservise/"+ org_id);
        }

        //*********************************************block manger**************************************************/

        public ActionResult blockmang(int id)
        {
            var blockman = db.user.SingleOrDefault(c => c.id == id);
            blockman.block = true;
            db.SaveChanges();
            db.user.Remove(blockman);
            return RedirectToAction("showmanUNBlocked");
        }

        //*********************************************unblock manger**************************************************/
        
        public ActionResult unblockmang(int id)
        {

            var blockman = db.user.SingleOrDefault(c => c.id == id);
            blockman.block = false;
            db.SaveChanges();
            db.user.Remove(blockman);
            return RedirectToAction("showmanBlocked");
        }


        //*****************************************************add manger "sequence"*******************************************************/

        [HttpGet]
        public ActionResult Addmanger()
        {
            user User = new user();
            return View(User);
        }

        [HttpPost]
        public ActionResult Addmanger(user mang)
        {
            var check = db.user.Where(c => c.username == mang.username).ToList();
            int result = 0;
            if (check.Count() != 0)
            {
                return RedirectToAction("Addmanger");
            }
            else
            {

                mang.person_id = 2;
                mang.count = 0;
                mang.block = false;
                db.user.Add(mang);
                db.SaveChanges();
                Session["man_id"] = mang.id;

                return RedirectToAction("AddOrganazation");
             }

        }

        //*****************************************************add organization"*******************************************************/

        [HttpGet]
        public ActionResult AddOrganazation()
        {
            organization org = new organization();
            return View(org);
        }


        [HttpPost]
        public JsonResult AddOrganazation(organization org)
        {
            var check = db.organization.Where(c => c.name == org.name);
            int result = 0;
            if (check.Count() != 0)
            {
                result = 0;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    org.manger_id = (int)Session["man_id"];
                    db.organization.Add(org);
                    db.SaveChanges();
                    Session["orggg_id"] = org.id;
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }
            return Json(new { result, JsonRequestBehavior.AllowGet});
        }

        //*****************************************************add branches get*******************************************************/

        public ActionResult AddBranch()
        {
            QueueSystemDBWeb1 db = new QueueSystemDBWeb1();
            return View(db.Branch);
        }

        //*****************************************************add branches post*******************************************************/
        public JsonResult AddbranchAdd(Branch[] branch)
        {
            string result = "خطأ! الفروع لما تضاف";
            if (branch != null)
            {

                foreach (Branch item in branch)
                {
                    item.organization_id = (int)Session["orggg_id"];
                    db.Branch.Add(item);
                }
                db.SaveChanges();
                result = "تم بنجاح اضافة الفروع";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //*****************************************************Report for organization*******************************************************/

        public ActionResult ExportReportOrgNumPlusService()
        {
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CrystalReportorg.rpt"));
            var list = db.Services_.ToList();
            rd.SetDataSource(db.Services_.Select(c => new {
                id = c.id,
                serviceName = c.name ?? "h",
                org_name = c.org_id ?? 0

            }).ToList());

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", "organizationservice.pdf");
            }
            catch
            {
                throw;
            }
        }

        //************************************add manger "to edit org"**********************************//
        [HttpGet]
        public ActionResult AddMangerForEditOrg()
        {
            user User = new user();
            return View(User);
        }

        [HttpPost]
        public JsonResult AddMangerForEditOrg(user mang)
        {
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var admin = db.user.SingleOrDefault(c => c.id == UserID);

            var check = db.user.Where(c => c.username == mang.username);
            
                int result = 0;
                if (check.Count() != 0)
                {
                    result = 0;
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        mang.person_id = 2;
                        mang.count = 0;
                        mang.block = true;
                        db.user.Add(mang);
                        db.SaveChanges();
                        Session["man_id"] = mang.id;
                        result = 1;
                    }            
                  else
                    {
                        result = -1;
                    }
            }
            return Json(new { result, JsonRequestBehavior.AllowGet });
        }


        //*********************************edit organization***********************************//
        [HttpGet]
        public ActionResult EditOrganization(int id)
        {
            string n;
            var org = db.organization.Find(id);
            showmangerviewmodel obj = new showmangerviewmodel();
            var all = org.name;
            obj.id = org.id;
            obj.name = org.name;
            obj.manger_id = org.user.name;
            obj.manger = db.user.ToList();

            List<user> allmanger = new List<user>();
            foreach (var x in obj.manger)
            {
                if (x.person_id == 2 && x.id == org.manger_id)
                {
                    allmanger.Add(x);
                     ViewBag.n = x.name;
                }
            }
            return View(obj);
        }

        [HttpPost]
        public JsonResult EditOrganization(int id, string name, string manger_id)
        {

            showmangerviewmodel orgg = new showmangerviewmodel();
            var orgs = db.organization.Where(c => c.name == name).ToList();
            var org = db.organization.SingleOrDefault(c => c.id == id);
            orgs.Remove(org);
            int result = 0;
            if (orgs.Count() != 0)
            {
                result = 0;
            }
            else
            {
                org.name = name;
                orgg.manger = db.user.ToList();
                List<user> allmanger = new List<user>();
                foreach (var x in orgg.manger)
                {
                    if (x.person_id == 2 && manger_id == x.name)
                    {
                        orgg.name = name;
                        org.name = name;
                        org.manger_id = x.id;
                        allmanger.Add(x);
                        db.SaveChanges();
                        result = 1;
                    }
                }
            }
            return Json(new { result, JsonRequestBehavior.AllowGet });

        }


        [HttpGet]
        public ActionResult AdminReport()
        {
            var orgs = db.organization.ToList();
            var allbranches = db.Branch.ToList();
            List <int> count_branches = new List<int>();
            List<List<Branch>> org_branches = new List<List<Branch>>();
            List<Messages> branch_feedback = new List<Messages>();
            List<Appointments_> branch_apps = new List<Appointments_>();
            List<List<Messages>> org_feedback = new List<List<Messages>>();
            List<List<Appointments_>> org_apps = new List<List<Appointments_>>();
            foreach (var x in orgs)
            {
                var branches = db.Branch.Where(c => c.organization_id == x.id).ToList();
                count_branches.Add(branches.Count());
                org_branches.Add(branches);
            }
      
            foreach(var o in orgs)
            {
                foreach(var b in allbranches)
                {
                    if(b.organization_id == o.id)
                    {
                        var bfb = db.Messages.Where(c => c.type == "feedback" && c.to == b.id).ToList();
                        var ba = db.Appointments_.Where(c => c.branch_id == b.id);
                        branch_feedback  = branch_feedback.Concat(bfb).ToList();
                        branch_apps = branch_apps.Concat(ba).ToList();
                    }
                    
                }
                org_feedback.Add(branch_feedback);
                org_apps.Add(branch_apps);
                branch_feedback = new List<Messages>();
                branch_apps = new List<Appointments_>();
            }

            ViewBag.orgs = orgs;
            ViewBag.count_branches = count_branches;
            ViewBag.org_branches = org_branches;
            ViewBag.org_feedback = org_feedback;
            ViewBag.org_apps = org_apps;
            ViewBag.allClients = db.user.Where(c => c.person_id == 4).ToList().Count();
            ViewBag.blockedClients = db.user.Where(c => c.person_id == 4 && c.block == true).ToList().Count();
            ViewBag.emps = db.user.Where(c => c.person_id == 3).ToList().Count();
            return View();
        }

        //*****************************************edit profile for any actor*****************************************//
        [HttpGet]
        public ActionResult EditProfile()
        {
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var user = db.user.SingleOrDefault(c => c.id == UserID);
            return View(user);
        }

        [HttpPost]
        public JsonResult EditProfile(int id, string name, string phone, string mail, string username, string password)
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