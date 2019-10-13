using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Threading;
using RestSharp;

namespace GamblingAppClient
{
    class Program
    {

        static string serverIP = "127.0.0.1"; //Default/Fallback Server IP
        static int serverPort = 1337; //Default/Fallback Server Port

        static void Main(string[] args)
        {

            bool loggedIn = false;
            RestRequest request;
            IRestResponse response;
            CookieContainer cookieContainer = new CookieContainer(); //CookieContainer so that all RestRequests use the same cookies (used for authentication with the server)
            string loggedInUsername = "";

            int choice = 0; //Used in menu inputs

            getServerInfoFromConfig(); //Gets server IP and port information from the Config.xml file

            /*Console.WriteLine($"Server IP: {serverIP}");
            Console.WriteLine($"Server Port: {serverPort}");
            Console.WriteLine("\n[*] Press Enter To Continue...");
            Console.ReadLine();*/

            RestClient client = new RestClient($"http://{serverIP}:{serverPort}"); //Creates RestSharp Client That We Can Use To Make Web Requests (Prettier Than HttpWebRequest)
            client.CookieContainer = cookieContainer; //Sets the RestClient's cookie container to the global one so we can store cookies between requests

            string[] mainMenuText = new string[3] {
                "[-] 1) Log In",
                "[-] 2) Register",
                "[-] 99) Quit"
            };

            string[] loggedInMenuText = new string[4] {
                "[-] 1) Play Game",
                "[-] 2) How To Play",
                "[-] 98) Change Account Details",
                "[-] 99) Logout"
            };

            string[] changeAccountDetailsMenuText = new string[4] {
                "[-] 1) Change Username",
                "[-] 2) Change Password",
                "[-] 3) Change Username & Password",
                "[-] 99) Go Back"
            };

            bool running = true; //Used to escape the upper-most while loop (to exit program)

            while (!loggedIn && running)
            {
                Console.Clear(); //Get a clear canvas :)

                for (int i = 0; i < mainMenuText.Length; i++) //Print main menu
                {
                    Console.WriteLine(mainMenuText[i]);
                }

                Console.Write("\n[*] Choice: ");

                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("\n[!] That Is Not A Number!");
                    Thread.Sleep(1500);
                    continue;
                }

                switch (choice - 1)
                {
                    case 0: //Log In
                        {
                            Console.Clear();

                            Console.WriteLine("[-] Login");
                            Console.Write("[*] Please Enter Your Username: ");
                            string username = Console.ReadLine();
                            Console.Write("[*] Please Enter Your Password: ");
                            string password = GetPassword();

                            request = new RestRequest("login"); //Make request to http://<serverIP>:<serverPort>/login
                            request.AddParameter("username", username); //Adds To POST Or URL Querystring Based On Method
                            request.AddParameter("password", password);

                            //Add HTTP Headers
                            request.AddHeader("ContentType", "application/x-www-form-urlencoded");

                            //Execute The Request
                            response = client.Post(request);

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                loggedIn = true;
                                loggedInUsername = username;
                                Console.WriteLine("\n\n[i] Logged In Successfully!");
                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                Console.ReadLine();

                                while (loggedIn)
                                {
                                    Console.Clear();

                                    for (int i = 0; i < loggedInMenuText.Length; i++) //Prints logged in menu
                                    {
                                        Console.WriteLine(loggedInMenuText[i]);
                                    }

                                    Console.Write("\n[*] Choice: ");

                                    if (!int.TryParse(Console.ReadLine(), out choice))
                                    {
                                        Console.WriteLine("\n[!] That Is Not A Number!");
                                        Thread.Sleep(1500);
                                        continue;
                                    }

                                    switch (choice - 1)
                                    {
                                        case 0: //Play game
                                            {
                                                //game logic code goes here
                                                break;
                                            }

                                        case 1: //How to play
                                            {
                                                //print how to play here
                                                break;
                                            }

                                        case 97:
                                            {
                                                Console.Clear();

                                                for (int i = 0; i < changeAccountDetailsMenuText.Length; i++) //Print main menu
                                                {
                                                    Console.WriteLine(changeAccountDetailsMenuText[i]);
                                                }

                                                Console.Write("\n[*] Choice: ");

                                                if (!int.TryParse(Console.ReadLine(), out choice))
                                                {
                                                    Console.WriteLine("\n[!] That Is Not A Number!");
                                                    Thread.Sleep(1500);
                                                    continue;
                                                }

                                                switch (choice - 1)
                                                {
                                                    case 0:
                                                        {
                                                            Console.Clear();

                                                            Console.WriteLine("[-] Changing Username");
                                                            Console.Write("[*] Please Enter Your New Username: ");
                                                            string newUsername = Console.ReadLine();

                                                            request = new RestRequest($"user/{loggedInUsername}/"); //Make request to http://<serverIP>:<serverPort>/user/<loggedInUsername>
                                                            request.AddParameter("username", newUsername); //Adds To POST Or URL Querystring Based On Method

                                                            //Add HTTP Headers
                                                            request.AddHeader("ContentType", "application/x-www-form-urlencoded");

                                                            //Execute The Request
                                                            response = client.Post(request);

                                                            if (response.StatusCode == HttpStatusCode.OK) //If everything goes smooth and account doesn't already exist in database
                                                            {
                                                                Console.WriteLine("\n\n[i] Account Details Edited Successfully!");
                                                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                                                Console.ReadLine();
                                                            }
                                                            else if (response.StatusCode == HttpStatusCode.Forbidden) //If the account username already exists
                                                            {
                                                                Console.WriteLine("\n\n[!] Error Editing Account Details! You May Somehow Not Be Signed In. Please Restart The Program.");
                                                                Console.WriteLine($"Server Response Code: {response.StatusCode.ToString()}");
                                                                Console.WriteLine($"HTML Response: {response.Content}");
                                                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                                                Console.ReadLine();
                                                            }
                                                            break;
                                                        }

                                                    case 1:
                                                        {
                                                            Console.Clear();
                                                            Console.WriteLine("[-] Changing Password");

                                                            Console.Write("[*] Please Enter Your New Password: ");
                                                            string newPassword = GetPassword();


                                                            request = new RestRequest($"user/{loggedInUsername}/"); //Make request to http://<serverIP>:<serverPort>/register
                                                            request.AddParameter("password", newPassword); //Adds To POST Or URL Querystring Based On Method

                                                            //Add HTTP Headers
                                                            request.AddHeader("ContentType", "application/x-www-form-urlencoded");

                                                            //Execute The Request
                                                            response = client.Post(request);

                                                            if (response.StatusCode == HttpStatusCode.OK) //If everything goes smooth and account doesn't already exist in database
                                                            {
                                                                Console.WriteLine("\n\n[i] Account Details Edited Successfully!");
                                                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                                                Console.ReadLine();
                                                            }
                                                            else if (response.StatusCode == HttpStatusCode.Forbidden) //If the account username already exists
                                                            {
                                                                Console.WriteLine("\n\n[!] Error Editing Account Details! You May Somehow Not Be Signed In. Please Restart The Program.");
                                                                Console.WriteLine($"Server Response Code: {response.StatusCode.ToString()}");
                                                                Console.WriteLine($"HTML Response: {response.Content}");
                                                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                                                Console.ReadLine();
                                                            }
                                                            break;
                                                        }

                                                    case 2:
                                                        {
                                                            Console.Clear();
                                                            Console.WriteLine("[-] Changing Username And Password");

                                                            Console.Write("[*] Please Enter Your New Username: ");
                                                            string newUsername = Console.ReadLine();

                                                            Console.Write("[*] Please Enter Your New Password: ");
                                                            string newPassword = GetPassword();

                                                            request = new RestRequest($"user/{loggedInUsername}/"); //Make request to http://<serverIP>:<serverPort>/register
                                                            request.AddParameter("username", newUsername); //Adds To POST Or URL Querystring Based On Method
                                                            request.AddParameter("password", newPassword);

                                                            //Add HTTP Headers
                                                            request.AddHeader("ContentType", "application/x-www-form-urlencoded");

                                                            //Execute The Request
                                                            response = client.Post(request);

                                                            if (response.StatusCode == HttpStatusCode.OK) //If everything goes smooth and account doesn't already exist in database
                                                            {
                                                                Console.WriteLine("\n\n[i] Account Details Edited Successfully!");
                                                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                                                Console.ReadLine();
                                                            }
                                                            else if (response.StatusCode == HttpStatusCode.Forbidden) //If the account username already exists
                                                            {
                                                                Console.WriteLine("\n\n[!] Error Editing Account Details! You May Somehow Not Be Signed In. Please Restart The Program.");
                                                                Console.WriteLine($"Server Response Code: {response.StatusCode.ToString()}");
                                                                Console.WriteLine($"HTML Response: {response.Content}");
                                                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                                                Console.ReadLine();
                                                            }
                                                            break;
                                                        }
                                                    case 3:
                                                        break;

                                                    default:
                                                        Console.WriteLine("\n[!] That Is Not A Valid Choice!");
                                                        Thread.Sleep(1500);
                                                        break;
                                                }

                                                break;
                                            }

                                        case 98: //Logout
                                            {
                                                request = new RestRequest("logout"); //Make request to http://<serverIP>:<serverPort>/logout

                                                //Execute The Request
                                                response = client.Get(request);

                                                if (response.StatusCode == HttpStatusCode.OK)
                                                {
                                                    Console.WriteLine("\n[i] User Logged Out Successfully");
                                                    Console.WriteLine("\n[*] Press Enter To Continue...");
                                                    Console.ReadLine();
                                                    loggedInUsername = "";
                                                    loggedIn = false;
                                                }
                                                else if (response.StatusCode == HttpStatusCode.Forbidden) //If user isn't already logged in (this should never get hit, it's just a failsafe)
                                                {
                                                    Console.WriteLine("[!] You are somehow not already logged in? Please restart the program.");
                                                }
                                                break;
                                            }
                                    }
                                }
                            }

                            else if (response.StatusCode == HttpStatusCode.Forbidden) //If the provided credentials are wrong
                            {
                                Console.WriteLine("\n\n[!] Invalid Credentials. Please Try Again");
                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                Console.ReadLine();
                            }
                            break;
                        }

                    case 1: //Registering
                        {
                            Console.Clear();

                            Console.WriteLine("[-] Registering");
                            Console.Write("Please Enter Your Username: ");
                            string username = Console.ReadLine();
                            Console.Write("Please Enter Your Password: ");
                            string password = GetPassword();

                            request = new RestRequest("register"); //Make request to http://<serverIP>:<serverPort>/register
                            request.AddParameter("username", username); //Adds To POST Or URL Querystring Based On Method
                            request.AddParameter("password", password);

                            //Add HTTP Headers
                            request.AddHeader("ContentType", "application/x-www-form-urlencoded");

                            //Execute The Request
                            response = client.Post(request);

                            if (response.StatusCode == HttpStatusCode.OK) //If everything goes smooth and account doesn't already exist in database
                            {
                                Console.WriteLine("\n\n[i] Account Created Successfully!");
                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                Console.ReadLine();
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden) //If the account username already exists
                            {
                                Console.WriteLine("\n\n[!] Error Creating Account! Account Username Probably Already Exists. Try Again.");
                                Console.WriteLine($"Server Response Code: {response.StatusCode.ToString()}");
                                Console.WriteLine($"HTML Response: {response.Content}");
                                Console.WriteLine("\n[*] Press Enter To Continue...");
                                Console.ReadLine();
                            }
                            break;
                        }

                    case 98: //Exit
                        running = false; //Just exits the while loop to end the program (I could change this to just ``return`` and remove the running bool
                        break;

                    default:
                        Console.WriteLine("\n[!] That Is Not A Valid Choice!");
                        Thread.Sleep(1500);
                        break;
                }

            }
        }

        public static void getServerInfoFromConfig() //Sets the serverIP and serverPort variables to the values found in Config.xml
        {
            if (File.Exists("Config.xml"))
            {
                XmlDocument configXmlDoc = new XmlDocument();
                configXmlDoc.Load("Config.xml");

                XmlNode config = configXmlDoc.SelectSingleNode("/config");

                serverIP = config["serverIP"].InnerText;

                if (!int.TryParse(config["serverPort"].InnerText, out serverPort))
                {
                    Console.WriteLine("[!] Server Port In Config File Appears To Not Be An Integer, Please Fix.");
                    Console.WriteLine("\n [*] Press Enter To Exit...");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("[!] Config File Doesn't Exist! Creating It For You Mow...");

                XmlWriter xmlWriter = XmlWriter.Create("Config.xml");

                Console.Write("[*] Server IP: ");
                serverIP = Console.ReadLine();

                while (true)
                {
                    Console.Clear();

                    Console.WriteLine("[!] Config File Doesn't Exist! Creating It For You Now...");
                    Console.WriteLine($"[-] Server IP: {serverIP}");

                    Console.Write("[*] Server Port: ");
                    string serverPortInput = Console.ReadLine();
                    if (!int.TryParse(serverPortInput, out serverPort))
                    {
                        Console.WriteLine("\n\n[!] That Is Not A Number!");
                        Thread.Sleep(1500);
                        continue;
                    }

                    Console.WriteLine("[!] Config File Doesn't Exist! Creating It For You Now...");
                    Console.WriteLine($"[-] Server IP: {serverIP}");
                    Console.WriteLine($"[-] Server Port: {serverPort}");

                    break;
                }

                xmlWriter.WriteStartDocument();

                xmlWriter.WriteStartElement("config");

                xmlWriter.WriteStartElement("serverIP");
                xmlWriter.WriteString(serverIP);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("serverPort");
                xmlWriter.WriteString(serverPort.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
        }

        public static string GetPassword() //Allows Users To Input Their Password And Have The Characters Hidden By Asterkisks For Increased Security
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
