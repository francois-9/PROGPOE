using PROGPOE.Models.Data;
using PROGPOE.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PROGPOE.Areas.Admin.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare list of pages
            List<PageVM> pagesList;

            //initialize the list
            using (Db db = new Db())
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            //return the view with the list
            return View(pagesList);
        }

        //GET: Admin/Page/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        //POST: Admin/Page/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //Declare slug
                string slug;

                //Initialize pageDTO
                PageDTO dto = new PageDTO();

                //DTO title
                dto.Title = model.Title;

                //Check for and set slug
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Makesure title and slug is unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }


                //DTO reset
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //Set Tempdata message
            TempData["SM"] = "You have added a new page";


            //Redirect
            return RedirectToAction("AddPage");
        }
        [HttpGet]
        //Get: Admin/Page/EditPage/ID
        public ActionResult EditPage(int id)
        {

            //Declare Page View Model
            PageVM model;

            using (Db db = new Db())
            {
                //Get the page
                PageDTO dto = db.Pages.Find(id);

                //Confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist");
                }

                //Initialize page
                model = new PageVM(dto);

            }
            //Return with view model
            return View(model);
        }

        //Post: Admin/Page/EditPage/ID
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //Get page id 
                int id = model.Id;

                //Initialize slug
                string slug = "home";

                //Get the pages
                PageDTO dto = db.Pages.Find(id);

                //DTO the title
                dto.Title = model.Title;

                //Check for the slug and set it if need be
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //Make sure the title and the slug is unqiue
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                   db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }

                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;


                //Save the DTO
                db.SaveChanges();
            }

            //Set TemptData message
            TempData["SM"] = "You have edited the page!";

            //Redirect
            return RedirectToAction("EditPage");


        }

        //Get: Admin/Page/PageDetails/ID
        public ActionResult PageDetails(int id)
        {
            //Declare PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Get the page
                PageDTO dto = db.Pages.Find(id);

                //Confirm the page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                //Initialize PageVM
                model = new PageVM(dto);

            }

            //Return view with the model
            return View(model);
        }

        //Get: Admin/Pages/DeletePage/Id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //Get the page
                PageDTO dto = db.Pages.Find(id);

                //Remove the page
                db.Pages.Remove(dto);

                //Save
                db.SaveChanges();

            }
            //Redirect
            return RedirectToAction("Index");
        }

        //Post: Admin/Pages/ReorderPages/ID
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //Set initial count
                int count = 1;

                //Declare page dto
                PageDTO dto;

                //Set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        //Get: Admin/Pages/EditSideBar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Declare the model
            SidebarVM model;

            using (Db db = new Db())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //Initialize the model
                model = new SidebarVM(dto);

            }
            //return view with the model

            return View(model);
        }

        //Post: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the body
                dto.Body = model.Body;

                //Save
                db.SaveChanges();
            }
            //Set TempData Message
            TempData["SM"] = "You have edited the sidebar";

            //Redirect
            return RedirectToAction("EditSidebar");
        }
    }
}
