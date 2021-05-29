using Project.BLL.DesignPatterns.GenericRepository.ConcRep;
using Project.COMMON.Tools;
using Project.ENTITIES.Models;
using Project.WEBUI.VMClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.WEBUI.Controllers
{
    public class RegisterController : Controller
    {

        AppUserRepository _apRep;
        ProfileRepository _proRep;

        public RegisterController()
        {
            _apRep = new AppUserRepository();
            _proRep = new ProfileRepository();
        }

        // GET: Register
        public ActionResult RegisterNow()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterNow(AppUserVM apvm)
        {
            AppUser appUser = apvm.AppUser;
            AppUserProfile profile = apvm.Profile;

            appUser.Password = DantexCrypt.Crypt(appUser.Password); //sifreyi kriptoladık

            if (_apRep.Any(x => x.UserName == appUser.UserName))
            {
                ViewBag.ZatenVar = "Kullanıcı ismi daha önce alınmıs";
                return View();
            }
            else if (_apRep.Any(x => x.Email == appUser.Email))
            {
                ViewBag.ZatenVar = "Email zaten kayıtlı";
                return View();
            }

            //Kullanıcı basarılı bir şekilde Register işlemini tamamladıysa ona Mail gönderecegiz... https://localhost:44329/

            string gonderilecekEmail = "Tebrikler... Hesabınız olusturulmustur...Hesabınızı aktive etmek icin https://localhost:44329/Register/Activation/" + appUser.ActivationCode + " linkine tıklayabilirsiniz";

            MailSender.Send(appUser.Email, body: gonderilecekEmail, subject: "Hesap aktivasyon!");

            _apRep.Add(appUser); //profilden önce bunu eklemelisiniz önceliginiz bunu eklemek olmalı..Cünkü AppUser'in ID'si ilk basta olusmalı...Cünkü bizim kurdugumuz birebir ilişkide AppUser zorunlu olan alan Profile ise opsiyonel alandır. Dolayısıyla ilk basta AppUser'in ID'si SaveChanges ile olusmalı ki Profile'i rahatca ekleyebilelim(eger ekleyeceksek)

            if (!string.IsNullOrEmpty(profile.FirstName.Trim()) || !string.IsNullOrEmpty(profile.LastName.Trim()) || !string.IsNullOrEmpty(profile.Address.Trim()))
            {
                profile.ID = appUser.ID;
                _proRep.Add(profile);
            }



            return View("RegisterOk");
        }


        public ActionResult Activation(Guid id)
        {
            AppUser aktifEdilecek = _apRep.FirstOrDefault(x => x.ActivationCode == id);
            if (aktifEdilecek != null)
            {
                aktifEdilecek.Active = true;
                _apRep.Update(aktifEdilecek);
                TempData["HesapAktifMi"] = "Hesabınız aktif hale getirildi";

            }
            else
                TempData["HesapAktifMi"] = "Hesabınız bulunamadı";
            return RedirectToAction("Login", "Home");
        }

        public ActionResult RegisterOk()
        {
            return View();
        }
    }
}