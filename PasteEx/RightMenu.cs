﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasteEx
{
    public class RightMenu
    {
        public static bool Init()
        {
            string command = (string)Registry.GetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\PasteEx\command", "", "");
            if (String.IsNullOrEmpty(command))
            {
                if (!Properties.Settings.Default.firstTipFlag)
                {
                    return true;
                }

                DialogResult result = MessageBox.Show(Resources.Resource_zh_CN.TipFirstRegister,
                    Resources.Resource_zh_CN.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Add();
                }
                else if (result == DialogResult.No)
                {
                    Properties.Settings.Default.firstTipFlag = false;
                    Properties.Settings.Default.Save();
                }
            }
            else if (command != Application.ExecutablePath + " \"%V\"")
            {
                if (MessageBox.Show(Resources.Resource_zh_CN.TipWrongValueInMenu, Resources.Resource_zh_CN.Title,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Add();
                }
            }
            return true;
        }

        public static void Add()
        {
            bool isAdmin = IsUserAdministrator();

            if (isAdmin)
            {
                try { UnRegister(); } catch { }

                try
                {
                    Register();
                    MessageBox.Show(Resources.Resource_zh_CN.TipRegister, Resources.Resource_zh_CN.Title,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + Resources.Resource_zh_CN.TipRunAsAdmin,
                        Resources.Resource_zh_CN.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // restart and run as admin
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    Arguments = "-reg",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas" // run as admin
                };
                Process.Start(startInfo);
            }
        }

        public static void Delete()
        {
            bool isAdmin = IsUserAdministrator();

            if (isAdmin)
            {
                try
                {
                    UnRegister();
                    MessageBox.Show(Resources.Resource_zh_CN.TipUnRegister, Resources.Resource_zh_CN.Title,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + Resources.Resource_zh_CN.TipRunAsAdmin,
                        Resources.Resource_zh_CN.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // restart and run as admin
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    Arguments = "-unreg",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas" // run as admin
                };
                Process.Start(startInfo);
            }
        }

        private static void Register()
        {
            var key = Registry.ClassesRoot.OpenSubKey("Directory").OpenSubKey("Background").OpenSubKey("shell", true).CreateSubKey("PasteEx"); ;
            key.SetValue("", Resources.Resource_zh_CN.Title);
            key.SetValue("Icon", Application.ExecutablePath);
            key = key.CreateSubKey("command");
            key.SetValue("", Application.ExecutablePath + " \"%V\"");

            key = Registry.ClassesRoot.OpenSubKey("Directory").OpenSubKey("shell", true).CreateSubKey("PasteEx");
            key.SetValue("", Resources.Resource_zh_CN.Title);
            key.SetValue("Icon", Application.ExecutablePath);
            key = key.CreateSubKey("command");
            key.SetValue("", Application.ExecutablePath + " \"%1\"");

        }

        private static void UnRegister()
        {
            var key = Registry.ClassesRoot.OpenSubKey("Directory").OpenSubKey("Background").OpenSubKey("shell", true);
            key.DeleteSubKeyTree("PasteEx");

            key = Registry.ClassesRoot.OpenSubKey("Directory").OpenSubKey("shell", true);
            key.DeleteSubKeyTree("PasteEx");
        }

        public static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
