using System;
using System.Collections.Generic;
using System.Linq;

namespace Deployer.Service.Core.Helpers
{
    public static class FoldersLocks
    {
        private static readonly object _syncRoot = new object();
        private static readonly Dictionary<string, string> _dict = 
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public static IEnumerable<string> GetLockedFolders() {
            lock (_syncRoot)
                return _dict.Select(d => d.Key).ToArray();
        }

        public static bool Add(string folderPath, string sessionId) {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Fodler path parameter is empty.", "folderPath");

            lock (_syncRoot) {
                if (_dict.ContainsKey(folderPath))
                    return false;

                _dict.Add(folderPath, sessionId);
                return true;
            }
        }

        public static bool Remove(string folderPath) {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Fodler path parameter is empty.", "folderPath");

            lock (_syncRoot)
                return _dict.Remove(folderPath);
        }

        public static int RemoveForSession(string sessionId) {
            if (string.IsNullOrEmpty(sessionId))
                throw new ArgumentException("Session id parameter is empty.", "sessionId");

            lock (_syncRoot) {
                var lockedFolders = _dict.Where(d => d.Value.Equals(sessionId)).ToArray();
                return lockedFolders.Count(lockedFolder => _dict.Remove(lockedFolder.Key));
            }
        }
    }
}