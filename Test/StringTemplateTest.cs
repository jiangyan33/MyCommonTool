using MyCommonTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyCommonTool.Test
{
    public class StringTemplateTest
    {
        public static void RunTest()
        {
            var person = new Person
            {
                Name = "江岩",
                Age = 25,
                Accounts = new List<string>
                {
                    "CEO",
                    "CTO"
                },
                Hobbies = new List<Hobby>
                {
                    new Hobby
                    {
                        Id= Guid.NewGuid().ToString(),
                        Name ="C#"
                    },
                     new Hobby
                    {
                        Id= Guid.NewGuid().ToString(),
                        Name="JavaScript"
                    }
                }
            };
            var source = Utils.Utils.ReadFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test/index.html"));

            var template = new StringTemplate(source);
            var result = template.Compile(person);
            Console.WriteLine(result);
        }
    }
}