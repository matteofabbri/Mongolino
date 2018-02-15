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



            Console.WriteLine("Let's create a new article in our blog");

            BlogArticle.Create(new BlogArticle
            {

            });
        }
    }
}
