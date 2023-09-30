using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using JupesModLauncher;
using Newtonsoft.Json;


public static class Program
{
    static LodingForm loadingForm; // You need to declare your loading form here
    static LauncherConfig config; // Declare your config class here

    [STAThread]
    public static void Main()
    {

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LodingForm());

        /*
        // Start a separate thread for the loading form
        Thread loadingThread = new Thread(OpenLoadingForm);

        if (File.Exists("./config.json"))
        {
            
            config = JsonConvert.DeserializeObject<LauncherConfig>(File.ReadAllText("./config.json"));
            if (config.Version.Equals(GetVersion()))
            {
                if (File.Exists(Path.Combine(config.InstallDir, "JupesMod.exe")))
                {
                    string jupesModPath = Path.Combine(config.InstallDir, "JupesMod.exe");

                    // Set the working directory to the directory where JupesMod.exe is located
                    string workingDirectory = Path.GetDirectoryName(jupesModPath);

                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = jupesModPath,
                        WorkingDirectory = workingDirectory
                    });
                }

            }

        }
        else
        {
            loadingThread.ApartmentState = ApartmentState.STA;  
            loadingThread.Start();
            // You can optionally wait for the loading thread to finish before exiting the main thread
            loadingThread.Join();
            

        }
        config = JsonConvert.DeserializeObject<LauncherConfig>(File.ReadAllText("./config.json"));
        if (config.Version.Equals(GetVersion()))
        {
            if (File.Exists(Path.Combine(config.InstallDir, "JupesMod.exe")))
            {
                string jupesModPath = Path.Combine(config.InstallDir, "JupesMod.exe");

                // Set the working directory to the directory where JupesMod.exe is located
                string workingDirectory = Path.GetDirectoryName(jupesModPath);

                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = jupesModPath,
                    WorkingDirectory = workingDirectory
                });
            }

        }
        */
    }

 
}
