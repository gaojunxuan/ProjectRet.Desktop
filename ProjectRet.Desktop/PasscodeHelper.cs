using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Web.Security;
using System.Security.Cryptography;
using System.Reflection;
using System.IO;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace ProjectRet.Desktop
{
    public class PasscodeHelper
    {
        private static string GeneratePassword()
        {
            string password = Membership.GeneratePassword(8,0);
            password = Regex.Replace(password, @"[^a-zA-Z0-9]", new Random().Next(0,9).ToString());
            return password;
        }
        private static string Base64Encode(string password)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(data);
        }
        private static string Base64Decode(string code)
        {
            byte[] data = Convert.FromBase64String(code);
            return System.Text.Encoding.UTF8.GetString(data);
        }
        internal static async void SavePassword(string password)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            using (StreamWriter outputFile = new StreamWriter(path + @"\credentials.txt", true))
            {
                await outputFile.WriteAsync(Base64Encode(password));
            }
        }
        internal static async Task<bool> Auth(string password,string key)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var system = await Windows.System.RemoteSystems.RemoteSystem.FindByHostNameAsync(new Windows.Networking.HostName("127.0.0.1"));
            if(system!=null)
            {
                if(string.Equals(system.Id,key))
                {
                    if (File.Exists(path + @"\credentials.txt"))
                    {
                        using (StreamReader sr = new StreamReader(path + @"\credentials.txt"))
                        {
                            // Read the stream to a string, and write the string to the console.
                            String line = await sr.ReadToEndAsync();
                            if (line == Base64Encode(password))
                                return true;
                        }
                    }
                }
            }            
            return false;
        }
        internal static async Task<string> GetPassword()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if(File.Exists(path + @"\credentials.txt"))
            {
                using (StreamReader sr = new StreamReader(path + @"\credentials.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = await sr.ReadToEndAsync();
                    return Base64Decode(line);
                }
            }
            else
            {
                string pass=GeneratePassword();
                SavePassword(pass);
                return pass;
            }
        }
    }
}
