using System;
using System.IO;
using Shell32;

namespace Leayal.Shell
{
    /// <summary>
    /// This class will not work on Windows XP and older.
    /// </summary>
    public class Shortcut
    {
        /// <summary>
        /// Create new <see cref="Shortcut"/> object from existing shortcut file.
        /// </summary>
        /// <param name="path">Path to the shortcut file</param>
        /// <returns><see cref="Shortcut"/></returns>
        public static Shortcut Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"'{path}' is not existed.");
            return new Shortcut(path);
        }

        /// <summary>
        /// Create a new shortcut file. If the file is already exist, exception will be thrown.
        /// </summary>
        /// <param name="destination">Fullpath to the shortcut file to update it</param>
        /// <param name="targetPath">Target which will is the shortcut point to</param>
        /// <param name="workingFolder">Set the 'Start in' folder</param>
        /// <param name="args">Arguments for the target path</param>
        /// <param name="iconPath">Fullpath to the icon file</param>
        /// <param name="iconIndex">Index of the icon in the icon group</param>
        /// <param name="comment">Comment of the shortcut</param>
        /// <returns>Bool</returns>
        public static bool CreateNew(string destination, string targetPath, string workingFolder = null, string args = null, string iconPath = null, int iconIndex = 0, string comment = null)
        {
            if (File.Exists(destination))
                throw new InvalidOperationException($"'{destination}' is already existed.");
            Shortcut sc = new Shortcut()
            {
                TargetPath = targetPath,
                Arguments = args,
                WorkingDirectory = workingFolder,
                Icon = iconPath,
                IconIndex = iconIndex,
                Comment = comment
            };
            if (sc.WriteTo(destination) == null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Create a new shortcut file. If the file is already exist, it will be overwritten.
        /// </summary>
        /// <param name="destination">Fullpath to the shortcut file to update it</param>
        /// <param name="targetPath">Target which will is the shortcut point to</param>
        /// <param name="workingFolder">Set the 'Start in' folder</param>
        /// <param name="args">Arguments for the target path</param>
        /// <param name="iconPath">Fullpath to the icon file</param>
        /// <param name="iconIndex">Index of the icon in the icon group</param>
        /// <param name="comment">Comment of the shortcut</param>
        /// <returns>Bool</returns>
        public static bool CreateOverwrite(string destination, string targetPath, string workingFolder = null, string args = null, string iconPath = null, int iconIndex = 0, string comment = null)
        {
            Shortcut sc = new Shortcut()
            {
                TargetPath = targetPath,
                Arguments = args,
                WorkingDirectory = workingFolder,
                Icon = iconPath,
                IconIndex = iconIndex,
                Comment = comment
            };
            if (sc.WriteTo(destination) == null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Update a shortcut file.
        /// </summary>
        /// <param name="destination">Fullpath to the shortcut file to update it</param>
        /// <param name="targetPath">Target which will is the shortcut point to</param>
        /// <param name="workingFolder">Set the 'Start in' folder</param>
        /// <param name="args">Arguments for the target path</param>
        /// <param name="iconPath">Fullpath to the icon file</param>
        /// <param name="iconIndex">Index of the icon in the icon group. -1 means no changes.</param>
        /// <param name="comment">Comment of the shortcut</param>
        /// <returns>Bool</returns>
        public static bool Update(string destination, string targetPath, string workingFolder = null, string args = null, string iconPath = null, int iconIndex = -1, string comment = null)
        {
            if (!File.Exists(destination))
                throw new FileNotFoundException($"'{destination}' is not existed.");
            Shortcut sc = new Shortcut(destination);

            sc.TargetPath = targetPath;

            if (!string.IsNullOrEmpty(comment))
                sc.Comment = comment;
            if (!string.IsNullOrEmpty(args))
                sc.Arguments = args;
            if (!string.IsNullOrEmpty(workingFolder))
                sc.WorkingDirectory = workingFolder;
            if (!string.IsNullOrEmpty(iconPath))
                sc.Icon = iconPath;
            if (iconIndex < 0) { } else sc.IconIndex = iconIndex;

            if (sc.WriteTo(destination) == null)
                return true;
            else
                return false;
        }

        private Type shellAppType;
        private Shell32.Shell _shell;
        private ShellLinkObject slo;
        private Folder dir;
        private FolderItem itm;

        public Shortcut()
        {
            this.shellAppType = Type.GetTypeFromProgID("Shell.Application");
            this._shell = (Shell32.Shell)Activator.CreateInstance(shellAppType);
            this.IconIndex = 0;
        }

        internal Shortcut(string path) : this()
        {
            this.dir = (Folder)this.shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, this._shell, new object[] { Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(path) });
            //Folder dir = this._shell.NameSpace(Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(path));
            this.itm = dir.ParseName(Path.GetFileName(path));
            if (this.itm.IsLink)
            {
                this.slo = (ShellLinkObject)this.itm.GetLink;
                this.TargetPath = this.slo.Path;
                this.Comment = this.slo.Description;
                this.Arguments = this.slo.Arguments;
                this.WorkingDirectory = this.slo.WorkingDirectory;
                string iconPath = null;
                this.IconIndex = this.slo.GetIconLocation(out iconPath);
                this.Icon = iconPath;
            }
            else
                throw new InvalidDataException($"'{path}' is not a shortcut.");
        }

        // public ConsoleKey? Hotkey { get; set; }
        public string TargetPath { get; set; }
        public string Comment { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public string Icon { get; set; }
        public int IconIndex { get; set; }

        /// <summary>
        /// Create a new shortcut file based on current properties. Return null if success, or <see cref="System.Runtime.InteropServices.COMException"/> if failed.
        /// </summary>
        /// <param name="path">Destination of the new shortcut file</param>
        /// <returns><see cref="System.Runtime.InteropServices.COMException"/></returns>
        public System.Runtime.InteropServices.COMException WriteTo(string path)
        {
            if (!File.Exists(this.TargetPath)) throw new FileNotFoundException("Target file is not existed.", this.TargetPath);
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("The destination path should not be empty or whitespace.");
            if (Path.GetFullPath(this.TargetPath).IsEqual(path, true)) throw new InvalidOperationException("Cannot create a shortcut which target to itself.");

            File.Create(path).Close();

            if (this.slo == null)
            {
                this.dir = (Folder)this.shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, this._shell, new object[] { Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(path) });
                this.itm = this.dir.ParseName(Path.GetFileName(path));
                this.slo = (ShellLinkObject)this.itm.GetLink;
            }

            this.slo.Path = this.TargetPath;

            if (!string.IsNullOrEmpty(this.Comment))
                this.slo.Description = this.Comment;

            if (!string.IsNullOrEmpty(this.Arguments))
                this.slo.Arguments = this.Arguments;
            else
                this.slo.Arguments = string.Empty;

            if (string.IsNullOrEmpty(this.WorkingDirectory))
                this.slo.WorkingDirectory = Path.GetDirectoryName(this.TargetPath);
            else
                this.slo.WorkingDirectory = this.WorkingDirectory;

            if (!string.IsNullOrEmpty(this.Icon))
            {
                if (this.IconIndex < 0)
                    this.IconIndex = 0;
                this.slo.SetIconLocation(this.Icon, this.IconIndex);
            }

            try { this.slo.Save(this.itm.Path); return null; }
            catch (System.Runtime.InteropServices.COMException ex) { return ex; }
        }
    }
}
