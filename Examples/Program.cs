using System;

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("First we need a user for our theorical application");
            var user = ApplicationUser.GetOrCreate(x => x.UserName == "matteo", new ApplicationUser
            {
                Name = "Matteo",
                FamilyName = "Fabbri",
                UserName = "matteo"
            });

            Console.WriteLine("Family name field is marked with the full text attribute so is full text indexed");

            foreach (var us in ApplicationUser.FullTextSearch("fabbri"))
            {
                Console.WriteLine(us.UserName);
            }

            Console.WriteLine("Now our user is logged in, so we can update his informations");
            ApplicationUser.Increase(user, x => x.LogInNumber, 1);

            //Obviously, even in async mode
            ApplicationUser.UpdateAsync(user, x => x.LastLogin, DateTime.Now).Wait();


            Console.WriteLine("Let's create a new article in our blog");

            BlogArticle.Create(new BlogArticle
            {
                //Author is a ObjectRef so it automatically keep trace of the user which is in another collection
                Author = user,
                Title = "THIS IS AN EXAMPLE",
                Body = "Babl blaalds asd dsad asdsa dsa dasdsadad asdsadsadsad",
                Category = "examples"
            });

            Console.WriteLine("Blog is not marked with index attibutes but we can define them at runtime");
            BlogArticle.DescendingIndex(x=>x.Category);


            Console.WriteLine("And obviously search trougth");
            foreach (var item in BlogArticle.Where(x=>x.Category == "examples"))
            {
                Console.WriteLine($"{item.Title} by {item.Author.Value.UserName}");
            }
        }
    }
}
