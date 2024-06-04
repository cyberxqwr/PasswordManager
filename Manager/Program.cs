using BCrypt.Net;
using Manager.Extras;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace Manager
{
    internal class Program
    {

        public readonly string loggedUser;
        public static bool loggedIn;
        public static string password = "";
        public static string username = "";

        private static List<PasswordData> passwordData = new List<PasswordData>();

        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        [STAThread]

        static void Main(string[] args)
        {

            int option;

            if (FileReader.FileExists("firstTime.txt"))
            {

                bool StartMenu = true;
                Console.WriteLine("Pasirinkite, ka norite daryti.");
                Console.WriteLine("1) Prisijungti.");
                Console.WriteLine("2) Registruotis.");
                while (StartMenu)
                {
                    Int32.TryParse(Console.ReadLine(), out option);
                    switch (option)
                    {

                        case 1:
                            Console.Clear();
                            Login();
                            break;
                        case 2:
                            Console.Clear();
                            Register();
                            break;
                        default:
                            Console.WriteLine("No such input.");
                            break;
                    };

                    if (loggedIn) StartMenu = false;
                }

            }
            else
            {
                Console.WriteLine("No account detected.");
                Register();
                FileReader.CreateFile("firstTime.txt");
            }

            bool LoggedMenu = true;


            while (LoggedMenu)
            {

                Console.Clear();
                Console.WriteLine($"Logged in: {username}");
                Console.WriteLine("Select method:");
                Console.WriteLine("1) Create a new password");
                Console.WriteLine("2) Search for a password");
                Console.WriteLine("3) Update a password");
                Console.WriteLine("4) Delete a password");

                Int32.TryParse(Console.ReadLine(), out option);
                switch (option)
                {

                    case 1:
                        Console.Clear();
                        Create();
                        break;
                    case 2:
                        Console.Clear();
                        Search();
                        break;
                    case 3:
                        Console.Clear();
                        Update();
                        break;
                    case 4:
                        Console.Clear();
                        Delete();
                        break;
                    default:
                        Console.WriteLine("No such input.");
                        break;
                };

            }

        }

        private static void Login()
        {


            Console.WriteLine("Enter your username.");
            string loginName = Console.ReadLine().ToLower();

            if (FileReader.FileExists($"{loginName}Data.txt"))
            {

                Console.WriteLine("Enter your password.");
                string psw = ReadLineMasked();

                if (BCrypt.Net.BCrypt.Verify(psw, AES.DecryptFile(FileReader.UserFile($"{loginName}Data.txt"))))
                {
                    Console.WriteLine("Login successful.");
                    loggedIn = true;
                    username = loginName;

                }
                else do
                    {

                        Console.WriteLine("Password incorrect.");

                        Console.WriteLine("Enter your password.");
                        psw = ReadLineMasked();

                        if (BCrypt.Net.BCrypt.Verify(psw, AES.DecryptFile(FileReader.UserFile($"{loginName}Data.txt"))))
                        {
                            Console.WriteLine("Login successful.");
                            loggedIn = true;
                            break;
                        }
                    } while (true);
            }
            else
            {

                Console.WriteLine("User not found.");
                Console.ReadKey();
                Console.Clear();
                Login();
            }

        }

        private static void Register()
        {

            // USN management

            Console.WriteLine("Please register.\n");
            Console.WriteLine("Enter your username.");
            username = Console.ReadLine().ToLower();

            if (username == null)
            {
                do
                {
                    Console.WriteLine("Invalid username.");
                    Console.WriteLine("\nEnter your username.");

                    if (username != null) break;
                }
                while (true);
            }

            // PSW management



            do
            {
                Console.WriteLine("\nEnter your password.");
                password = BCrypt.Net.BCrypt.HashPassword(ReadLineMasked());

                if (password != null) break;
                else Console.WriteLine("Invalid password.");
            }
            while (true);



            Console.Clear();

            using (var stream = File.Create($"{username}Data.txt")) { }
            using (var stream2 = File.Create($"{username}Passwords.txt")) { }

            AES.EncryptFile(password, FileReader.UserFile($"{username}Data.txt"));

            loggedIn = true;

        }

        private static string ReadLineMasked(char mask = '*')
        {
            var sb = new StringBuilder();
            ConsoleKeyInfo keyInfo;
            while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (!char.IsControl(keyInfo.KeyChar))
                {
                    sb.Append(keyInfo.KeyChar);
                    Console.Write(mask);
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);

                    if (Console.CursorLeft == 0)
                    {
                        Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                        Console.Write(' ');
                        Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                    }
                    else Console.Write("\b \b");
                }
            }
            Console.WriteLine();
            return sb.ToString();
        }

        private static void Create()
        {

            PasswordData psData = new PasswordData();
            string pName, pPassword, pURL, pComments;

            pPassword = null;
            pURL = null;

            Console.WriteLine("Enter a name for the listing.");
            do
            {
                pName = Console.ReadLine();
                if (pName != null) break;
            } while (true);

            psData.Name = pName;

            Console.WriteLine("Should the password be generated? Y/N");


            do
            {
                string opt = Console.ReadLine();
                switch (opt)
                {
                    case "y":
                    case "Y":
                        pPassword = CreateRandomPassword();
                        break;
                    case "n":
                    case "N":

                        do
                        {
                            Console.WriteLine("Enter your password.");
                            pPassword = ReadLineMasked();
                            if (pPassword != null) break;
                            else Console.WriteLine("Password is empty");
                        } while (true);
                        break;
                    default:
                        Console.WriteLine("Incorrect option.");
                        break;
                }

                if (pPassword != null) break;
            } while (true);

            psData.Password = AES.Encrypt(pPassword, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });

            Console.WriteLine("Enter website's URL");

            do
            {

                pURL = Console.ReadLine();
                if (pURL != null) break;
                else Console.WriteLine("URL is empty");
            } while (true);

            psData.URL = pURL;

            Console.WriteLine("Enter a comment (optional)");
            string pComment = Console.ReadLine();

            psData.Comment = pComment;



            if (FileReader.FileExists($"{username}Passwords.txt") && FileReader.ReadFileToString($"{username}Passwords.txt") == String.Empty)
            {

                passwordData.Add(psData);
                FileReader.WriteToFile($"{username}Passwords.txt", JSON.Serialize(passwordData));
            }
            else if (FileReader.ReadFileToString($"{username}Passwords.txt") != String.Empty)
            {

                passwordData = JSON.Deserialize<List<PasswordData>>(FileReader.ReadJSON($"{username}Passwords.txt"));
                passwordData.Add(psData);
                FileReader.WriteToFile($"{username}Passwords.txt", JSON.Serialize(passwordData));
            }



            passwordData.Clear();
        }

        private static void Search()
        {

            Console.WriteLine("Enter the name of the password.");
            int indexOf = 0;
            bool found = false;
            string lookup = Console.ReadLine();

            List<PasswordData> all = JSON.Deserialize<List<PasswordData>>(FileReader.ReadJSON($"{username}Passwords.txt"));
            foreach (PasswordData data in all)
            {
                if (data.Name.Contains(lookup))
                {
                    found = true;
                    indexOf = data.Name.IndexOf(lookup);
                    Console.WriteLine("Name: " + data.Name);
                    Console.WriteLine("Password: " + data.Password);
                    Console.WriteLine("URL: " + data.URL);
                    Console.WriteLine("Comment: " + data.Comment + "\n");
                }
            }

            string revealedPass = "";

            if (found)
            {
                string opt;
                bool revealed = false;
                Console.WriteLine("Do you want to view the orginal password? Y/N");
                do
                {
                    opt = Console.ReadLine();

                    switch (opt)
                    {
                        case "y":
                        case "Y":
                            revealedPass = AES.Decrypt(all[indexOf].Password, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });
                            Console.WriteLine(revealedPass);
                            revealed = true;
                            break;
                        case "n":
                        case "N":
                            revealed = true;
                            break;
                        default:
                            Console.WriteLine("Incorrect option.");
                            break;
                    }
                    if (revealed) break;
                } while (true);
            }

            if (revealedPass != String.Empty)
            {
                Console.WriteLine("Would you like to copy the password to your clipboard? Y/N");

                string opt;
                bool copied = false;
                do
                {
                    opt = Console.ReadLine();

                    switch (opt)
                    {
                        case "y":
                        case "Y":

                            OpenClipboard(IntPtr.Zero);
                            var ptr = Marshal.StringToHGlobalUni(revealedPass);
                            SetClipboardData(13, ptr);
                            CloseClipboard();
                            Marshal.FreeHGlobal(ptr);
                            copied = true;
                            break;
                        case "n":
                        case "N":
                            copied = true;
                            break;
                        default:
                            Console.WriteLine("Incorrect option.");
                            break;
                    }
                    if (copied) break;
                } while (true);
            }

            Console.ReadKey();

        }


        private static void Update()
        {

            Console.WriteLine("Enter the name of the password.");
            string lookup = Console.ReadLine();
            bool found = false;

            List<PasswordData> all = JSON.Deserialize<List<PasswordData>>(FileReader.ReadJSON($"{username}Passwords.txt"));
            foreach (PasswordData data in all)
            {
                if (data.Name.Contains(lookup))
                {
                    found = true;
                    Console.WriteLine("Enter a new password.");
                    string psw = Console.ReadLine();
                    data.Password = AES.Encrypt(psw, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });

                }
            }

            if (!found) Console.WriteLine("Password not found.");
            else
            {
                Console.WriteLine("Password has been updated.");
                FileReader.WriteToFile($"{username}Passwords.txt", JSON.Serialize(all));
            }

            Console.ReadKey();

        }

        private static void Delete()
        {

            Console.WriteLine("Enter the name of the password.");
            string lookup = Console.ReadLine();
            bool found = false;

            List<PasswordData> all = JSON.Deserialize<List<PasswordData>>(FileReader.ReadJSON($"{username}Passwords.txt"));
            for (int i = 0; i < all.Count(); i++)
            {
                if (all[i].Name.Contains(lookup))
                {
                    found = true;
                    all.Remove(all[i]);
                    break;
                }
            }

            if (!found) Console.WriteLine("Password not found.");
            else
            {
                Console.WriteLine("Password has been deleted.");
                FileReader.WriteToFile($"{username}Passwords.txt", JSON.Serialize(all));
            }

            Console.ReadKey();
        }
        private static string CreateRandomPassword()
        {

            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            char[] chars = new char[15];
            for (int i = 0; i < 15; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }
    }


}
