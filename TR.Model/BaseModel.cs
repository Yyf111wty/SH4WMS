using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
{
    public class BaseModel
    {
        private string textField;
        private string valueField;

        public string Text
        {
            get { return this.textField; }
            set { this.textField = value; }
        }

        public string Value
        {
            get { return this.valueField; }
            set { this.valueField = value; }
        }
    }
}