﻿namespace Toast.Elements
{
    public class Command : Element
    {
        public Command(string value) : base(value) { }

        public new string GetValue()
        {
            return (string)base.GetValue();
        }
    }
}
