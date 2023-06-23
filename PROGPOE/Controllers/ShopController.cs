using PROGPOE.Models.Data;
using PROGPOE.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PROGPOE.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //Declare list of CategoryVM
            List<CategoryVM> categoryVMList;

            //Init the list
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            //Return partial list
            return PartialView(categoryVMList);
        }

        //GET: Shop/Category/Name
        public ActionResult Category(string name)
        {
            //Declare a list of ProductVM
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //Get category id
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                //Init the list
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //Get category name 
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                if (productCat == null)
                {

                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }

            //Return view with list
            return View(productVMList);
        }

        //GET: Shop/Product-details/Name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //Declare the VM and the DTO
            ProductVM model;
            ProductDTO dto;

            //Init  product id
            int id = 0;

            using (Db db = new Db())
            {

                //Check if product exist
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                //Init productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //Get id
                id = dto.Id;

                //Init model
                model = new ProductVM(dto);

            }

            //Get all gallery images
          //  model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
           // model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
            //    .Select(fn => Path.GetFileName(fn));


            //Return view model
            return View("ProductDetails", model);
        }


    }
}