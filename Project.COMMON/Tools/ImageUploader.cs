using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Project.COMMON.Tools
{
    public static class ImageUploader
    {
        //Geriye string değer döndüren metodumuz parametre olarak resmin hangi dosya yolunda oldugunu ve hangi resmi alacağını bilmek zorunda

        //HttpPostedFileBase => MVC'de eger Post yönteminde dosya geliyorsa bu, HttpPostedFileBase tipinde tutulur...

        public static string UploadImage(string serverPath,HttpPostedFileBase file)
        {
            if (file!=null)
            {
                Guid uniqueName = Guid.NewGuid();
                // ~/Images/asadasdasd.jps
                // /Images
                //MVC'de ~ karakteri normal şekilde kök dizine çık olarak backend işlemlerinden geçtikten sonra algılanmaz...(@Url.Action helper'i kullanmadığınız sürece...) Dolayısıyla bu string karakteri kaldırarak path'i düzenlemek daha sağlıklıdır...

                serverPath = serverPath.Replace("~", string.Empty); //Eger gelen serverPath parametresinde ~(tilda) karakteri varsa onu boslukla degiştirdik... 

                //cagri.starWars.tieFighter.png

                string[] fileArray = file.FileName.Split('.'); //Burada Split metodu sayesinde ilgili yapının uzantısını da iceren bir elemanlar kümesi olusturduk...Split metodu belirttigimiz char karakterinden metni bölerek bize bir Array sunar... "cagri", "starWars","tieFighter","png"

                string extension = fileArray[fileArray.Length - 1].ToLower(); //Dosya uzantısını yakalayarak kücük harflere çevirdik...

                string fileName = $"{uniqueName}.{extension}"; //kendimize has yepyeni bir dosya adı olusturduk(sonuna da uzantısını ekledik)

                if (extension=="jpg"||extension=="gif"||extension=="png"||extension=="jpeg")
                {

                    if (File.Exists(HttpContext.Current.Server.MapPath(serverPath+fileName)))
                    {
                        return "1"; //(Dosya zaten var kodu) ancak Guid ile isim belirledigimiz bu açıdan mevcut şartlarda güvendeyiz...Eger siz ismi Guid ile degil de anlamlı olabilmesi icin kullanıcı input'undan alırsanı o zaman bu kontrolü yapmalısınız...
                    }
                    else
                    {
                        string filePath = HttpContext.Current.Server.MapPath(serverPath + fileName);
                        file.SaveAs(filePath);
                        return serverPath + fileName;
                    }



                }
                else
                {
                    return "2"; //Secilen dosya bir resim degildir
                }

                
            }

            return "3"; //Dosya null kodu
        }


    }
}