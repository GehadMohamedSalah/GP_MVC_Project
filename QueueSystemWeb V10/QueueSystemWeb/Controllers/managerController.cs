using CrystalDecisions.CrystalReports.Engine;
using QueueSystemWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QueueSystemWeb.Controllers
{
    public class managerController : Controller
    {
        QueueSystemDBWeb1 db = new QueueSystemDBWeb1();
        public ActionResult index()
        {
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var user = db.user.SingleOrDefault(c => c.id == UserID);
            return View(user);
        }

        // GET: manger

        //*********************************************show all employees************************************************************/

        public ActionResult showemp()
        {
            var emp = db.user.Where(x => x.person_id == 3).ToList();
            List<user> user = new List<user>();
            if (emp.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                foreach (user x in emp)
                {
                    user.Add(x);
                }

                ViewBag.emp = emp;
                ViewBag.numberRow = emp.Count();
                return View();
            }
        }

        //*******************************************show organization*******************************************//
        public ActionResult showorg()
        {
            int UserID = Convert.ToInt32(Session["UserID"]);
            var manager = db.user.SingleOrDefault(c => c.id == UserID);
            var org_id = db.organization.SingleOrDefault(c => c.manger_id == manager.id).id;
            var organization = db.organization.SingleOrDefault(c => c.id == org_id);
            if (organization.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.organization = organization;
                return View();
            }
        }



        //*****************************************************delete employee**********************************************/
 
        public ActionResult deleteuser(int id)
        {

            var user = db.user.SingleOrDefault(c => c.id == id);
            var deluser = db.user.Remove(user);
            db.SaveChanges();
            return RedirectToAction("showemp");
        }
        //*************************************************delete services*************************************************//
        public ActionResult deleteserv(int id)
        {
            var serv = db.Services_.SingleOrDefault(c => c.id == id);
            var org_id = db.organization.SingleOrDefault(c => c.id == serv.org_id).id;
            var delserv = db.Services_.Remove(serv);
            db.SaveChanges();
            return RedirectToAction("showservise/"+ org_id);
        }
        //*********************************************edit organization****************************************************************/

        [HttpGet]
        public ActionResult Editorg(int id)
        {
            var org = db.organization.Find(id);
            return View(org);
        }

        [HttpPost]
        public JsonResult Editorg(int id, string name)
        {
            var result = 0;
            var orgs = db.organization.Where(c => c.name == name).ToList();
            var org = db.organization.SingleOrDefault(c => c.id == id);
            orgs.Remove(org);
            if (orgs.Count() != 0)
            {
                result = 0;
            }
            else
            {
                org.name = name;
                db.SaveChanges();
                result = 1;
            }

            return Json(new { result, JsonRequestBehavior.AllowGet });
        }
        //*********************************************edit branch*********************************************//
        [HttpGet]
        public ActionResult Editbranch(int id)
        {
            var branch = db.Branch.Find(id);
            return View(branch);
        }

        [HttpPost]
        public ActionResult Editbranch(int id, string branch_location , string name)
        {

            var branch = db.Branch.SingleOrDefault(c => c.id == id);
            branch.branch_location = branch_location;
            branch.name = name;
            db.SaveChanges();
            return RedirectToAction("showbranch/"+branch.organization_id);
        }

        //*********************************************edit service*********************************************//
        [HttpGet]
        public ActionResult Editservice(int id)
        {
            var service = db.Services_.Find(id);
            return View(service);
        }

        [HttpPost]
        public JsonResult Editservice(int id, string name, TimeSpan starttime, TimeSpan endtime, TimeSpan breakStart, TimeSpan breakPeriod, TimeSpan period, int toFinish, int toExpire)
        {
            var services = db.Services_.Where(c => c.name == name).ToList();
            var serve = db.Services_.SingleOrDefault(c => c.id == id);
            services.Remove(serve);
            int result = 0;
            if (services.Count() != 0)
            {
                result = 0;
            }
            else
            {
                serve.starttime = starttime;
                serve.endtime = endtime;
                serve.period = period;
                serve.name = name;
                serve.breakStart = breakStart;
                serve.breakPeriod = breakPeriod;
                serve.toExpire = toExpire;
                serve.toFinish = toFinish;
                db.SaveChanges();
                result = 1;
            }


            return Json(new { result, JsonRequestBehavior.AllowGet });
        }
        //************************************************add employee************************************************************/
        //
        [HttpGet]
        public ActionResult Addemp()
        {           
            user User = new user();
            return View(User);
        }

        [HttpPost]
        public JsonResult Addemp(user emp)
        {
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var manager = db.user.SingleOrDefault(c => c.id == UserID);
            var check = db.user.Where(c => c.username == emp.username);

            int result = 0;

            if (check.Count() != 0)
            {
                result = 0;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    emp.person_id = 3;
                    emp.count = 0;
                    emp.block = false;
                    db.user.Add(emp);
                    db.SaveChanges();
                    var org_id = db.organization.SingleOrDefault(c => c.manger_id == manager.id).id;
                    var emp_service = new Emp_Org_Services
                    {
                        emp_id = emp.id,
                        org_id = org_id
                    };
                    db.Emp_Org_Services.Add(emp_service);
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


        //************************************************AssignEmpToServices************************************************************//
        [HttpGet]
        public ActionResult AssignEmpToServices()
        {
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var manager = db.user.SingleOrDefault(c => c.id == UserID);
            int org_id = db.organization.SingleOrDefault(c => c.manger_id == manager.id).id;
            var emps = db.Emp_Org_Services.Where(c => c.org_id == org_id).ToList();
            var branches = db.Branch.Where(c => c.organization_id == org_id).ToList();
            var services = db.Services_.Where(c => c.org_id == org_id).ToList();
            EmpsOrg EmpsOrg = new EmpsOrg();
            List<user> employees = new List<user>();
            foreach(var x in emps)
            {
                var e = db.user.SingleOrDefault(c => c.id == x.emp_id);
                var a= e.name;
                if (!employees.Contains(x.user))
                {
                    employees.Add(e);
                }
                
            }
            EmpsOrg.emps = employees;
            EmpsOrg.branches = branches;
            EmpsOrg.services = services;
            return View(EmpsOrg);
        }

        [HttpPost]
        public ActionResult AssignEmpToServices(EmpsOrg EmpsOrg)
        {
            string result = "";
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var manager = db.user.SingleOrDefault(c => c.id == UserID);
            var org_id = db.organization.SingleOrDefault(c => c.manger_id == manager.id).id;
            var e = db.Emp_Org_Services.SingleOrDefault(c => c.emp_id == EmpsOrg.emp_id && c.branch_id == null && c.service_id == null);
            var e2 = db.Emp_Org_Services.Where(c => c.emp_id == EmpsOrg.emp_id).ToList();
            var check = 0;
            foreach (var x in e2)
            {
                if (EmpsOrg.service_id == x.service_id)
                {
                    check = 1;
                }
            }
            if (e != null)
            {
                db.Emp_Org_Services.Remove(e);
                db.SaveChanges();
            }

            if (EmpsOrg.branch_id != e2[0].branch_id && e2[0].branch_id != null)
            {
                result = "هذا الموظف بالفعل مضاف لفرع لا يمكن اضافته الى فرع اخر...";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if (check != 0)
            {
                result = "هذا الموظف مضاف بالفعل الى هذه الخدمة " +
                    db.Services_.SingleOrDefault(c => c.id == EmpsOrg.service_id).name;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var empOrgServices = new Emp_Org_Services
                {
                    emp_id = EmpsOrg.emp_id,
                    service_id = EmpsOrg.service_id,
                    branch_id = EmpsOrg.branch_id,
                    org_id = org_id
                };
                db.Emp_Org_Services.Add(empOrgServices);
                db.SaveChanges();
                result = "تم بنجاح ...";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }


        //************************************************show services************************************************************/
        public ActionResult showservise(int id)
        {
            //Doc_Req ServePlusDoc = new Doc_Req();

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
                        ViewBag.servorg = x.org_id;
                        ViewBag.serv = x.id;

                    }
                    Session["service"] = ViewBag.serv;
                }

                //foreach(var i in serv2)
                //{
                //    var docs = db.Requirments_Doc.Where(c => c.Service_id == i.id).ToList();
                //    all_docs.Add(docs);
                //    all_docs = null;
                //}
                Session["or"] = ViewBag.servorg;
                ViewBag.serv2 = serv2;

                ViewBag.numberRow = serv2.Count();
                //ViewBag.all_docs = all_docs;
                //for doc
                ViewBag.docment = new SelectList(db.Doc, "id", "name");
                return View(serv2);
            }
            
        }

        //*****************************new service*****************************************.//
        public JsonResult showservise2(Services_[] order)
        {
            List<Services_> serv = new List<Services_>();
            int check = 0;
            string result = "خطأ حاول مرة اخرى";
            if (order != null)
            {
                foreach (Services_ item in order)
                {
                    serv = db.Services_.ToList();
                    foreach (Services_ ser in serv)
                    {
                        if (ser.name == item.name && ser.org_id == item.org_id)
                        {
                            check = -1;
                            break;
                        }
                                     
                        else 
                        {
                            item.org_id = (int)Session["or"];
                            db.Services_.Add(item);
                            check = 1;                         
                        }
                    }                   
                }
                if (check == -1)
                {
                    result = "الخدمه مضافة لهذه المنظمه بالفعل";
                }
                else if (check == 1)
                {
                    db.SaveChanges();
                    result = "تم اضافة الخدمه";
                }
                
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //*****************************add new doc*****************************************.//
        public JsonResult AddDocument(Doc_Req [] documents)
        {
            List<Doc> docs = new List<Doc>();
            string result = "خطأ حاول مرة اخرى";
            if (documents != null)
            {           
                foreach (Doc_Req item in documents)
                {
                    if (item.namedoc == null)
                    {
                        Requirments_Doc req_doc = new Requirments_Doc
                        {
                            notes = item.notes,
                            Service_id = item.serviceID,
                            doc_id = item.id
                        };
                        db.Requirments_Doc.Add(req_doc);
                        db.SaveChanges();
                        result = "تم اضافةالملفات الموجودة بنجاح";
                    }
                    else if(item.id == 0)
                    {
                        docs = db.Doc.Where(c => c.name == item.namedoc).ToList();
                        if (docs.Count()!=0)
                        {
                            result = "هذا الملف مضاف بالفعل ";
                        }
                        else
                        {
                            Doc doc = new Doc
                            {
                                name = item.namedoc
                            };
                            db.Doc.Add(doc);
                            db.SaveChanges();

                            var AllDoc = db.Doc.SingleOrDefault(x => x.name == item.namedoc).id;
                            Requirments_Doc docum = new Requirments_Doc
                            {
                                notes = item.notes,
                                Service_id = item.serviceID,
                                doc_id = AllDoc
                            };
                            db.Requirments_Doc.Add(docum);
                            db.SaveChanges();
                            result = "تم اضافة الملفات بنجاح";
                        }
                    }                       
                } 
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //**************************************************show branch**************************************************
        public ActionResult showbranch(int id )
        {
            var organization = db.organization.SingleOrDefault(y => y.id == id);
            ViewBag.organization = organization;
            var branch = db.Branch.ToList();
            List<Branch> branch2 = new List<Branch>();
            if (branch.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                foreach (Branch x in branch)
                {
                    if (x.organization_id == id)
                    {
                        branch2.Add(x);
                        ViewBag.servorg2 = x.organization_id;
                    }
                }
                Session["or2"] = ViewBag.servorg2;
                ViewBag.serv2 = branch2;
                ViewBag.numberRow = branch2.Count();

                return View(branch2);
            }
            
        }

        //**************************************************new branch**************************************************//
        public JsonResult showbranch2(Branch[] branch)
        {
            string result = "خطأ! لم يتم اضافة الفرع";
            if (branch != null)
            {

                foreach (Branch item in branch)
                {
                    item.organization_id = (int)Session["or2"];
                    db.Branch.Add(item);
                }
                db.SaveChanges();
                result = "تم اضافة الفرع بنجاح";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //*************************************************delete branch*************************************************//
        public ActionResult deletebranch(int id)
        {

            var branch = db.Branch.SingleOrDefault(c => c.id == id);
            var org_id = db.organization.SingleOrDefault(c => c.id == branch.organization_id).id;
            var msgs = db.Messages.Where(c => c.to == branch.id).ToList();
            foreach(var x in msgs)
            {
                db.Messages.Remove(x);
            }
            
            var delserv = db.Branch.Remove(branch);
            db.SaveChanges();
            return RedirectToAction("showbranch/"+org_id);
        }

        [HttpGet]
        public ActionResult ManagerReport()
        {
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var org = db.organization.SingleOrDefault(c => c.manger_id == UserID);
            var emp_org_services = db.Emp_Org_Services.Where(c => c.org_id == org.id).ToList();
            List<int> emp_ids = new List<int>();
            foreach (var x in emp_org_services)
            {
                if (x.branch_id != null && x.service_id != null)
                {
                    emp_ids.Add((int)x.emp_id);
                }

            }
            var emp_idss = emp_ids.Distinct().ToList();
            List<user> emps = new List<user>();
            List<string> emp_branchs = new List<string>();
            List<List<Services_>> emp_services = new List<List<Services_>>();
            List<int> allreq = new List<int>();
            List<int> pendingreq = new List<int>();
            List<int> repliedmsg = new List<int>();
            int numofReqs = 0;
            int numofPendReqs = 0;
            int complaints = 0;
            foreach (var x in emp_idss)
            {
                var emp = db.user.SingleOrDefault(c => c.id == x);
                emps.Add(emp);

                var emp_branch = db.Emp_Org_Services.Where(c => c.emp_id == x).ToList();
                emp_branchs.Add(emp_branch[0].Branch.branch_location);

                var services = Emp_Services(x);
                emp_services.Add(services);

                var reqs = Emp_Requests(x);
                allreq.Add(reqs);
                numofReqs = numofReqs + reqs;

                var reqs1 = Pending_Requests(x);
                pendingreq.Add(reqs1);
                numofPendReqs = numofPendReqs + reqs1;

                var msgs = Replied_Messages(x);
                repliedmsg.Add(msgs);
            }

            var branches = db.Branch.Where(c => c.organization_id == org.id).ToList();
            List<Appointments_> apps = new List<Appointments_>();
            var allapps = db.Appointments_.ToList();
            var allComplaints = db.Messages.Where(c => c.type == "complaint").ToList();
            foreach (var x in branches)
            {
                foreach (var y in allapps)
                {
                    if (y.branch_id == x.id)
                    {
                        apps.Add(y);
                    }
                }

                foreach (var c in allComplaints)
                {
                    if (c.to == x.id)
                    {
                        complaints = complaints + 1;
                    }
                }
            }


            ViewBag.emps = emps;
            ViewBag.emp_branchs = emp_branchs;
            ViewBag.emp_services = emp_services;
            ViewBag.allreq = allreq;
            ViewBag.pendingreq = pendingreq;
            ViewBag.repliedmsg = repliedmsg;
            ViewBag.apps = apps.Count();
            ViewBag.numofReqs = numofReqs;
            ViewBag.numofPendReqs = numofPendReqs;
            ViewBag.complaints = complaints;
            return View();
        }

        public List<Services_> Emp_Services(int emp_id)
        {
            var emp_services = db.Emp_Org_Services.Where(c => c.emp_id == emp_id).ToList();
            List<Services_> services = new List<Services_>();
            foreach(var x in emp_services)
            {
                services.Add(x.Services_);
            }
            return services;
        }

        public int Emp_Requests(int emp_id)
        {
            var allreq = db.Request.Where(c => c.empoyee_id == emp_id).ToList().Count();
            return allreq;
        }

        public int Pending_Requests(int emp_id)
        {
            var pendingreq = db.Request.Where(c => c.empoyee_id == emp_id && c.state == "pending").ToList().Count();
            return pendingreq;
        }

        public int Replied_Messages(int emp_id)
        {
            var repliedmsg = db.Messages.Where(c => c.emp_id == emp_id).ToList().Count();
            return repliedmsg;
        }
        public ActionResult showempBlocked()
        { 
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var manager = db.user.SingleOrDefault(c => c.id == UserID);
            var org_id = db.organization.SingleOrDefault(c => c.manger_id == manager.id).id;
            var emps = db.Emp_Org_Services.Where(c => c.org_id == org_id).ToList();
            List<user> employees = new List<user>();
            if (emps.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                foreach (var x in emps)
                {
                    var e = db.user.SingleOrDefault(c => c.id == x.emp_id);
                    if (e.block == true)
                    {


                        employees.Add(e);
                    }
                }

                ViewBag.users = employees;
                ViewBag.numberRow = employees.Count();
                return View();
            }
          
        }



        public ActionResult showempUNBlocked()
        {
            ViewBag.fullname = Session["UserName"];
            ViewBag.userid = Session["UserID"];
            ViewBag.user_id = new SelectList(db.user, "Id");
            int UserID = Convert.ToInt32(Session["UserID"]);
            var manager = db.user.SingleOrDefault(c => c.id == UserID);
            var org_id = db.organization.SingleOrDefault(c => c.manger_id == manager.id).id;
            var emps = db.Emp_Org_Services.Where(c => c.org_id == org_id).ToList();
            List<user> employees = new List<user>();
            if (emps.Equals(null))
            {
                return HttpNotFound();
            }
            else
            {
                foreach (var x in emps)
                {
                    var e = db.user.SingleOrDefault(c => c.id == x.emp_id);
                    if (e.block == false)
                    {


                        employees.Add(e);
                    }
                }
                ViewBag.users = employees;
                ViewBag.numberRow = employees.Count();
                return View();
            }
          
        }

        public ActionResult blockemp(int id)
        {
            var blockemp = db.user.SingleOrDefault(c => c.id == id);
            blockemp.block = true;
            db.SaveChanges();
            db.user.Remove(blockemp);
            return RedirectToAction("showempUNBlocked");
        }


        public ActionResult unblockemp(int id)
        {

            var blockemp = db.user.SingleOrDefault(c => c.id == id);
            blockemp.block = false;
            db.SaveChanges();
            db.user.Remove(blockemp);
            return RedirectToAction("showempBlocked");
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