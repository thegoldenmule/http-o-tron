using System.Collections.Generic;

namespace WebServer
{
    /// <summary>
    /// Bucket for multi-part request params.
    /// </summary>
    public class MultipartRequestParameters
    {
        /// <summary>
        /// Lookup from name to payload.
        /// </summary>
        private readonly Dictionary<string, byte[]> _parts = new Dictionary<string, byte[]>();

        /// <summary>
        /// Retrieves the payload by name.
        /// </summary>
        /// <param name="name">Name of the payload.</param>
        /// <param name="bytes">Bytes.</param>
        /// <returns></returns>
        public bool Get(string name, out byte[] bytes)
        {
            return _parts.TryGetValue(name, out bytes);
        }

        /// <summary>
        /// Puts a payload into the collection.
        /// </summary>
        /// <param name="name">Name of the payload.</param>
        /// <param name="bytes">Bytes.</param>
        public void Put(string name, ref byte[] bytes)
        {
            _parts[name] = bytes;
        }
    }
}