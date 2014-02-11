using System.Text;

namespace DispatcherLibrary
{
    public static class Extensions
    {
        public static byte[] ConvertToBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string ConvertToString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
