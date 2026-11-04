using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace TRWCS_WebAPI.Common
{
    public class RegularFil
    {
        public static int RegularLorN(string a)//字母或数字
        {
            Regex regularLorN = new Regex("^[A-Za-z0-9]+$");//字母或数字
            if (regularLorN.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int RegularL(string a)//字母
        {
            Regex regularL = new Regex("^[A-Za-z]+$");//字母
            if (regularL.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int Regularl(string a)//小写字母
        {
            Regex regularL = new Regex("^[a-z]+$");//字母
            if (regularL.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int RegularLorC(string a)//汉字或字母
        {
            Regex regularLorC = new Regex("^[\u4E00-\u9FA5A-Za-z]+$");//汉字或字母
            if (regularLorC.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int RegularN(string a)//数字
        {
            Regex regularN = new Regex("^[0-9]*$");
            if (regularN.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int RegularCorLorN(string a)//汉字或字母或数字
        {
            Regex regularCorLorN = new Regex("^[\u4E00-\u9FA5A-Za-z0-9]+$");//字母或数字
            if (regularCorLorN.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int RegularEmail(string a)//邮箱
        {
            Regex regularEmail = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            if (regularEmail.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int RegularInt(string a)//正整数
        {
            Regex regularInt = new Regex(@"^\d+$");
            if (regularInt.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int RegularC(string a)//汉字
        {
            Regex regularInt = new Regex("^[\u4e00-\u9fa5]{0,}$");
            if (regularInt.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static int RegularLink(string a)//链接地址
        {
            
            Regex regularLink = new Regex("^(?=.*[a-zA-Z]+)(?=.*[/]+)[a-zA-Z/]+$");
            if (regularLink.IsMatch(a))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}