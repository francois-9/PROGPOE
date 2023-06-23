using PROGPOE.Models.Data;
using PROGPOE.Models.ViewModels.Account;
using PROGPOE.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PROGPOE.Controllers
{

    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        //GET:/account/login
        [HttpGet]
        public ActionResult Login()
        {
            //Confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");
            }
            //return view
            return View();
        }

        //POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //Check if user is invalid
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }
            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }

        }

        //GET:/account/create-account
        [HttpGet]
        [ActionName("create-account")]
        [AllowAnonymous]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }



        //POST:/account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }
            //Check if password match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //Check if username is unique 
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Username " + model.Username + " is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }
                //Create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password

                };

                //Add the dto
                db.Users.Add(userDTO);

                //Save
                db.SaveChanges();

                //Add to userRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                //Add the DTO
                db.UserRoles.Add(userRolesDTO);

                //Save
                db.SaveChanges();

            }
            //Create a TempData message
            TempData["SM"] = "You are now registered and can login.";

            //Redirect
            return Redirect("~/account/login");
        }

        //GET: /account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            //Get username
            string username = User.Identity.Name;

            //Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            //Return partial view with model
            return PartialView(model);
        }

        //GET:/account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            //Get username
            string username = User.Identity.Name;

            //Decalare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                //Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //Build model
                model = new UserProfileVM(dto);
            }

            //Return view with model.
            return View("UserProfile", model);
        }

        //POST:/account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //Check if passwords match
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //Get username
                string username = User.Identity.Name;

                //Make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Username " + model.Username + " already exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                //Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Address = model.Address;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
                //Save
                db.SaveChanges();
            }

            //Set TempData message
            TempData["SM"] = "You have edited your profile.";

            //Redirect
            return Redirect("~/account/user-profile");
        }

        //GET: /account/orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //Initialize list of OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                //Get user id
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                //Get users address address
                user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();

                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                //Loop through lsit of OrderVM
                foreach (var order in orders)
                {
                    //Init products dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();



                    //Declare Total
                    decimal total = 0m;

                    //Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        //Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        //Get product price
                        decimal price = product.Price;

                        //Get product name
                        string productName = product.Name;

                        //Add to product dict
                        productsAndQty.Add(productName, orderDetails.Quantity);



                        //Get total
                        total += orderDetails.Quantity * price;

                        //Get total
                        ordersForUser.Add(new OrdersForUserVM()
                        {
                            OrderNumber = order.OrderId,
                            Total = total,

                            ProductsAndQty = productsAndQty,

                            //Address =address

                            CreatedAt = order.CreatedAt
                        });
                    }
                }
            }

            //Return view with the list of OrdersForUserVM
            return View(ordersForUser);
        }
    }
}