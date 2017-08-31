using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebServer
{
    /// <summary>
    /// Provides a nice interface for setting bindings.
    /// </summary>
    public class RequestMethodBinder
    {
        /// <summary>
        /// Method that responds to requests.
        /// </summary>
        /// <param name="request">Request object.</param>
        /// <param name="response">Response object.</param>
        public delegate void Responder(
            HttpRequest request,
            HttpResponse response);

        /// <summary>
        /// A binding from a pattern to a responder. The pattern is a simple
        /// regex.
        /// </summary>
        private class Binding
        {
            /// <summary>
            /// The pattern.
            /// </summary>
            public readonly string Pattern;

            /// <summary>
            /// Regex created from pattern.
            /// </summary>
            public readonly Regex PatternRegex;

            /// <summary>
            /// Responder.
            /// </summary>
            public readonly Responder Responder;

            /// <summary>
            /// Creates a new Binding.
            /// </summary>
            public Binding(
                string pattern,
                Responder responder)
            {
                Pattern = pattern;
                Responder = responder;
                PatternRegex = new Regex(pattern);
            }
        }

        /// <summary>
        /// Keeps an ordered list of bindings.
        /// </summary>
        private readonly List<Binding> _bindings = new List<Binding>();

        /// <summary>
        /// Gets/sets a responder for a pattern.
        /// </summary>
        /// <param name="pattern">Pattern to match.</param>
        /// <returns></returns>
        public Responder this[string pattern]
        {
            get
            {
                foreach (var binding in _bindings)
                {
                    if (binding.Pattern == pattern)
                    {
                        return binding.Responder;
                    }
                }

                return null;
            }
            set
            {
                for (int i = 0, len = _bindings.Count; i < len; i++)
                {
                    if (_bindings[i].Pattern == pattern)
                    {
                        _bindings[i] = new Binding(pattern, value);
                        return;
                    }
                }

                _bindings.Add(new Binding(pattern, value));
            }
        }

        /// <summary>
        /// Retrieves the matching responder for a URI.
        /// </summary>
        /// <param name="uri">The URI to match against.</param>
        /// <returns></returns>
        public Responder Match(string uri)
        {
            foreach (var binding in _bindings)
            {
                if (binding.PatternRegex.IsMatch(uri))
                {
                    return binding.Responder;
                }
            }

            return null;
        }
    }
}