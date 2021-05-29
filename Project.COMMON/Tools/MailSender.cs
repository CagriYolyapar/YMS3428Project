using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace Project.COMMON.Tools
{
    //yms34243423@gmail.com
    //34243424


    public static class MailSender
    {
        public static void Send(string receiver,string password="34243424",string body="Test mesajıdır",string subject="E-Mail Testi",string sender= "yms34243423@gmail.com")
        {
            MailAddress senderEmail = new MailAddress(sender);
            MailAddress receiverEmail = new MailAddress(receiver);

            //Bizim Email işlemlerimiz SMTP'ye göre yapılır...
            //Kullandıgınız hesap (gönderim yaptığınız hesap) gmail ise onun baska uygulamalar tarafından mesaj gönderme özelligini acmalısınız...
            SmtpClient smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address,password)
            };


            using (MailMessage message = new MailMessage(senderEmail, receiverEmail) { 
            
            Subject = subject,
            Body = body

            })
            {
                smtp.Send(message);
            };
            
                
            
        }
    }
}