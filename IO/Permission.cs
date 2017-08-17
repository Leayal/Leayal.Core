using System.IO;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Leayal.IO
{
    public static class Permission
    {
        private static SecurityIdentifier _everyone;
        public static SecurityIdentifier Everyone
        {
            get
            {
                if (_everyone == null)
                    _everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                return _everyone;
            }
        }

        private static SecurityIdentifier _currentuser;
        public static SecurityIdentifier CurrentUser
        {
            get
            {
                if (_currentuser == null)
                {
                    using (var myself = WindowsIdentity.GetCurrent())
                        _currentuser = myself.User;
                }
                return _currentuser;
            }
        }

        private static SecurityIdentifier _administration;
        public static SecurityIdentifier Administration
        {
            get
            {
                if (_administration == null)
                    _administration = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                return _administration;
            }
        }

        public static void AddFileSecurity(string fileName, FileSystemRights rights)
        {
            AddFileSecurity(fileName, CurrentUser, rights, AccessControlType.Allow);
        }

        public static void AddFileSecurity(string fileName, FileSystemRights rights, AccessControlType controlType)
        {
            AddFileSecurity(fileName, CurrentUser, rights, controlType);
        }

        public static void AddFileSecurity(string fileName, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            AddFileSecurity(fileName, CurrentUser, permissions);
        }

        public static void AddFileSecurity(string fileName, SecurityIdentifier sid, FileSystemRights rights, AccessControlType controlType)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(sid, rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void AddFileSecurity(string fileName, params FileSystemAccessRule[] rules)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            for (int i = 0; i < rules.Length; i++)
                fSecurity.AddAccessRule(rules[i]);

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void AddFileSecurity(string fileName, SecurityIdentifier sid, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            foreach (var _keypair in permissions)
                fSecurity.AddAccessRule(new FileSystemAccessRule(sid, _keypair.Key, _keypair.Value));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void AddFileSecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void AddFileSecurity(string fileName, string account, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            foreach (var _keypair in permissions)
                fSecurity.AddAccessRule(new FileSystemAccessRule(account, _keypair.Key, _keypair.Value));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void RemoveFileSecurity(string fileName, FileSystemRights rights, AccessControlType controlType)
        {
            RemoveFileSecurity(fileName, CurrentUser, rights, controlType);
        }

        public static void RemoveFileSecurity(string fileName, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            RemoveFileSecurity(fileName, CurrentUser, permissions);
        }

        public static void RemoveFileSecurity(string fileName, SecurityIdentifier sid, FileSystemRights rights, AccessControlType controlType)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Remove the FileSystemAccessRule from the security settings.
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(sid, rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void RemoveFileSecurity(string fileName, SecurityIdentifier sid, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Remove the FileSystemAccessRule from the security settings.
            foreach (var _keypair in permissions)
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(sid, _keypair.Key, _keypair.Value));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void RemoveFileSecurity(string fileName, params FileSystemAccessRule[] rules)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Remove the FileSystemAccessRule from the security settings.
            for (int i = 0; i< rules.Length; i++)
                fSecurity.RemoveAccessRule(rules[i]);

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        /// <summary>
        /// Removes an ACL entry on the specified file for the specified account.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="account"></param>
        /// <param name="rights"></param>
        /// <param name="controlType"></param>
        public static void RemoveFileSecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Remove the FileSystemAccessRule from the security settings.
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void RemoveFileSecurity(string fileName, string account, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Cannot find the file", fileName);
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Remove the FileSystemAccessRule from the security settings.
            foreach (var _keypair in permissions)
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(account, _keypair.Key, _keypair.Value));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        public static void AddDirectorySecurity(string dir, FileSystemRights rights)
        {
            AddDirectorySecurity(dir, CurrentUser, rights, AccessControlType.Allow);
        }

        public static void AddDirectorySecurity(string dir, FileSystemRights rights, AccessControlType controlType)
        {
            AddDirectorySecurity(dir, CurrentUser, rights, controlType);
        }

        public static void AddDirectorySecurity(string dir, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            AddDirectorySecurity(dir, CurrentUser, permissions);
        }

        public static void AddDirectorySecurity(string dir, SecurityIdentifier sid, FileSystemRights rights, AccessControlType controlType)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(sid, rights, controlType));

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void AddDirectorySecurity(string dir, SecurityIdentifier sid, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            foreach (var _keypair in permissions)
                fSecurity.AddAccessRule(new FileSystemAccessRule(sid, _keypair.Key, _keypair.Value));

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void AddDirectorySecurity(string dir, string account, FileSystemRights rights, AccessControlType controlType)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void AddDirectorySecurity(string dir, string account, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            foreach (var _keypair in permissions)
                fSecurity.AddAccessRule(new FileSystemAccessRule(account, _keypair.Key, _keypair.Value));

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void AddDirectorySecurity(string dir, params FileSystemAccessRule[] rules)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            for (int i = 0; i < rules.Length; i++)
                fSecurity.AddAccessRule(rules[i]);

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void RemoveDirectorySecurity(string dir, FileSystemRights rights)
        {
            RemoveDirectorySecurity(dir, CurrentUser, rights, AccessControlType.Allow);
        }

        public static void RemoveDirectorySecurity(string dir, FileSystemRights rights, AccessControlType controlType)
        {
            RemoveDirectorySecurity(dir, CurrentUser, rights, controlType);
        }

        public static void RemoveDirectorySecurity(string dir, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            RemoveDirectorySecurity(dir, CurrentUser, permissions);
        }

        public static void RemoveDirectorySecurity(string dir, SecurityIdentifier sid, FileSystemRights rights, AccessControlType controlType)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(sid, rights, controlType));

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void RemoveDirectorySecurity(string dir, SecurityIdentifier sid, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            foreach (var _keypair in permissions)
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(sid, _keypair.Key, _keypair.Value));

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void RemoveDirectorySecurity(string dir, string account, FileSystemRights rights, AccessControlType controlType)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void RemoveDirectorySecurity(string dir, string account, IDictionary<FileSystemRights, AccessControlType> permissions)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            foreach (var _keypair in permissions)
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(account, _keypair.Key, _keypair.Value));

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }

        public static void RemoveDirectorySecurity(string dir, params FileSystemAccessRule[] rules)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Cannot find the directory '{dir}'");
            // Get a FileSecurity object that represents the
            // current security settings.
            DirectorySecurity fSecurity = Directory.GetAccessControl(dir);

            // Add the FileSystemAccessRule to the security settings.
            for (int i = 0; i < rules.Length; i++)
                fSecurity.RemoveAccessRule(rules[i]);

            // Set the new access settings.
            Directory.SetAccessControl(dir, fSecurity);
        }
    }
}
