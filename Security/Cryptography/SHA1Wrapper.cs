using System.IO;
using System.Security.Cryptography;

namespace Leayal.Security.Cryptography
{
    public static class SHA1Wrapper
    {
        public static string HashFromFile(FileInfo fileinfo)
        {
            return HashFromFile(fileinfo, 4096);
        }

        public static string HashFromFile(string path)
        {
            return HashFromFile(path, 4096);
        }

        public static string HashFromFile(FileInfo fileinfo, int buffersize)
        {
            return HashFromFile(fileinfo.FullName, buffersize);
        }

        public static string HashFromFile(string path, int buffersize)
        {
            string result = null;
            using (SHA1 sha = SHA1.Create())
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, buffersize))
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(fs));
                sha.Clear();
            }
            return result;
        }

        public static string HashFromStream(Stream contentStream)
        {
            string result = null;
            using (SHA1 sha = SHA1.Create())
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(contentStream));
                sha.Clear();
            }
            return result;
        }

        public static string HashFromContent(byte[] content)
        {
            return HashFromContent(content, 0, content.Length);
        }

        public static string HashFromContent(byte[] content, int offset, int count)
        {
            string result = null;
            using (SHA1 sha = SHA1.Create())
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(content, offset, count));
                sha.Clear();
            }
            return result;
        }

        public static string HashFromString(TextReader contentReader)
        {
            return HashFromString(contentReader.ReadToEnd());
        }

        public static string HashFromString(string content)
        {
            string result = null;
            using (SHA1 sha = SHA1.Create())
            using (BytesConverter bc = new BytesConverter())
            {
                result = bc.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content)));
                sha.Clear();
            }
            return result;
        }
    }
}
