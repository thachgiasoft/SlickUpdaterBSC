﻿/* THIS FILE IS INCOMPLETE
 * PLEASE FINISH BEFORE PULL!!!!
 * TODO:
 * Figure out why the file wont extract/update to the right folder
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Net;
using System.ComponentModel;
using SevenZip;
using System.Threading;

namespace SlickUpdater {

    public static class a2UpdateManager {
        public static string arma2Path = regcheck.arma2RegCheck();
        static Queue<string> queue = new Queue<string>();
        static public bool isUpdateStarted;
        static string url = "";
        static string modlist = "modlist.cfg";
        public static bool a2UpdateComplete;
        static int updateProgress;
        static int totalFiles;


        public static void arma2UpdateCheck() {
            string mod;
            int index;
            string[] mods;
            string modFolder;
            string versionFile;
            string versionString;
            string version0String;
            WebClient client = new WebClient();
            string xmlLine = Properties.Settings.Default.A2repo;
            string slickVersion = downloader.webRead("http://projectawesomemodhost.com/beta/repo/slickupdater/slickversion");
            
#if DEBUG
            //xmlLine = "http://localhost/repo/";
            slickVersion = downloader.webRead("http://localhost/repo/slickupdater/slickversion");
#endif
             
            string[] parsedslickVersion = slickVersion.Split('%');
            if (xmlLine != "")
            {
                url = xmlLine;
            }else{
                MessageBox.Show("Your repourl is not set. Go into settings and change it! Setting it to default!");
                url = parsedslickVersion[5];
                Properties.Settings.Default.A2repo = parsedslickVersion[5];
            }
            

            BitmapImage modRed = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modRed.png"));
            BitmapImage modGreen = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modGreen.png"));
            BitmapImage modBlue = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modBlue.png"));
            BitmapImage modBrown = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modBrown.png"));
            BitmapImage modYellow = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modYellow.png"));
            List<Mod> a2Items = new List<Mod>();
            try {
                mods = downloader.webReadLines(url + modlist);
            } catch (WebException e) {
                WindowManager.mainWindow.checkWorker.ReportProgress(-1, e.Message);
                return;
            }
            bool date = true;
            foreach (string modline in mods) {
                index = modline.IndexOf("#");
                if (index != 0) {
                    if (index != -1) {
                        mod = modline.Substring(0, index);
                    } else {
                        mod = modline;
                    }
                    modFolder = arma2Path + "\\" + mod;
                    versionFile = arma2Path + "\\" + mod + "\\SU.version";
                    version0String = downloader.webRead(url + "/" + mod + "/" + "SU.version");
                    if (Directory.Exists(modFolder)) {
                        if (File.Exists(versionFile)) {
                            versionString = File.ReadAllText(versionFile);
                            if (versionString == version0String) {
                                modGreen.Freeze();
                                a2Items.Add(new Mod() { status = modGreen, modName = mod, version = "v. " + versionString, servVersion = "v. " + version0String });
                                logIt.addData(mod + " is up to date.");
                                //MessageBox.Show(mod + " is up to date.");
                            } else {
                                modYellow.Freeze();
                                a2Items.Add(new Mod() { status = modYellow, modName = mod, version = "v. " + versionString, servVersion = "v. " + version0String });
                                date = false;
                                //MessageBox.Show(mod + " is out of date.");
                                logIt.addData(mod + " is out to date.");
                            }
                        } else {
                            modBrown.Freeze();
                            a2Items.Add(new Mod() { status = modBrown, modName = mod, version = "No file", servVersion = "v. " + version0String });
                            date = false;
                            //MessageBox.Show(mod + " is missing a version file.");
                            logIt.addData(mod + " is missing a version file.");
                        }
                    } else {
                        modBlue.Freeze();
                        a2Items.Add(new Mod() { status = modBlue, modName = mod, version = "No file", servVersion = "v. " + version0String });
                        //File.Delete(versionFile);
                        date = false;
                        //MessageBox.Show(mod + " doesn't exist on your computer.");
                        logIt.addData(mod + " doesn't exist on your computer.");
                    }
                }
            }
            if (date == true) {
                WindowManager.mainWindow.checkWorker.ReportProgress(1, "Launch ArmA 2");
            } else {
                WindowManager.mainWindow.checkWorker.ReportProgress(1, "Update ArmA 2");
            }
            WindowManager.mainWindow.checkWorker.ReportProgress(2, a2Items);
        }


        public static void a2Update() {
            if (url == ""){
                url = Properties.Settings.Default.A2repo;
            }

            string arma2Path = regcheck.arma2RegCheck();
            string mod;
            string[] mods;
            string modFolder;
            string versionString = "";
            string version0String = "";
            WebClient client = new WebClient();

            mods = downloader.webReadLines(url + modlist);
            int i = 0;
            foreach (string modline in mods) {
                i++;
                int index = modline.IndexOf("#");
                if (index != 0) {
                    if (index != -1) {
                        mod = modline.Substring(0, index);
                    } else {
                        mod = modline;
                    }
                    modFolder = arma2Path + "\\" + mod;
                    if (Directory.Exists(modFolder)) {
                        string versionFile = arma2Path + "\\" + mod + "\\" + "SU.version";
                        string version0File = "SU.version";
                        if (File.Exists(versionFile)) {
                            
                            versionString = File.ReadAllText(versionFile);
                            version0String = downloader.webRead(url + mod + "\\" + version0File);
                            logIt.addData("Fetched versionfile from server version is " + versionString);
                            File.Delete(version0File);
                            if (versionString == version0String) {
                                //a2Items.Add(new Mod() { status = modGreen, modName = mod });
                                //MessageBox.Show(mod + " is up to date.");
                            } else {
                                //a2Items.Add(new Mod() { status = modYellow, modName = mod });
                                //MessageBox.Show(mod + " is out of date.");
                                a2DetailUpdate(mod, client);
                            }
                        } else {
                            //a2Items.Add(new Mod() { status = modBrown, modName = mod });
                            //MessageBox.Show(mod + " is missing a version file.");
                            version0String = downloader.webRead(url + mod + "\\" + version0File);
                            MessageBoxResult result = MessageBox.Show("SlickUpdater have detected that you have the folder " + modFolder + " if your 100% sure this is up to date you don't have to re-download. \n\nAre you sure this mod is up to date?", "Mod folder detacted", MessageBoxButton.YesNo);
                            switch (result)
                            {
                                case MessageBoxResult.Yes:
                                    File.WriteAllText(modFolder + "\\SU.version", version0String);
                                    break;
                                case MessageBoxResult.No:
                                    a2DetailUpdate(mod, client);
                                    break;
                            }
                        }
                    } else {
                        //a2Items.Add(new Mod() { status = modBlue, modName = mod });
                        //MessageBox.Show(mod + " doesn't exist on your computer.");
                        a2DetailUpdate(mod, client);
                    }
                }
                double status = (double)i / (double)mods.Length ;
                WindowManager.mainWindow.worker.ReportProgress((int)(status * 100) + 202);
            }
        }

        static private void increment() {
            double progress = ((double)++updateProgress / (double)totalFiles);
            WindowManager.mainWindow.worker.ReportProgress((int)(progress*100) + 101);                
        }

        static private void a2DetailUpdate (string mod, WebClient client) {
            string arma2Path = regcheck.arma2RegCheck();
            string modPath = arma2Path + "\\" + mod;
            
            Directory.CreateDirectory(modPath);

            updateProgress = 0;
            try {
                totalFiles = Convert.ToInt32(downloader.webRead(url + mod + "/count.txt"));
            } catch (WebException) {
                WindowManager.mainWindow.worker.ReportProgress(-1, "Web exception in reading " + url + mod + "/count.txt");
                return;
            } catch (Exception e) {
                WindowManager.mainWindow.worker.ReportProgress(-1, e.Message);
                return;
            }
            checkFilesFolders(modPath);

            downloader.download(url + mod + "/SU.version", client);
            File.Delete(modPath + "\\SU.version");
            File.Move("SU.version", modPath + "\\SU.version");
        }

        static private void checkFilesFolders(string folder) {
            string relativePath = folder.Replace(arma2Path, "");
            string[] files = downloader.webReadLines(url + relativePath + "/files.cfg");

            DirectoryInfo info = new DirectoryInfo(folder);

            string[] dirs = downloader.webReadLines(url + relativePath + "\\dirs.cfg");

            foreach (DirectoryInfo dirInfo in info.GetDirectories()) {
                bool exists = false;
                foreach (string dir in dirs) {
                    if (dir == dirInfo.Name) {
                        exists = true;
                    }
                }
                if (!exists) {
                    dirInfo.Delete(true);
                }
            }

            foreach (string dir in dirs) {
                DirectoryInfo dirInfo = new DirectoryInfo(folder + "\\" + dir);
                if (!dirInfo.Exists) {
                    dirInfo.Create();
                }
                checkFilesFolders(dirInfo.FullName);
            }

            foreach (FileInfo file in info.GetFiles()) {
                bool exists = false;
                foreach (string fileString in files) {
                    if (file.Name == fileString) {
                        exists = true;
                    }
                }
                if (exists == false) {
                    file.Delete();
                }
            }
            WebClient client = new WebClient();
            foreach (string file in files) {
                FileInfo fileInfo = new FileInfo(folder + "\\" + file);
                if (fileInfo.Exists) {
                    string hash = RepoGenerator.md5Calc(fileInfo.FullName);
                    string downloadedHash = downloader.webRead(url + relativePath + "\\" + fileInfo.Name + ".hash");
                    if (hash != downloadedHash) {
                        downloader.download(url + relativePath + "\\" + fileInfo.Name + ".7z", client);
                        Unzippy.extract(fileInfo.Name + ".7z", fileInfo.DirectoryName);
                        increment();
                        File.Delete(fileInfo.Name + ".7z");
                    }
                } else {
                    downloader.download(url + relativePath + "\\" + fileInfo.Name + ".7z", client);
                    Unzippy.extract(fileInfo.Name + ".7z", fileInfo.DirectoryName);
                    increment();
                    File.Delete(fileInfo.Name + ".7z");
                }
            }

            if (info.Name == "plugin") {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(info.FullName, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(info.FullName, Properties.Settings.Default.ts3Dir + "\\plugins"));

                //Copy all the files
                foreach (string newPath in Directory.GetFiles(info.FullName, "*.*", SearchOption.AllDirectories)) {
                    if (!File.Exists(newPath.Replace(info.FullName, Properties.Settings.Default.ts3Dir + "\\plugins")))
                    {
                        try {
                            File.Copy(newPath, newPath.Replace(info.FullName, Properties.Settings.Default.ts3Dir + "\\plugins"), true);
                            logIt.addData("Copied ACRE plugin to TS3 folder");
                        } catch (Exception e) {
                            WindowManager.mainWindow.worker.ReportProgress(-1, e.Message);
                            logIt.addData("Failed to copy ACRE plugin to TS3 folder. Error Message: " + e.Message);
                        }
                    }
                }
            }

            if (info.Name == "userconfig") {
                string output = arma2Path + "\\userconfig";
                Directory.CreateDirectory(output);

                foreach (string dirPath in Directory.GetDirectories(info.FullName, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(info.FullName, output));

                foreach (string newPath in Directory.GetFiles(info.FullName, "*.*",
                    SearchOption.AllDirectories))
                    try {
                        File.Copy(newPath, newPath.Replace(info.FullName, output), true);
                    } catch (Exception e) {
                        WindowManager.mainWindow.worker.ReportProgress(-1, e.Message);
                    }
            }
            
        }

        public static void downloadAsync (string url) {
            WebClient client = new WebClient();
            Uri uri = new Uri(url);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            try {
                client.DownloadFileAsync(uri, filename);
            } catch (ArgumentNullException e) {
                MessageBox.Show(e.Message);
                logIt.addData(e.Message);
            } catch (WebException e) {
                MessageBox.Show(e.Message + " on " + url);
                logIt.addData(e.Message + " on " + url);
            } catch (InvalidOperationException e) {
                MessageBox.Show(e.Message);
                logIt.addData(e.Message);
            }

        }



    }
}
