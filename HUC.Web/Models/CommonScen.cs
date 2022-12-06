using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace HUC.Web.Models
{
    public class CommonScen
    {
    }
    public class StrCmpLogicalComparer : Comparer<string>
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string x, string y);

        public override int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }
}