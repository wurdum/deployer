using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Deployer.Tests.Service.Helpers.IISManagers
{
    public class Impersonator : IDisposable
    {
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private WindowsImpersonationContext _impersonationContext;

        /// <summary>
        ///     Constructor. Starts the impersonation with the given credentials.
        ///     Please note that the account that instantiates the Impersonator class
        ///     needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        public Impersonator(string domainName, string userName, string password) {
            ImpersonateValidUser(domainName, userName, password);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        ///     Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domain">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        private void ImpersonateValidUser(string domain, string userName, string password) {
            var token = IntPtr.Zero;
            var tokenDuplicate = IntPtr.Zero;

            try {
                if (!RevertToSelf() || LogonUser(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) == 0 ||
                    DuplicateToken(token, 2, ref tokenDuplicate) == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                var tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                _impersonationContext = tempWindowsIdentity.Impersonate();
            } finally {
                if (token != IntPtr.Zero)
                    CloseHandle(token);

                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
        }

        public void Dispose() {
            UndoImpersonation();
        }

        /// <summary>
        ///     Reverts the impersonation.
        /// </summary>
        private void UndoImpersonation() {
            if (_impersonationContext != null)
                _impersonationContext.Undo();
        }
    }
}