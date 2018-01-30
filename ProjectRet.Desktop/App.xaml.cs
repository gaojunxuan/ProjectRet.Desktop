using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation;

namespace ProjectRet.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            InstallProtocol();
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                Uri argUri;
                string command = "";
                string pass = "";
                if (Uri.TryCreate(args[1], UriKind.Absolute, out argUri))
                {
                    var decoder = new WwwFormUrlDecoder(argUri.Query);
                    if (decoder.Any())
                    {
                        foreach (var entry in decoder)
                        {
                            if (entry.Name == "comm")
                                command = entry.Value;
                            else if (entry.Name == "auth")
                                pass = entry.Value;
                        }
                    }
                    switch (command)
                    {
                        case "shutdown":
                            if (await PasscodeHelper.Auth(pass))
                                ShutdownHelper.ShutdownSystem();
                            break;
                        case "reboot":
                            if (await PasscodeHelper.Auth(pass))
                                ShutdownHelper.RebootSystem();
                            break;

                    }
                }
            }
        }
        private void InstallProtocol()
        {
            using (var hkcr = Registry.ClassesRoot)
            {
                if (hkcr.GetSubKeyNames().Contains("projectret"))
                {
                    return;
                }
                if (!PrivilegeHelper.IsRunAsAdmin())
                {
                    PrivilegeHelper.Elevate();
                    App.Current.Shutdown();
                }
                else
                {
                    using (var schemeKey = hkcr.CreateSubKey("projectret"))
                    {
                        schemeKey.SetValue(string.Empty, "Url: ProjectRet Protocol");
                        schemeKey.SetValue("URL Protocol", string.Empty);
                        schemeKey.SetValue("UseOriginalUrlEncoding", 1, RegistryValueKind.DWord);

                        //[HKEY_CLASSES_ROOT\com.aruntalkstech.wpf\shell]
                        using (var shellKey = schemeKey.CreateSubKey("shell"))
                        {
                            //[HKEY_CLASSES_ROOT\com.aruntalkstech.wpf\shell\open]
                            using (var openKey = shellKey.CreateSubKey("open"))
                            {
                                //[HKEY_CLASSES_ROOT\com.aruntalkstech.wpf\shell\open\command]
                                using (var commandKey = openKey.CreateSubKey("command"))
                                {
                                    commandKey.SetValue(string.Empty, Assembly.GetExecutingAssembly().Location + " %1");
                                    commandKey.Close();
                                }

                                openKey.Close();
                            }
                            shellKey.Close();
                        }
                        schemeKey.Close();
                    }
                }
                hkcr.Close();
            }
        }
    }
    
}
