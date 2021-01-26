已经实现的功能

1.简单的字符串模板
```c#
        string source = $@"<html>
                                <div>
                                    <p> {{{{Name}}}}</p>
                                    <p> {{{{CreateDate}}}}</p>
                                    <p> {{{{Accounts[0]}}}}</p>
                                </div>
                                <ul>
                                    {{{{#each Hobbies}}}}
                                    <li>
                                        {{{{this.Name}}}}
                                    </li>
                                    <li>
                                        {{{{this.CreateDate}}}}
                                    </li>
                                    {{{{/each}}}}
                                </ul>
                                <ul>
                                    {{{{#each Accounts}}}}
                                    <li>{{{{this}}}}</li>
                                    {{{{/each}}}}
                                </ul>
                            </html>";
        var template = new StringTemplate(source);
        var person = new Person
        {
            Name = "jiangyan",
            Accounts = new List<string> { "One", "Two" },
            Hobbies = new List<Hobby> {
                new Hobby{
                    Name="C#"
                },
                new Hobby{
                    Name="JavaScript"
                }
            }
        };
        var result = template.Compile(person);
        Console.WriteLine(result);
        
    internal class Person
    {
        public string Name { get; set; }
        public List<string> Accounts { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public List<Hobby> Hobbies { get; set; }
    }

    internal class Hobby
    {
        public string Name { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
```
输出
```html
<html>
    <div>
        <p> jiangyan</p>
        <p> 2020-06-10 18:03:53</p>
        <p> One</p>
    </div>
    <ul>
                                        
        <li>
            C#
        </li>
        <li>
            2020-06-10 18:03:53
        </li>
                                        
        <li>
            JavaScript
        </li>
        <li>
            2020-06-10 18:03:53
        </li>
                                        
    </ul>
    <ul>
                                        
        <li>One</li>
                                        
        <li>Two</li>
                                        
    </ul>
</html>
```
2.动态获取实体类中的属性
```c#
    // 上方的那个实体类
    var person = new Person
    {
        Name = "jiangyan"
    };
    var name = person.GetAttrValue<string>("Name");
    var count = person.GetAttrValue<string>("Count");
    Console.WriteLine(name); // jiangyan
    Console.WriteLine(count); // 
```
2.0版本新增一些公用方法
  全角转半角、AES加密解密、Sha1加密、整数转换中文(仅支持1万以内)、url中query参数的相关操作、随机字符串、当前时间戳等等
