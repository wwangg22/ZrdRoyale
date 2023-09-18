using System;
using System.Collections.Generic;
using System.Text;

namespace ClashRoyale.Extensions.Utils
{
    public class AdminUtils
    {
        public static bool CheckIfAdmin(int id)
        {
            if(id == Resources.Configuration.admin1 ^ id == Resources.Configuration.admin2 ^ id == Resources.Configuration.admin3)
                return true;
            return false;
        }
    }
}
