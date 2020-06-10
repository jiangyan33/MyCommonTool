using System;
using System.Collections.Generic;

namespace MyCommonTool.Test
{
    internal class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<string> Accounts { get; set; }
        public List<Hobby> Hobbies { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}