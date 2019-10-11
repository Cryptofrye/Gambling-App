using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GamblingAppClient
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Base code for gambling game client");

            Console.Write("Please Enter Your Username: ");
            string username = Console.ReadLine();
            Console.Write("Please Enter Your Password: ");
            string password = GetPassword();

            Console.WriteLine($"\nUsername Was: {username}");
            Console.WriteLine($"Password Was: {password.ToString()}");


            var request = (HttpWebRequest)WebRequest.Create("XXX.XXX.XXX.XXX");

            var postData = "username=" + Uri.EscapeDataString("hello");
            postData += "&password=" + Uri.EscapeDataString("world");
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            Console.WriteLine(responseString);
            Console.ReadLine();

        }

        public static string GetPassword()
        {
            string pwd = "";
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd = pwd.Substring(0, (pwd.Length - 1));
                        Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000') // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
                {
                    pwd += i.KeyChar;
                    Console.Write("*");
                }
            }
            return pwd;
        }
    }
}
