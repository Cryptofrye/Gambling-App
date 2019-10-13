using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Threading;
using RestSharp;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GamblingAppClient
{
    class Program
    {

        static string serverIP = "127.0.0.1"; //Default/Fallback Server IP
        static int serverPort = 1337; //Default/Fallback Server Port
        static RestClient client;
        static CookieContainer cookieContainer = new CookieContainer(); //CookieContainer so that all RestRequests use the same cookies (used for authentication with the server)
        static RestRequest request;
        static IRestResponse response;
        static string loggedInUsername = "";
        static string registeredAt = "";
        static float balance = 0;
        static bool loggedIn = false;

        static int choice = 0; //Used in menu inputs
        static bool running = true; //Used to escape the upper-most while loop (to exit program)
        static bool playingGame = false; //Used to stay inside the inner game menu so you don't have to navigate lots of submenus to keep betting

        static string[] arrQuitResponses = new string[5]
        {
                "q",
                "b",
                "quit",
                "exit",
                "leave"
        };

        static string[] arrMainMenuText = new string[3]
        {
            "[-] 1) Log In",
            "[-] 2) Register",
            "[-] 99) Quit"
        };

        static string strLoggedInMenuText =
@"[i] Logged In As: {0}
[i] Registered At: {1}
[i] Balance: {2}

[-] 1) Play Game
[-] 98) Change Account Details
[-] 99) Logout";

        static string strGamesMenuText =
@"[i] Logged In As: {0}
[i] Registered At: {1}
[i] Balance: {2}

[-] 1) Dice Game
[-] 2) Template Game (Not Implemented Yet)
[-] 99) Go Back";

        static string strDiceGameMenuText =
@"[i] Logged In As: {0}
[i] Registered At: {1}
[i] Balance: {2}

[-] 1) Play With Default Bet
[-] 2) Play With Custom Bet
[-] 3) How To Play
[-] 99) Go Back";

        static string[] arrChangeAccountDetailsMenuText = new string[4]
        {
            "[-] 1) Change Username",
            "[-] 2) Change Password",
            "[-] 3) Change Username & Password",
            "[-] 99) Go Back"
        };

        

        static void Main(string[] args)
        {

            getServerInfoFromConfig(); //Gets server IP and port information from the Config.xml file

            client = new RestClient($"http://{serverIP}:{serverPort}"); //Creates RestSharp Client That We Can Use To Make Web Requests (Prettier Than HttpWebRequest)
            client.CookieContainer = cookieContainer; //Sets the RestClient's cookie container to the global one so we can store cookies between requests

            while (!loggedIn && running)
            {

                mainMenuHandler();

            }
        }

        public static void mainMenuHandler()
        {
            Console.Clear(); //Get a clear canvas :)

            for (int i = 0; i < arrMainMenuText.Length; i++) //Print main menu
            {
                Console.WriteLine(arrMainMenuText[i]);
            }

            Console.Write("\n[*] Choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("\n[!] That Is Not A Number!");
                Thread.Sleep(1500);
                return;
            }

            switch (choice - 1)
            {
                case 0: //Log In
                    {
                        loginWrapper();

                        while (loggedIn)
                        {
                            loggedInMenuHandler();
                        }

                        break;
                    }

                case 1: //Registering
                    {
                        registerNewUserWrapper();
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

        public static void loggedInMenuHandler()
        {
            Console.Clear();

            request = new RestRequest($"users/user/{loggedInUsername}/"); //Make request to http://<serverIP>:<serverPort>/users/user/<loggedInUsername>

            //Execute The Request
            response = client.Get(request);

            if (response.StatusCode == HttpStatusCode.NotFound) //If user isn't already logged in (this should never get hit, it's just a failsafe)
            {
                Console.WriteLine("[!] Can't Get User Information! Account Doesn't Seem To Exist? Please Restart The Program.");
                return;
            }

            // Parsing JSON content into element-node JObject
            JObject jsonObject = JObject.Parse(response.Content);

            //Extracting Node element using Getvalue method
            registeredAt = jsonObject.GetValue("date_registered").ToString();
            balance = jsonObject.GetValue("money").ToObject<float>();

            Console.WriteLine(String.Format(strLoggedInMenuText, loggedInUsername, registeredAt, balance)); //This prints the strLoggedInMenuText with our variables in place of the placeholders

            Console.Write("\n[*] Choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("\n[!] That Is Not A Number!");
                Thread.Sleep(1500);
                return;
            }

            switch (choice - 1)
            {
                case 0: //Play game
                    {
                        //game logic code goes here
                        gameMenuHandler();
                        break;
                    }

                case 97: //Change acccount details
                    {
                        changeAccountDetailsHandler();
                        break;
                    }

                case 98: //Logout
                    {
                        if (logout()) //If successfully logged out
                        {
                            Console.WriteLine("\n[i] User Logged Out Successfully");
                            Console.WriteLine("\n[*] Press Enter To Continue...");
                            Console.ReadLine();
                            loggedInUsername = "";
                            loggedIn = false;
                        }
                        else
                        {
                            Console.WriteLine("\n[!] You are somehow not already logged in? Please restart the program.");
                            Console.WriteLine("\n[*] Press Enter To Continue...");
                            Console.ReadLine();
                        }

                        break;
                    }

                default:
                    Console.WriteLine("\n[!] That Is Not A Valid Choice!");
                    Thread.Sleep(1500);
                    break;
            }
        }

        public static void gameMenuHandler()
        {
            Console.Clear();

            Console.WriteLine(String.Format(strGamesMenuText, loggedInUsername, registeredAt, balance)); //This prints the strLoggedInMenuText with our variables in place of the placeholders

            Console.Write("\n[*] Choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("\n[!] That Is Not A Number!");
                Thread.Sleep(1500);
                return;
            }

            switch (choice - 1)
            {
                case 0: //Play dice game
                    {
                        playingGame = true;
                        while (playingGame)
                        {
                            diceGameMenuHandler();
                        }
                        break;
                    }

                case 1: //Blackjack probably
                    {
                        break;
                    }

                case 98: //Go back
                    {
                        break;
                    }

                default:
                    Console.WriteLine("\n[!] That Is Not A Valid Choice!");
                    Thread.Sleep(1500);
                    break;
            }
        }

        public static void diceGameMenuHandler()
        {
            Console.Clear();

            Console.WriteLine(String.Format(strDiceGameMenuText, loggedInUsername, registeredAt, balance)); //This prints the strLoggedInMenuText with our variables in place of the placeholders

            Console.Write("\n[*] Choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("\n[!] That Is Not A Number!");
                Thread.Sleep(1500);
                return;
            }

            switch (choice - 1)
            {
                case 0: //£0.20 bet
                    request = new RestRequest("game/dice/play/"); //Make request to http://<serverIP>:<serverPort>/register
                    request.AddParameter("amount", 0.2); //Adds To POST Or URL Querystring Based On Method

                    //Add HTTP Headers
                    request.AddHeader("ContentType", "application/x-www-form-urlencoded");

                    //Execute The Request
                    response = client.Post(request);

                    // Parsing JSON content into element-node JObject
                    JObject jsonObject = JObject.Parse(response.Content);

                    //Extracting Node element using Getvalue method
                    int dice1 = jsonObject.GetValue("dice1").ToObject<int>();
                    int dice2 = jsonObject.GetValue("dice2").ToObject<int>();
                    int dice3 = jsonObject.GetValue("dice3").ToObject<int>();
                    bool gameWon = jsonObject.GetValue("wonGame").ToObject<bool>();
                    float amountWon = jsonObject.GetValue("amountWon").ToObject<float>();
                    balance = jsonObject.GetValue("newBalance").ToObject<float>();

                    if (gameWon)
                    {
                        Console.WriteLine($"\n[i] You Bet £0.20 And Won £{amountWon}!");
                        Console.WriteLine($"[i] Dice 1: {dice1}");
                        Console.WriteLine($"[i] Dice 2: {dice2}");
                        Console.WriteLine($"[i] Dice 3: {dice3}");
                        Console.WriteLine($"[i] New Balance: {balance}");

                        Console.WriteLine("\n[*] Press Enter To Continue...");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine($"\n[i] You Bet £0.20 And Lost!");
                        Console.WriteLine($"[i] Dice 1: {dice1}");
                        Console.WriteLine($"[i] Dice 2: {dice2}");
                        Console.WriteLine($"[i] Dice 3: {dice3}");
                        Console.WriteLine($"[i] New Balance: {balance}");

                        Console.WriteLine("\n[*] Press Enter To Continue...");
                        Console.ReadLine();
                    }

                    break;

                case 1: //Custom bet
                    break;

                case 2:
                    howToPlay();
                    break;

                case 98: //Go back
                    playingGame = false;
                    break;

                default:
                    Console.WriteLine("\n[!] That Is Not A Valid Choice!");
                    Thread.Sleep(1500);
                    break;
            }
        }

        public static void howToPlay()
        {
            Console.Clear();

            Console.WriteLine("[-] How To Play");

            request = new RestRequest("game/dice/howtoplay/"); //Make request to http://<serverIP>:<serverPort>/logout

            //Execute The Request
            response = client.Get(request);

            // Parsing JSON content into element-node JObject
            JObject jsonObject = JObject.Parse(response.Content);

            //Extracting Node element using Getvalue method
            string instructions = jsonObject.GetValue("instructions").ToString();

            string[] lines = instructions.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );

            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine($"[i] {lines[i]}");
            }

            Console.WriteLine("[*] Press Enter To Continue...");
            Console.ReadLine();
        }

        public static void changeAccountDetailsHandler()
        {
            Console.Clear();

            for (int i = 0; i < arrChangeAccountDetailsMenuText.Length; i++) //Print change account details menu
            {
                Console.WriteLine(arrChangeAccountDetailsMenuText[i]);
            }

            Console.Write("\n[*] Choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("\n[!] That Is Not A Number!");
                Thread.Sleep(1500);
                return;
            }

            switch (choice - 1)
            {
                case 0: //Change username
                    {
                        changeUsernameWrapper();
                        break;
                    }

                case 1: //Changing password
                    {
                        changePasswordWrapper();
                        break;
                    }

                case 2: //Changing username and password
                    {
                        changeUsernameAndPasswordWrapper();
                        break;
                    }

                case 98: //Goes back
                    break;

                default:
                    Console.WriteLine("\n[!] That Is Not A Valid Choice!");
                    Thread.Sleep(1500);
                    break;
            }
        }

        public static void loginWrapper()
        {
            Console.Clear();

            Console.WriteLine("[-] Login");
            Console.WriteLine("[i] Leave Either Input Blank To Go Back...\n");

            Console.Write("[*] Please Enter Your Username: ");
            string usernameInput = Console.ReadLine();
            if (usernameInput.Length == 0) return;

            Console.Write("[*] Please Enter Your Password: ");
            string passwordInput = GetPassword();
            if (passwordInput.Length == 0) return;

            if (login(usernameInput, passwordInput))
            {
                loggedIn = true;
                loggedInUsername = usernameInput;
                Console.WriteLine("\n\n[i] Logged In Successfully!");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("\n\n[!] Invalid Credentials! Please Try Again.");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
            }
        }

        public static void registerNewUserWrapper()
        {
            Console.Clear();

            Console.WriteLine("[-] Registering");
            Console.WriteLine("[i] Leave Username Input Blank To Go Back...\n");

            Console.Write("[*] Please Enter Your Username: ");
            string usernameInput = Console.ReadLine();
            if (usernameInput.Length == 0) return; //Used to go back
            if (usernameInput.Length <= 4) //Make sure username meets requirements
            {
                Console.WriteLine("\n[!] Username Must Be Over 4 Characters Long!");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("[*] Please Enter Your Password: ");
            string passwordInput = GetPassword();
            if (passwordInput.Length == 0) return; //Used to go back
            if (passwordInput.Length <= 6) //Make sure password meets requirements
            {
                Console.WriteLine("\n\n[!] Password Must Be Over 6 Characters Long!");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
                return;
            }

            registerNewUser(usernameInput, passwordInput); //Makes RESTful API call to register a new user
        }

        public static void changeUsernameWrapper()
        {
            Console.Clear();

            Console.WriteLine("[-] Changing Username");
            Console.WriteLine("[i] Leave Username Input Blank To Go Back...\n");

            Console.Write("[*] Please Enter Your New Username: ");
            string newUsername = Console.ReadLine();
            if (newUsername.Length == 0) return; //Used to go back
            if (newUsername.Length <= 4 || newUsername == loggedInUsername) //Make sure their new username meets the requirements
            {
                Console.WriteLine("\n[!] New Username Must Be Over 4 Characters Long And Different To Current Username!");
                return;
            }

            if (changeUsername(loggedInUsername, newUsername)) //Makes RESTful API call to change username
            {
                loggedInUsername = newUsername;
                Console.WriteLine("\n[i] Account Details Edited Successfully!");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("\n[!] Error Editing Account Details! You May Somehow Not Be Signed In. Please Restart The Program.");
                Console.WriteLine($"[!] Server Response Code: {response.StatusCode.ToString()}");
                Console.WriteLine($"[!] HTML Response: {response.Content}");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
            }
        }

        public static void changePasswordWrapper()
        {
            Console.Clear();
            Console.WriteLine("[-] Changing Password");
            Console.WriteLine("[i] Leave Password Input Blank To Go Back...\n");

            Console.Write("[*] Please Enter Your New Password: ");
            string newPassword = GetPassword();
            if (newPassword.Length == 0) return; //Used to go back
            if (newPassword.Length <= 6) //Make sure their new password meets the requirements
            {
                Console.WriteLine("[!] New Password Must Be Over 6 Characters Long!");
                return;
            }

            if (changePassword(loggedInUsername, newPassword)) //Makes RESTful API call to change password
            {
                Console.WriteLine("\n\n[i] Account Details Edited Successfully!");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
            }

            else
            {
                Console.WriteLine("\n[!] Error Editing Account Details! You May Somehow Not Be Signed In. Please Restart The Program.");
                Console.WriteLine($"[!] Server Response Code: {response.StatusCode.ToString()}");
                Console.WriteLine($"[!] HTML Response: {response.Content}");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
            }
        }

        public static void changeUsernameAndPasswordWrapper()
        {
            Console.Clear();
            Console.WriteLine("[-] Changing Username And Password");
            Console.WriteLine("[i] Leave Either Input Blank To Go Back...\n");

            Console.Write("[*] Please Enter Your New Username: ");
            string newUsername = Console.ReadLine();
            if (newUsername.Length == 0) return; //Used to go back

            Console.Write("[*] Please Enter Your New Password: ");
            string newPassword = GetPassword();
            if (newPassword.Length == 0) return; //Used to go back

            //Makes RESTful API call to change username & password
            //We put loggedInUsername in the change username then newUsername in the changePassword as the changeUsername function is ran first, so the username has already changed by the time we run the changePassword function
            //
            if (changeUsername(loggedInUsername, newUsername) && changePassword(newUsername, newPassword))
            {
                loggedInUsername = newUsername;
                Console.WriteLine("\n\n[i] Account Details Edited Successfully!");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("\n\n[!] Error Editing Account Details! You May Somehow Not Be Signed In. Please Restart The Program.");
                Console.WriteLine($"[!] Server Response Code: {response.StatusCode.ToString()}");
                Console.WriteLine($"[!] HTML Response: {response.Content}");
                Console.WriteLine("\n[*] Press Enter To Continue...");
                Console.ReadLine();
            }
        }

        public static void registerNewUser(string username, string password)
        {

            request = new RestRequest("users/register/"); //Make request to http://<serverIP>:<serverPort>/register
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

            Console.WriteLine($"Server Response Code: {response.StatusCode.ToString()}");
            Console.WriteLine($"HTML Response: {response.Content}");
            Console.WriteLine("\n[*] Press Enter To Continue...");
            Console.ReadLine();
        }

        public static bool login(string username, string password)
        {
            request = new RestRequest("users/login/"); //Make request to http://<serverIP>:<serverPort>/login
            request.AddParameter("username", username); //Adds To POST Or URL Querystring Based On Method
            request.AddParameter("password", password);

            //Add HTTP Headers
            request.AddHeader("ContentType", "application/x-www-form-urlencoded");

            //Execute The Request
            response = client.Post(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            else if (response.StatusCode == HttpStatusCode.Forbidden) //If the provided credentials are wrong
            {
                return false;
            }

            return false;
        }

        public static bool logout()
        {
            request = new RestRequest("users/logout/"); //Make request to http://<serverIP>:<serverPort>/logout

            //Execute The Request
            response = client.Get(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden) //If user isn't already logged in (this should never get hit, it's just a failsafe)
            {
                return false;
            }

            return false;
        }

        public static bool changeUsername(string currentUsername, string newUsername)
        {
            request = new RestRequest($"users/user/{currentUsername}/"); //Make request to http://<serverIP>:<serverPort>/user/<currentUsername>
            request.AddParameter("username", newUsername); //Adds To POST Or URL Querystring Based On Method

            //Add HTTP Headers
            request.AddHeader("ContentType", "application/x-www-form-urlencoded");

            //Execute The Request
            response = client.Post(request);

            if (response.StatusCode == HttpStatusCode.OK) //If everything goes smooth and account doesn't already exist in database
            {
                return true;
                
            }
            else //If we run into an error
            {
                return false;
            }
        }

        public static bool changePassword(string currentUsername, string newPassword)
        {
            request = new RestRequest($"users/user/{currentUsername}/"); //Make request to http://<serverIP>:<serverPort>/user/<currentUsername>
            request.AddParameter("password", newPassword); //Adds To POST Or URL Querystring Based On Method

            //Add HTTP Headers
            request.AddHeader("ContentType", "application/x-www-form-urlencoded");

            //Execute The Request
            response = client.Post(request);

            if (response.StatusCode == HttpStatusCode.OK) //If everything goes smoothly
            {
                return true;
            }
            else //If we run into an error
            {
                return false;
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
