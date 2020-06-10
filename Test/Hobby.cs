using System;

namespace MyCommonTool.Test
{
    internal class Hobby
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}