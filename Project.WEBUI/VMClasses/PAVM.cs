using PagedList;
using Project.ENTITIES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.WEBUI.VMClasses
{
    //Pagination amacıyla olusturulmus (ShoppingList) VM'dir
    public class PAVM
    {
        public Product Product { get; set; }
        public List<Category> Categories { get; set; }
        public IPagedList<Product> PagedProducts { get; set; } //Sayfalama işlemleri icin (Pagination) tutulan Property'dir...
    }
}