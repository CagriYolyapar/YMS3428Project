using PagedList;
using Project.BLL.DesignPatterns.GenericRepository.ConcRep;
using Project.COMMON.Tools;
using Project.ENTITIES.Models;
using Project.WEBUI.Models.ShoppingTools;
using Project.WEBUI.VMClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Project.WEBUI.Controllers
{
    public class ShoppingController : Controller
    {

        OrderRepository _oRep;
        ProductRepository _pRep;
        CategoryRepository _cRep;
        OrderDetailRepository _odRep;

        public ShoppingController()
        {
            _oRep = new OrderRepository();
            _pRep = new ProductRepository();
            _cRep = new CategoryRepository();
            _odRep = new OrderDetailRepository();
        }


        // GET: Shopping

        //Sayfalama işlemleri yapmak icin Pagination kütüphanesinden yararlanıyoruz(PagedList)
        public ActionResult ShoppingList(int? page, int? categoryID) //nullable int vermemizin sebebi aslında buradaki int'in sayfa sayımızı temsil edecek olmasıdır...Ancak birisi ilk kez siteye girdiginde sayfa sayısı gönderemeyecektir bu durumda da ilk sayfadan baslayacaktır...
        {

            //string a = "Mehmet";
            //a = null;
            //string b = a ?? "Çagri"; //Eger a null ise b'ye Cagrı degerini at..Eger a null degilse b'ye git a'nın degerini at...

            //page??1

            //Bu Action iki parametre almaktadır(page ve categoryID) ve bunlar null da gelebilmektedir... Bu action'a bu bilgiler gelmedigi takdirde Category'den bagımsız tüm ürünlerin gösterilmesinin yanı sıra page de 1. sayfadan baslayacaktır...page argümanı null geliyorsa 1. sayfadan baslama talimatı ToPagedList metodunun icerisindeki page??1 argümanı sayesinde olmustur. Bu talimat aynı zamanda eger page argümanı null degilse o argümanın degerini alarak o sayfadan baslat demektir...Bunun yanında bu metodun ikinci argümanı da bir sayfada kac veri gösterilecegini belirtir (9).. Ternary if'in else kısmında(:) bu durumda sadece ilgili categoryID'sindeki ürünleri listeleyerek yine ToPagedList ile bir Pagination talimatı verilmiştir...


            PAVM pavm = new PAVM
            {
                PagedProducts = categoryID == null ? _pRep.GetActives().ToPagedList(page ?? 1, 9) : _pRep.Where(x => x.CategoryID == categoryID).ToPagedList(page ?? 1, 9),
                Categories = _cRep.GetActives()
            };

            if (categoryID != null) TempData["catID"] = categoryID; //bu talimatı eger bir Category secilmişse o bilgiyi saklayabilmek ve Pagination'a tekrar gönderebilmek icin tutuyoruz...

            return View(pavm);
        }

        public ActionResult AddToCart(int id)
        {
            Cart c = Session["scart"] == null ? new Cart() : Session["scart"] as Cart;
            Product eklenecekUrun = _pRep.Find(id);

            CartItem ci = new CartItem
            {
                ID = eklenecekUrun.ID,
                Name = eklenecekUrun.ProductName,
                Price = eklenecekUrun.UnitPrice,
                ImagePath = eklenecekUrun.ImagePath
            };
            c.SepeteEkle(ci);
            Session["scart"] = c;
            return RedirectToAction("ShoppingList");
        }

        public ActionResult CartPage()
        {
            if (Session["scart"] != null)
            {
                CartPageVM cpvm = new CartPageVM();
                Cart c = Session["scart"] as Cart;
                cpvm.Cart = c;
                return View(cpvm);
            }
            TempData["sepetBos"] = "Sepetinizde ürün bulunmamaktadır";
            return RedirectToAction("ShoppingList");
        }

        public ActionResult DeleteFromCart(int id)
        {
            if (Session["scart"] != null)
            {
                Cart c = Session["scart"] as Cart;
                c.SepettenSil(id);
                if (c.Sepetim.Count == 0)
                {
                    Session.Remove("scart");
                    TempData["sepetBos"] = "Sepetinizde tamamen bosalmıstır";
                    return RedirectToAction("ShoppingList");
                }
                return RedirectToAction("CartPage");
            }

            return RedirectToAction("ShoppingList");
        }





        public ActionResult SiparisiOnayla()
        {
            //Todo Ödev
            AppUser mevcutKullanici;
            if (Session["member"] != null)
            {
                mevcutKullanici = Session["member"] as AppUser;
            }
            else TempData["anonim"] = "Kullanıcı üye degil";
            return View();
        }




        //https://localhost:44337/api/Payment/ReceivePayment

        [HttpPost]
        public ActionResult SiparisiOnayla(OrderVM ovm)
        {
            bool result;

            Cart sepet = Session["scart"] as Cart;
            ovm.Order.TotalPrice = ovm.PaymentDTO.ShoppingPrice = sepet.TotalPrice;

            #region APISection


            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44337/api/");
                Task<HttpResponseMessage> postTask = client.PostAsJsonAsync("Payment/ReceivePayment", ovm.PaymentDTO);
                HttpResponseMessage sonuc;

                try
                {
                    sonuc = postTask.Result;
                }
                catch (Exception ex)
                {
                    TempData["baglantiRed"] = "Banka baglantıyı red etti";
                    return RedirectToAction("ShoppingList");
                }

                if (sonuc.IsSuccessStatusCode) result = true;
                else result = false;

                if (result)
                {
                    if (Session["member"]!=null)
                    {
                        AppUser kullanici = Session["member"] as AppUser;
                        ovm.Order.AppUserID = kullanici.ID;
                        ovm.Order.UserName = kullanici.UserName;
                    }
                    else
                    {
                        ovm.Order.AppUserID = null;
                        ovm.Order.UserName = TempData["anonim"].ToString();
                    }

                    _oRep.Add(ovm.Order); //OrderRepository bu noktada Order'i eklerken onun ID'sini olusturuyor ki...

                    foreach (CartItem item in sepet.Sepetim)
                    {
                        OrderDetail od = new OrderDetail();
                        od.OrderID = ovm.Order.ID;
                        od.ProductID = item.ID;
                        od.TotalPrice = item.SubTotal;
                        od.Quantity = item.Amount;
                        _odRep.Add(od);

                        //Stoktan düsmesini istiyorsanız
                        Product stokDus = _pRep.Find(item.ID);
                        stokDus.UnitsInStock -= item.Amount;
                        _pRep.Update(stokDus);


                    }

                    TempData["odeme"] = "Siparişiniz bize ulasmıstır..Tesekkür ederiz";
                    MailSender.Send(ovm.Order.Email, body: $"Siparişiniz basarıyla alındı..{ovm.Order.TotalPrice}",subject:"Sipariş!!");
                    Session.Remove("scart");
                    return RedirectToAction("ShoppingList");


                }

                else
                {
                    TempData["sorun"] = "Odeme ile ilgili bir sorun olustu..Lutfen bankanız ile iletişime geciniz";
                    return RedirectToAction("ShoppingList");
                }

            }




            #endregion
        }


    }
}