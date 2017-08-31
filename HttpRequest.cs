using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace WebServer
{
    /// <summary>
    /// Request object.
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// The C# API request.
        /// </summary>
        public readonly HttpListenerRequest Request;

        /// <summary>
        /// For multi-part body.
        /// </summary>
        public readonly MultipartRequestParameters Parts = new MultipartRequestParameters();

        /// <summary>
        /// Creats a new request.
        /// </summary>
        /// <param name="request">The C# request.</param>
        public HttpRequest(HttpListenerRequest request)
        {
            Request = request;

            ProcessMultipart();
        }

        /// <summary>
        /// Processes multipart body if necessary.
        /// </summary>
        private void ProcessMultipart()
        {
            if (string.IsNullOrEmpty(Request.ContentType))
            {
                return;
            }

            // find boundary
            var isMultipart = false;
            var boundary = string.Empty;
            var split = Request.ContentType.Split(';');
            for (int i = 0, len = split.Length; i < len; i++)
            {
                var value = split[i].Trim();
                if (value == "multipart/form-data")
                {
                    isMultipart = true;
                }
                else if (value.StartsWith("boundary"))
                {
                    var boundarySplit = value.Split('=');
                    boundary = boundarySplit[1];
                }
            }

            // if it has proper Content-Type, parse at boundary
            if (isMultipart)
            {
                ParseMultiPart(boundary);
            }
        }

        /// <summary>
        /// Parses along a boundary.
        /// </summary>
        /// <param name="boundary"></param>
        private void ParseMultiPart(string boundary)
        {
            Log("Parsing multipart request.");

            // get queries
            var boundaryBytes = Encoding.UTF8.GetBytes($"--{boundary}");

            // get full buffer
            byte[] bytes;
            using (var memory = new MemoryStream())
            {
                Request.InputStream.CopyTo(memory);
                bytes = memory.ToArray();
            }

            // find boundaries
            var index = 0;
            while (true)
            {
                index = IndexOf(ref bytes, ref boundaryBytes, index);
                if (-1 == index)
                {
                    Log("Could not find any more boundaries.");
                    break;
                }

                Log($"Found boundary at {index}.");

                // move past boundary
                index += boundaryBytes.Length;

                // find interval
                // [start, end)
                var start = index;
                var end = IndexOf(ref bytes, ref boundaryBytes, index);
                if (-1 == end)
                {
                    Log("No next boundary.");

                    // last boundary
                    break;
                }

                // total part length
                var partLength = end - start;
                if (!ParsePart(start, partLength, ref bytes))
                {
                    Log("Could not parse part.");

                    // malformed
                    break;
                }
            }
        }

        /// <summary>
        /// Parses a part between boundary tags.
        /// 
        /// TODO: Fairly specific to out use case, may need generalized.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private bool ParsePart(
            int start,
            int length,
            ref byte[] bytes)
        {
            Log($"Parsing part at [{start}, {start + length}].");

            var newLine = Encoding.UTF8.GetBytes("\n");
            var newLineWindows = Encoding.UTF8.GetBytes("\r\n");

            var name = string.Empty;
            var index = start;

            // advance past initial blank lines
            while (bytes[index] == '\n' || bytes[index] == '\r')
            {
                index += 1;
            }
            
            // parse lines
            while (index < start + length)
            {
                // advance past initial blank lines
                while (bytes[index] == '\n' || bytes[index] == '\r')
                {
                    index += 1;
                }

                var lineStart = index;
                var lineEnd = IndexOf(ref bytes, ref newLine, lineStart);
                var lineEndWindows = IndexOf(ref bytes, ref newLineWindows, lineStart);
                if (-1 == lineEndWindows)
                {
                    if (-1 == lineEnd)
                    {
                        // malformed
                        Log("Malformed.");
                        return false;
                    }
                }
                else
                {
                    lineEnd = lineEndWindows;
                }

                var line = Encoding.UTF8.GetString(bytes, lineStart, lineEnd - lineStart);
                Log($"\tParse: \"{line}\"");

                if (line.StartsWith("Content-Disposition"))
                {
                    // pull out name
                    var contentSplit = line.Split(':');
                    var contentValueSplit = contentSplit[1].Trim().Split(';');
                    foreach (var valueSplit in contentValueSplit)
                    {
                        var trimmed = valueSplit.Trim();
                        if (trimmed.StartsWith("name="))
                        {
                            name = trimmed.Split('=')[1].Trim('"');

                            Log($"\t[Name of part: {name}.]");
                            break;
                        }
                    }
                }
                else if (line.StartsWith("Content-Type"))
                {
                    //
                }
                else
                {
                    index = lineStart;
                    break;
                }

                // advance until after line
                index = lineEnd + 1;
            }

            Log("\tReady for payload.");

            // advance past trailing blank lines
            while (bytes[index] == '\n' || bytes[index] == '\r')
            {
                index += 1;
            }

            // parse what's left
            if (index < start + length)
            {
                var payload = new byte[start + length - index];
                Array.Copy(bytes, index, payload, 0, payload.Length);
                Parts.Put(name, ref payload);

                if (payload.Length < 100)
                {
                    Log($"Put {Encoding.UTF8.GetString(payload)} into {name}.");
                }
                else
                {
                    Log($"Put {payload.Length} bytes into {name}.");
                }
            }

            return true;
        }
        
        /// <summary>
        /// Finds the index in bytes that the query occurs after startIndex, 
        /// inclusive.
        /// </summary>
        private int IndexOf(ref byte[] bytes, ref byte[] query, int startIndex = 0)
        {
            for (int i = startIndex, len = bytes.Length - query.Length; i <= len; i++)
            {
                var match = true;
                for (var j = 0; j < query.Length && match; j++)
                {
                    match = bytes[i + j] == query[j];
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Compile out logs.
        /// </summary>
        [Conditional("DEBUG_LOGGING")]
        private void Log(string message, params object[] replacements)
        {
            Console.WriteLine(message, replacements);
        }
    }
}