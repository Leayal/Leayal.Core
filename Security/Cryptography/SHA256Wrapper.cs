using System.IO;
using System.Security.Cryptography;

namespace Leayal.Security.Cryptography
{
    public static class SHA256Wrapper
    {
        public static string FromFile(FileInfo fileinfo)
        {
            return FromFile(fileinfo, 4096);
        }

        public static string FromFile(string path)
        {
            return FromFile(path, 4096);
        }

        public static string FromFile(FileInfo fileinfo, int buffersize)
        {
            return FromFile(fileinfo.FullName, buffersize);
        }

        public static string FromFile(string path, int buffersize)
        {
            string result = null;
            using (SHA256 sha = SHA256.Create())
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffersize))
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(fs));
                sha.Clear();
            }
            return result;
        }

        public static string FromStream(Stream contentStream)
        {
            string result = null;
            using (SHA256 sha = SHA256.Create())
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(contentStream));
                sha.Clear();
            }
            return result;
        }

        public static string FromContent(byte[] content)
        {
            string result = null;
            using (SHA256 sha = SHA256.Create())
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(content));
                sha.Clear();
            }
            return result;
        }

        public static string FromContent(byte[] content, int offset, int count)
        {
            string result = null;
            using (SHA256 sha = SHA256.Create())
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(content, offset, count));
                sha.Clear();
            }
            return result;
        }

        public static string FromString(TextReader contentReader)
        {
            string result = null;
            using (SHA256 sha = SHA256.Create())
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(contentReader.ReadToEnd())));
                sha.Clear();
            }
            return result;
        }

        public static string FromString(string content)
        {
            string result = null;
            using (SHA256 sha = SHA256.Create())
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content)));
                sha.Clear();
            }
            return result;
        }
    }
}
