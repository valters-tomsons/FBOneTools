﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using BFBC2_Toolkit.Data;
using BFBC2_Toolkit.Tools;
using BFBC2_Toolkit.Helpers;

namespace BFBC2_Toolkit.Functions
{
    public class SelectedFile
    {        
        public static async Task Export()
        {
            try
            {
                string selectedFilePath = String.Empty,
                       selectedFileName = String.Empty;

                if (Vars.IsDataTreeView)
                {
                    selectedFilePath = Dirs.SelectedFilePathData;
                    selectedFileName = Dirs.SelectedFileNameData;
                }
                else
                {
                    selectedFilePath = Dirs.SelectedFilePathMod;
                    selectedFileName = Dirs.SelectedFileNameMod;
                }

                if (selectedFilePath.EndsWith(".dbx"))
                {
                    string path = selectedFilePath.Replace(".dbx", ".xml"),
                           name = selectedFileName.Replace(".dbx", ".xml");

                    if (File.Exists(Dirs.OutputXML + @"\" + name))
                        await Task.Run(() => File.Delete(Dirs.OutputXML + @"\" + name));

                    await Task.Run(() => File.Copy(path, Dirs.OutputXML + @"\" + name));
                }
                else if (selectedFilePath.EndsWith(".itexture"))
                {
                    string path = selectedFilePath.Replace(".itexture", ".dds"),
                           name = selectedFileName.Replace(".itexture", ".dds");

                    if (File.Exists(Dirs.OutputDDS + @"\" + name))
                        await Task.Run(() => File.Delete(Dirs.OutputDDS + @"\" + name));

                    await Task.Run(() => File.Copy(path, Dirs.OutputDDS + @"\" + name));
                }
                else if (selectedFilePath.EndsWith(".ps3texture"))
                {
                    string path = selectedFilePath.Replace(".ps3texture", ".dds"),
                           name = selectedFileName.Replace(".ps3texture", ".dds");

                    if (File.Exists(Dirs.OutputDDS + @"\" + name))
                        await Task.Run(() => File.Delete(Dirs.OutputDDS + @"\" + name));

                    await Task.Run(() => File.Copy(path, Dirs.OutputDDS + @"\" + name));
                }
                else if (selectedFilePath.EndsWith(".terrainheightfield"))
                {
                    string path = selectedFilePath.Replace(".terrainheightfield", ".raw"),
                           name = selectedFileName.Replace(".terrainheightfield", ".raw");

                    if (File.Exists(Dirs.OutputHeightmap + @"\" + name))
                        await Task.Run(() => File.Delete(Dirs.OutputHeightmap + @"\" + name));

                    await Task.Run(() => File.Copy(path, Dirs.OutputHeightmap + @"\" + name));
                }
                else if (selectedFilePath.EndsWith(".binkmemory"))
                {
                    string name = selectedFileName.Replace(".binkmemory", ".bik");

                    if (File.Exists(Dirs.OutputVideo + @"\" + name))
                        await Task.Run(() => File.Delete(Dirs.OutputVideo + @"\" + name));

                    await Task.Run(() => File.Copy(selectedFilePath, Dirs.OutputVideo + @"\" + name));
                }
                else if (selectedFilePath.EndsWith(".swfmovie"))
                {
                    var process = Process.Start(Settings.PathToPython, "\"" + Dirs.ScriptSwfMovie + "\" \"" + selectedFilePath + "\" \"" + Dirs.OutputSwfMovie + "\"");
                    await Task.Run(() => process.WaitForExit());
                }
            }
            catch (Exception ex)
            {
                Write.ToErrorLog(ex);
                Write.ToEventLog("Unable to export file! See error.log", "error");
            }
        }

        public static async Task Import()
        {
            try
            {
                await MediaStream.Dispose();

                string selectedFilePath = String.Empty;

                var ctvi = new CustomTreeViewItem();

                if (Vars.IsDataTreeView)
                {
                    selectedFilePath = Dirs.SelectedFilePathData;

                    if (ctvi != null)
                        ctvi = UIElements.TreeViewDataExplorer.SelectedItem as CustomTreeViewItem;
                }
                else
                {
                    selectedFilePath = Dirs.SelectedFilePathMod;

                    if (ctvi != null)
                        ctvi = UIElements.TreeViewModExplorer.SelectedItem as CustomTreeViewItem;
                }

                if (selectedFilePath.EndsWith(".dbx"))
                {
                    var ofd = new OpenFileDialog();
                    ofd.Filter = "xml file (.xml)|*.xml";
                    ofd.Title = "Select xml file...";

                    if (ofd.ShowDialog() == true)
                    {
                        Write.ToEventLog("Importing file...", "");

                        string path = selectedFilePath.Replace(".dbx", ".xml");

                        if (File.Exists(path))
                            await Task.Run(() => File.Delete(path));

                        await Task.Run(() => File.Copy(ofd.FileName, path));

                        var process = Process.Start(Settings.PathToPython, "\"" + Dirs.ScriptDBX + "\" \"" + path.Replace(@"\", @"\\") + "\"");
                        await Task.Run(() => process.WaitForExit());

                        UIElements.TextEditor.Text = await Task.Run(() => File.ReadAllText(path));
                        UIElements.TextEditor.ScrollToHome();

                        Write.ToEventLog("", "done");
                    }
                }
                else if (selectedFilePath.EndsWith(".itexture"))
                {
                    var ofd = new OpenFileDialog();
                    ofd.Filter = "dds file (.dds)|*.dds";
                    ofd.Title = "Select dds file...";

                    if (ofd.ShowDialog() == true)
                    {
                        Write.ToEventLog("Importing file...", "");

                        string path = selectedFilePath.Replace(".itexture", ".dds");

                        if (File.Exists(path))
                            await Task.Run(() => File.Delete(path));

                        await Task.Run(() => File.Copy(ofd.FileName, path));

                        string[] file = { path };

                        await Task.Run(() => TextureConverter.ConvertFile(file, true, false));

                        try
                        {
                            UIElements.ImageElement.Source = BitmapHelper.LoadImage(path);
                        }
                        catch
                        {
                            UIElements.ImageElement.Visibility = Visibility.Hidden;

                            Write.ToEventLog("Unable to load texture preview! Exporting and importing should still work fine.", "warning");
                        }

                        Write.ToInfoBox(ctvi);
                        Write.ToEventLog("", "done");
                    }
                }
                else if (selectedFilePath.EndsWith(".bik"))
                {
                    var ofd = new OpenFileDialog();
                    ofd.Filter = "bik file (.bik)|*.bik";
                    ofd.Title = "Select bik file...";

                    if (ofd.ShowDialog() == true)
                    {
                        Write.ToEventLog("Importing file...", "");

                        await RenameToBik();

                        string path = selectedFilePath.Replace(".binkmemory", ".mp4");

                        if (File.Exists(path))
                            await Task.Run(() => File.Delete(path));

                        if (File.Exists(selectedFilePath.Replace(".binkmemory", ".bik")))
                            await Task.Run(() => File.Delete(selectedFilePath.Replace(".binkmemory", ".bik")));

                        await Task.Run(() => File.Copy(ofd.FileName, path));

                        if (File.Exists(selectedFilePath))
                            await Task.Run(() => File.Delete(selectedFilePath));

                        await Task.Run(() => File.Copy(ofd.FileName, selectedFilePath));

                        Play.Video(path);

                        Write.ToEventLog("", "done");
                    }
                }
            }
            catch (Exception ex)
            {
                Write.ToErrorLog(ex);
                Write.ToEventLog("Unable to import file! See error.log", "error");
            }
        }

        public static async Task DeleteFile(string explorer)
        {
            try
            {
                if (explorer == "data")
                {
                    if (!Vars.IsGameProfile)
                    {
                        if (UIElements.TreeViewDataExplorer.SelectedItem != null)
                        {
                            await Task.Run(() => File.Delete(Dirs.SelectedFilePathData));

                            var item = UIElements.TreeViewDataExplorer.SelectedItem as CustomTreeViewItem;
                            var parent = item.ParentItem;

                            parent.Items.Remove(item);
                        }
                    }
                    else
                    {
                        Write.ToEventLog("You can't delete a file from a game profile!", "warning");
                    }
                }
                else if (explorer == "mod")
                {
                    if (UIElements.TreeViewModExplorer.SelectedItem != null)
                    {
                        await Task.Run(() => File.Delete(Dirs.SelectedFilePathMod));

                        var item = UIElements.TreeViewModExplorer.SelectedItem as CustomTreeViewItem;
                        var parent = item.ParentItem;

                        parent.Items.Remove(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Write.ToErrorLog(ex);
                Write.ToEventLog("Unable to delete file! See error.log", "error");
            }
        }

        public static async Task RestoreFile()
        {
            try
            {
                await Task.Run(() => CleanUp.FilesAndDirs(Dirs.FilesPathMod));

                string filePathData = "";

                if (!Vars.IsGameProfile)
                {
                    foreach (KeyValuePair<string, string> kvp in Vars.FbrbFiles)
                    {
                        if (Dirs.FilePath.Contains(kvp.Value))
                        {
                            filePathData = Dirs.FilePath.Replace(kvp.Value + @"\", "");
                            break;
                        }
                    }
                }
                else
                {
                    filePathData = Dirs.FilePath;
                }

                filePathData = Dirs.FilesPathData + filePathData;

                if (File.Exists(filePathData))
                {
                    if (File.Exists(Dirs.SelectedFilePathMod))
                        await Task.Run(() => File.Delete(Dirs.SelectedFilePathMod));

                    await Task.Run(() => File.Copy(filePathData, Dirs.SelectedFilePathMod));
                }
                else
                {
                    MessageBox.Show("Unable to locate the original file!\nOpen the correct game profile or fbrb archive.", "Error!");
                }

                var item = UIElements.TreeViewModExplorer.SelectedItem as CustomTreeViewItem;

                item.IsSelected = false;
                item.IsSelected = true;
            }
            catch (Exception ex)
            {
                Write.ToErrorLog(ex);
                Write.ToEventLog("Unable to restore file! See error.log", "error");
            }
        }

        public static async Task CopyToMod()
        {
            try
            {
                await MediaStream.Dispose();

                await Task.Run(() => CleanUp.FilesAndDirs(Dirs.FilesPathMod));

                string filePathMod = Dirs.FilesPathMod + @"\" + Dirs.FilePath;

                if (!Vars.IsGameProfile)
                {
                    foreach (KeyValuePair<string, string> kvp in Vars.FbrbFiles)
                    {
                        if (Dirs.FilesPathData.EndsWith(kvp.Value))
                        {
                            filePathMod = Dirs.FilesPathMod + @"\" + kvp.Value + @"\" + Dirs.FilePath;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (string gameId in Vars.GameIds)
                    {
                        if (filePathMod.Contains(gameId))
                        {
                            filePathMod = filePathMod.Replace(gameId + @"\", "");
                            break;
                        }
                    }
                }

                if (File.Exists(filePathMod))
                    await Task.Run(() => File.Delete(filePathMod));

                var fileInfo = new FileInfo(filePathMod);
                await Task.Run(() => fileInfo.Directory.Create());

                await Task.Run(() => File.Copy(Dirs.SelectedFilePathData, filePathMod));

                Tree.Populate(UIElements.TreeViewModExplorer, Dirs.FilesPathMod);
            }
            catch (Exception ex)
            {
                Write.ToErrorLog(ex);
                Write.ToEventLog("Unable to copy file to mod project! See error.log", "error");
            }
        }

        public static async Task RenameToBik()
        {
            try
            {
                string selectedFilePath = "",
                       selectedFileName = "";

                UIElements.MediaElement.Stop();
                UIElements.MediaElement.Close();
                UIElements.MediaElement.Source = null;

                if (Vars.IsDataTreeView)
                {
                    selectedFilePath = Dirs.SelectedFilePathData;
                    selectedFileName = Dirs.SelectedFileNameData;
                }
                else
                {
                    selectedFilePath = Dirs.SelectedFilePathMod;
                    selectedFileName = Dirs.SelectedFileNameMod;
                }

                await Task.Run(() => GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true));

                await Task.Delay(500);

                if (File.Exists(selectedFilePath.Replace(".binkmemory", ".mp4")))
                    await Task.Run(() => FileSystem.RenameFile(selectedFilePath.Replace(".binkmemory", ".mp4"), selectedFileName.Replace(".binkmemory", ".bik")));
            }
            catch (Exception ex)
            {
                Write.ToErrorLog(ex);
                Write.ToEventLog("Unable to rename file! See error.log", "error");
            }
        }

        public static void OpenFileLocation()
        {
            try
            {
                string selectedFilePath = "";

                if (Vars.IsDataTreeView)
                    selectedFilePath = Dirs.SelectedFilePathData;
                else
                    selectedFilePath = Dirs.SelectedFilePathMod;

                Process.Start("explorer.exe", "/select, " + selectedFilePath);
            }
            catch (Exception ex)
            {
                Write.ToErrorLog(ex);
                Write.ToEventLog("Unable to open file location! See error.log", "error");
            }
        }
    }
}
