using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XYD.Models;

namespace XYD.Common
{
    public class VendorUtil
    {
        public static XYD_Vendor GetVendor(string vendorNo)
        {
            using(var db = new DefaultConnection())
            {
                var vendor = db.Vendor.Where(n => n.Code == vendorNo).FirstOrDefault();
                return vendor;
            }
        }
    }
}