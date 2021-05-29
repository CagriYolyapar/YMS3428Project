using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.ENTITIES.Models
{
    public class Order:BaseEntity
    {
        public string ShippedAddress { get; set; }
        public decimal TotalPrice { get; set; }

        //Sipariş işlemlerinin icerisindeki bilgileri daha rahat yakalamak adına actıgımız property'lerdir...
        public string UserName { get; set; }
        public string Email { get; set; }
        public int? AppUserID { get; set; }



        //Relaitonal Properties
        public virtual AppUser AppUser { get; set; }
        public virtual List<OrderDetail> OrderDetails { get; set; }




    }
}
