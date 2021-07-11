﻿namespace Toast.Elements
{
    public class Text : Element
    {
        public Text(string value) : base(value) { }

        public new string GetValue()
        {
            return (string)base.GetValue();
        }
    }
}
