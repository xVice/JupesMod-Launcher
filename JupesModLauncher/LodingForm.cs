using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using ReaLTaiizor.Forms;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design.Behavior;

namespace JupesModLauncher
{


    public partial class LodingForm : Form
    {
        //Imagine if the engine code base was this good! :C
        public LodingForm()
        {
            DoubleBuffered = true;
            InitializeComponent();
        }

        private void LodingForm_Load(object sender, EventArgs e)
        {

            StartLaunching();

        }

        LauncherConfig config = null;

        public string GetVersion()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string url = "https://raw.githubusercontent.com/xVice/JupeModUpdateFiles/main/version.txt";
                    string version = client.DownloadString(url);
                    return version;
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the web request.
                    // You may want to log the error or handle it differently.
                    return "Error: " + ex.Message;
                }
            }
        }

        public void StartLaunching()
        {
            MakeTempDir();
           
            if (EvaluteFirstLaunch())
            {
            
                Log("First Launch, creating config..");
                config = new LauncherConfig();
                config.Version = GetVersion();
                config.InstallDir = GetInstallDir();
                File.WriteAllText("./config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
                AfterConfig();
            }
            else
            {
                AfterConfig();
            }
        }

        private void AfterConfig()
        {
            Log("Found config, reading it..");
            config = JsonConvert.DeserializeObject<LauncherConfig>(File.ReadAllText("./config.json"));
            string exePath = Path.Combine(config.InstallDir, "JupesMod.exe");

            if (config.Version.Equals(GetVersion()))
            {
                if (File.Exists(exePath))
                {
                    Log("Found game exe, launching..");
                    StartGameExitLauncher();
                }
                else
                {
                    Log("Game exe not found.");
                    Log("Requesting folder clear..");
                    DialogResult msgBox = MessageBox.Show($"Do you want to delete the contents of {config.InstallDir} and download the game files?");


                    if (msgBox == DialogResult.OK)
                    {
                        Directory.Delete(config.InstallDir, true);
                        Directory.CreateDirectory(config.InstallDir);
                        DownloadGameFilesAsync(config.InstallDir);
                    }
                }
            }
            else
            {
                Log("New Version found.");
                Log("Requesting folder clear..");
                DialogResult msgBox = MessageBox.Show($"Do you want to delete the contents of {config.InstallDir} and download the new game files?");


                if (msgBox == DialogResult.OK)
                {
                    Directory.Delete(config.InstallDir, true);
                    Directory.CreateDirectory(config.InstallDir);
                    DownloadGameFilesAsync(config.InstallDir);
                }
            }


        }

        private static void MakeTempDir()
        {
            if (Directory.Exists("./temp"))
            {
                Directory.Delete("./temp", true);
                Directory.CreateDirectory("./temp");
            }
            else
            {
                Directory.CreateDirectory("./temp");
            }
        }

        private async Task DownloadGameFilesAsync(string targetPath)
        {
            string url = "https://github.com/xVice/JupeModUpdateFiles/raw/main/jmod.7z";
            Log("Downloading game files..");

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    long totalBytes = response.Content.Headers.ContentLength ?? -1;
                    long downloadedBytes = 0;

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        
                        using (FileStream fileStream = File.Create("./temp/jmod.7z"))
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                downloadedBytes += bytesRead;
                                
                                // Calculate progress and update the UI asynchronously
                                int progress = (int)((double)downloadedBytes / totalBytes * 100);
                                InvokeOnUiThread(() => UpdateProgressBar(progress));
                                Application.DoEvents();
                            }
                        }
                    }

                    Log("Downloaded latest game files..");

                    Log("Unzipping");

                    await Task.Run(() =>
                    {
                        var archive = ArchiveFactory.Open("./temp/jmod.7z");
                        int totalEntries = archive.Entries.Count();
                        int completedEntries = 0;

                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                InvokeOnUiThread(() => Log($"Unpacking: {Path.GetFileNameWithoutExtension(entry.Key)}"));
                                entry.WriteToDirectory(targetPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                            }

                            completedEntries++;

                            // Calculate progress and update the UI asynchronously
                            int progress = (int)((double)completedEntries / totalEntries * 100);
                            InvokeOnUiThread(() => UpdateProgressBar(progress));
                            Application.DoEvents();
                        }
                    });

                    Log("Unzipped");

                    StartGameExitLauncher();
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");
                }
            }
        }

        public void StartGameExitLauncher()
        {
            Log("Starting and exiting launcher..");

            string jupesModPath = Path.Combine(config.InstallDir, "JupesMod.exe");

            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = jupesModPath,
                    WorkingDirectory = config.InstallDir // Set the working directory to config.InstallDir
                });
                Application.Exit();
            }
            catch (Exception ex)
            {
                Log($"Error starting process: {ex.Message}");
            }
        }



        private void UpdateProgressBar(int value)
        {
            hopeProgressBar1.ValueNumber = value;
        }

        private void InvokeOnUiThread(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }

        private string GetInstallDir()
        {
            string installDir;

            do
            {
                installDir = AskForFolder();
            } while (string.IsNullOrEmpty(installDir));

            return installDir;
        }


        private string AskForFolder()
        {
            using (System.Windows.Forms.OpenFileDialog folderBrowser = new System.Windows.Forms.OpenFileDialog())
            {
                // Set the dialog properties
                folderBrowser.Title = "Select a folder";
                folderBrowser.CheckFileExists = false;
                folderBrowser.ValidateNames = false;
                folderBrowser.FileName = "Folder Selection";

                // Show the folder browser dialog and capture the result
                if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Get the selected folder path
                    string selectedPath = System.IO.Path.GetDirectoryName(folderBrowser.FileName);
                    return selectedPath;
                }
                else
                {
                    return string.Empty;
                }
            }
        }


        private bool EvaluteFirstLaunch()
        {
            if (!File.Exists("./config.json"))
            {
                return true;
            }
            return false;
        }





        public void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm");
            string log = $"[{timestamp}] - {message}\n";

            Console.WriteLine(log);
            richTextBox1.AppendText(log); // Use AppendText to add text
            hopeForm1.Text = "Jupes Mod Launcher - " + message;
            hopeForm1.Refresh();

            richTextBox1.ScrollToBottom();
        }

        private void hopeProgressBar1_Click(object sender, EventArgs e)
        {

        }
    }


    public class LauncherConfig
    {
        public string InstallDir { get; set; }

        public string[] CustomInstallDirs { get; set; }
        public bool FastLaunch { get; set; }
        public bool IsServer { get; set; }
        public bool IsHeadless { get; set; }
        public bool IsDebug { get; set; }
        public string Version { get; set; }

    }

    public static class RichTextBoxUtils
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int SendMessage(System.IntPtr hWnd, int wMsg, System.IntPtr wParam, System.IntPtr lParam);

        private const int WM_VSCROLL = 0x115;
        private const int SB_BOTTOM = 7;

        /// <summary>
        /// Scrolls the vertical scroll bar of a text box to the bottom.
        /// </summary>
        /// <param name="tb">The text box base to scroll</param>
        public static void ScrollToBottom(this System.Windows.Forms.TextBoxBase tb)
        {
            if (System.Environment.OSVersion.Platform != System.PlatformID.Unix)
                SendMessage(tb.Handle, WM_VSCROLL, new System.IntPtr(SB_BOTTOM), System.IntPtr.Zero);
        }

    }

    class Compiler
    {
        private string exportDir = string.Empty;

        public Compiler(string exportDir)
        {
            this.exportDir = exportDir;
        }

        public bool CompileToExe(string entryFilePath, string outputExeName)
        {
            // Read the code from the entry file
            string code = File.ReadAllText(entryFilePath);

            // Create a syntax tree from the code
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            // Define compilation options
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

            // Define references (assuming Luminosity3D.dll is in the same directory)
            MetadataReference[] references = { MetadataReference.CreateFromFile($"{exportDir}/Luminosity3D.dll") };

            // Create a compilation
            CSharpCompilation compilation = CSharpCompilation.Create(outputExeName,
                new[] { syntaxTree },
                references,
                compilationOptions);

            // Define a memory stream to store the generated executable
            using (var ms = new MemoryStream())
            {
                // Emit the compilation result into the memory stream
                EmitResult result = compilation.Emit(ms);

                if (result.Success)
                {
                    // Save the generated executable to a file
                    File.WriteAllBytes(outputExeName, ms.ToArray());

                    Console.WriteLine("Executable generated successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Compilation failed:");
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.WriteLine(diagnostic);
                    }
                    return false;
                }
            }
        }
    }

}
