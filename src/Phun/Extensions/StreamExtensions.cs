namespace Phun.Extensions
{
    using System.IO;

    /// <summary>
    /// Stream extensions class.
    /// </summary>
    public static class StreamExtensions
    {

        /// <summary>
        /// Reads all bytes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>All bytes read.</returns>
        public static byte[] ReadAll(this Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
