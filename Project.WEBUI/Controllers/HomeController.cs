using Project.BLL.DesignPatterns.GenericRepository.ConcRep;
using Project.COMMON.Tools;
using Project.ENTITIES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.WEBUI.Controllers
{
    public class HomeController : Controller
    {

        AppUserRepository _apRep;

        public ActionResult Simulation()
        {
            return View();
        }


        public HomeController()
        {
            _apRep = new AppUserRepository();
        }

        // GET: Home
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(AppUser appUser)
        {
            AppUser yakalanan = _apRep.FirstOrDefault(x => x.UserName == appUser.UserName);
            if (yakalanan == null)
            {
                ViewBag.Kullanici = "Kullanıcı bulunamadı";
                return View();
            }

            string decrypted = DantexCrypt.DeCrypt(yakalanan.Password); //Veritabanımızdaki hashlenmiş şifremizi alarak decrypt ettik...
            if (appUser.Password == decrypted && yakalanan.Role == ENTITIES.Enums.UserRole.Admin)
            {
                if (!yakalanan.Active)
                {
                    return AktifKontrol();
                }
                Session["admin"] = yakalanan;
                //Eger bir RedirectToAction metodu ile yönlendirme yapmak istediginiz alan bir Area ise bunun acıkca RouteValues parametresinde anonim tip olarak belirtilmesi gerekir.
                return RedirectToAction("CategoryList", "Category", new { Area = "Admin" });
            }
            else if (yakalanan.Role == ENTITIES.Enums.UserRole.Member && appUser.Password == decrypted)
            {
                if (!yakalanan.Active)
                {
                    return AktifKontrol();
                }
                Session["member"] = yakalanan;
                return RedirectToAction("ShoppingList", "Shopping");
            }
            ViewBag.Kullanici = "Kullanici bulunamadı";
            return View();

        }

        private ActionResult AktifKontrol()
        {
            ViewBag.AktifDegil = "Lutfen hesabınızı aktif hale getiriniz..Mailinizi kontrol ediniz";
            return View("Login");
        }
    }
}