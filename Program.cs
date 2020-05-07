using NLog;
using BlogsConsole.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogsConsole
{
    class MainClass
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            logger.Info("Program started");
            try
            {
                string choice;
                do
                {
                    Console.WriteLine("Enter your selection:");
                    Console.WriteLine("1) Display all blogs");
                    Console.WriteLine("2) Add Blog");
                    Console.WriteLine("3) Create Post");
                    Console.WriteLine("Enter 4 to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info("Option {choice} selected", choice);

                    if (choice == "1")
                    {

                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(b => b.Name);

                        Console.WriteLine($"{query.Count()} Blogs returned");
                        foreach (var item in query)
                        {
                            Console.WriteLine(item.Name);
                        }
                    }
                    else if (choice == "2")
                    {

                        Console.Write("Enter a name for a new Blog: ");
                        var blog = new Blog { Name = Console.ReadLine() };

                        ValidationContext context = new ValidationContext(blog, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(blog, context, results, true);
                        if (isValid)
                        {
                            var db = new BloggingContext();

                            if (db.Blogs.Any(b => b.Name == blog.Name))
                            {

                                isValid = false;
                                results.Add(new ValidationResult("Blog name exists", new string[] { "Name" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");

                                db.AddBlog(blog);
                                logger.Info("Blog added - {name}", blog.Name);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {

                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(b => b.BlogId);

                        Console.WriteLine("Select the blog you would to post to:");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.BlogId}) {item.Name}");
                        }
                        if (int.TryParse(Console.ReadLine(), out int BlogId))
                        {
                            if (db.Blogs.Any(b => b.BlogId == BlogId))
                            {
                                Post post = InputPost(db);
                                if (post != null)
                                {
                                    post.BlogId = BlogId;
                                    db.AddPost(post);
                                    logger.Info("Post added - {title}", post.Title);
                                }
                            }
                            else
                            {
                                logger.Error("There are no Blogs saved with that Id");
                            }
                        }
                        else
                        {
                            logger.Error("Invalid Blog Id");
                        }
                    }

                } while (choice != "4");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            logger.Info("Program ended");
        }

        public static Post InputPost(BloggingContext db)
        {
            Post post = new Post();
            Console.WriteLine("Enter the Post title");
            post.Title = Console.ReadLine();
            Console.WriteLine("Enter the Post content");
            post.Content = Console.ReadLine();

            ValidationContext context = new ValidationContext(post, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(post, context, results, true);
            if (isValid)
            {
                return post;
            }
            else
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
            return null;
        }

        
    }
}

