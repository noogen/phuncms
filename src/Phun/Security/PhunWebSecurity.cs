namespace Phun.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Web;
    using System.Web.Routing;
    using System.Web.Security;

    using WebMatrix.WebData;

    /// <summary>
    /// Not trying to be microsoft websecurity which is horrible.  Static?
    /// </summary>
    public static class PhunWebSecurity
    {
        /// <summary>
        /// The enable simple membership key
        /// </summary>
        public static readonly string EnableSimpleMembershipKey = "enableSimpleMembership";

        /// <summary>
        /// Gets a value indicating whether this <see cref="PhunWebSecurity"/> is initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initialized; otherwise, <c>false</c>.
        /// </value>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Gets the current user id.
        /// </summary>
        /// <value>
        /// The current user id.
        /// </value>
        public static int CurrentUserId
        {
            get { return GetUserId(CurrentUserName); }
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <value>
        /// The name of the current user.
        /// </value>
        public static string CurrentUserName
        {
            get { return Context.User.Identity.Name; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has user id.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has user id; otherwise, <c>false</c>.
        /// </value>
        public static bool HasUserId
        {
            get { return CurrentUserId != -1; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is authenticated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        public static bool IsAuthenticated
        {
            get { return Request.IsAuthenticated; }
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        internal static HttpContextBase Context
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        internal static HttpRequestBase Request
        {
            get { return Context.Request; }
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        internal static HttpResponseBase Response
        {
            get { return Context.Response; }
        }

        /// <summary>
        /// Pres the app start init.
        /// </summary>
        internal static void PreAppStartInit()
        {
            // Allow use of <add key="EnableSimpleMembershipKey" value="false" /> to disable registration of membership/role providers as default.
            if (PhunConfigUtil.SimpleMembershipEnabled)
            {
                // called during PreAppStart, should also hook up the config for MembershipProviders?
                // Replace the AspNetSqlMembershipProvider (which is the default that is registered in root web.config)
                const string BuiltInMembershipProviderName = "AspNetSqlMembershipProvider";
                var builtInMembership = Membership.Providers[BuiltInMembershipProviderName];
                if (builtInMembership != null)
                {
                    var simpleMembership = CreateDefaultSimpleMembershipProvider(BuiltInMembershipProviderName, currentDefault: builtInMembership);
                    Membership.Providers.Remove(BuiltInMembershipProviderName);
                    Membership.Providers.Add(simpleMembership);
                }

                Roles.Enabled = true;
                const string BuiltInRolesProviderName = "AspNetSqlRoleProvider";
                var builtInRoles = Roles.Providers[BuiltInRolesProviderName];
                if (builtInRoles != null)
                {
                    var simpleRoles = CreateDefaultSimpleRoleProvider(BuiltInRolesProviderName, currentDefault: builtInRoles);
                    Roles.Providers.Remove(BuiltInRolesProviderName);
                    Roles.Providers.Add(simpleRoles);
                }
            }
        }

        /// <summary>
        /// Verifies the provider.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">No extended membership provider.</exception>
        private static ExtendedMembershipProvider VerifyProvider()
        {
            var provider = Membership.Provider as ExtendedMembershipProvider;
            if (provider == null)
            {
                throw new InvalidOperationException("No extended membership provider.");
            }

            // Have the provider verify that it's initialized (only our SimpleMembershipProvider does anything here)
            var phunProvider = provider as PhunSimpleMembershipProvider;
            if (phunProvider != null) phunProvider.VerifyInitialized();
 
            return provider;
        }

        /// <summary>
        /// Initializes the database connection.
        /// </summary>
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <param name="userTableName">Name of the user table.</param>
        /// <param name="userIdColumn">The user id column.</param>
        /// <param name="userNameColumn">The user name column.</param>
        /// <param name="autoCreateTables">if set to <c>true</c> [auto create tables].</param>
        public static void InitializeDatabaseConnection(string connectionStringName, string userTableName, string userIdColumn, string userNameColumn, bool autoCreateTables)
        {
            InitializeProviders(connectionStringName, userTableName, userIdColumn, userNameColumn, autoCreateTables);
        }


        /// <summary>
        /// Initializes the providers.
        /// </summary>
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <param name="userTableName">Name of the user table.</param>
        /// <param name="userIdColumn">The user id column.</param>
        /// <param name="userNameColumn">The user name column.</param>
        /// <param name="autoCreateTables">if set to <c>true</c> [auto create tables].</param>
        private static void InitializeProviders(string connectionStringName, string userTableName, string userIdColumn, string userNameColumn, bool autoCreateTables)
        {
            var simpleMembership = Membership.Provider as PhunSimpleMembershipProvider;
            if (simpleMembership != null)
            {
                InitializeMembershipProvider(simpleMembership, connectionStringName, userTableName, userIdColumn, userNameColumn, autoCreateTables);
            }

            var simpleRoles = Roles.Provider as PhunSimpleRoleProvider;
            if (simpleRoles != null)
            {
                InitializeRoleProvider(simpleRoles, connectionStringName, userTableName, userIdColumn, userNameColumn, autoCreateTables);
            }

            Initialized = true;
        }

        /// <summary>
        /// Initializes the membership provider.
        /// </summary>
        /// <param name="simpleMembership">The simple membership.</param>
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <param name="userTableName">Name of the user table.</param>
        /// <param name="userIdColumn">The user id column.</param>
        /// <param name="userNameColumn">The user name column.</param>
        /// <param name="createTables">if set to <c>true</c> [create tables].</param>
        /// <exception cref="System.InvalidOperationException">Initialize membership provider already called.</exception>
        internal static void InitializeMembershipProvider(PhunSimpleMembershipProvider simpleMembership, string connectionStringName, string userTableName, string userIdColumn, string userNameColumn, bool createTables)
        {
            if (simpleMembership.InitializeCalled)
            {
                throw new InvalidOperationException("Initialize membership provider already called.");
            }

            simpleMembership.ConnectionStringName = connectionStringName;
            simpleMembership.UserIdColumn = userIdColumn;
            simpleMembership.UserNameColumn = userNameColumn;
            simpleMembership.UserTableName = userTableName;
            if (createTables)
            {
                simpleMembership.CreateTablesIfNeeded();
            }
            else
            {
                // We want to validate the user table if we aren't creating them
                simpleMembership.ValidateUserTable();
            }
            simpleMembership.InitializeCalled = true;
        }

        /// <summary>
        /// Initializes the role provider.
        /// </summary>
        /// <param name="simpleRoles">The simple roles.</param>
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <param name="userTableName">Name of the user table.</param>
        /// <param name="userIdColumn">The user id column.</param>
        /// <param name="userNameColumn">The user name column.</param>
        /// <param name="createTables">if set to <c>true</c> [create tables].</param>
        /// <exception cref="System.InvalidOperationException">Initialize role provider already called.</exception>
        internal static void InitializeRoleProvider(PhunSimpleRoleProvider simpleRoles, string connectionStringName, string userTableName, string userIdColumn, string userNameColumn, bool createTables)
        {
            if (simpleRoles.InitializeCalled)
            {
                throw new InvalidOperationException("Initialize role provider already called.");
            }

            simpleRoles.ConnectionStringName = connectionStringName;
            simpleRoles.UserTableName = userTableName;
            simpleRoles.UserIdColumn = userIdColumn;
            simpleRoles.UserNameColumn = userNameColumn;
            if (createTables)
            {
                simpleRoles.CreateTablesIfNeeded();
            }
            simpleRoles.InitializeCalled = true;
        }

        /// <summary>
        /// Creates the default simple membership provider.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="currentDefault">The current default.</param>
        /// <returns></returns>
        private static PhunSimpleMembershipProvider CreateDefaultSimpleMembershipProvider(string name, MembershipProvider currentDefault)
        {
            var membership = new PhunSimpleMembershipProvider(previousProvider: currentDefault);
            var config = new NameValueCollection();
            membership.Initialize(name, config);
            return membership;
        }

        /// <summary>
        /// Creates the default simple role provider.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="currentDefault">The current default.</param>
        /// <returns></returns>
        private static SimpleRoleProvider CreateDefaultSimpleRoleProvider(string name, RoleProvider currentDefault)
        {
            var roleProvider = new SimpleRoleProvider(previousProvider: currentDefault);
            var config = new NameValueCollection();
            roleProvider.Initialize(name, config);
            return roleProvider;
        }

        /// <summary>
        /// Logins the specified user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="persistCookie">if set to <c>true</c> [persist cookie].</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is used more consistently in ASP.Net")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a helper class, and we are not removing optional parameters from methods in helper classes")]
        public static bool Login(string userName, string password, bool persistCookie = false)
        {
            VerifyProvider();
            bool success = Membership.ValidateUser(userName, password);
            if (success)
            {
                FormsAuthentication.SetAuthCookie(userName, persistCookie);
            }
            return success;
        }

        /// <summary>
        /// Logouts this instance.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Logout", Justification = "Login is used more consistently in ASP.Net")]
        public static void Logout()
        {
            VerifyProvider();
            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public static bool ChangePassword(string userName, string currentPassword, string newPassword)
        {
            VerifyProvider();
            bool success = false;
            try
            {
                var currentUser = Membership.GetUser(userName, true /* userIsOnline */);
                success = currentUser.ChangePassword(currentPassword, newPassword);
            }
            catch (ArgumentException)
            {
                // An argument exception is thrown if the new password does not meet the provider's requirements
            }

            return success;
        }

        /// <summary>
        /// Confirms the account.
        /// </summary>
        /// <param name="accountConfirmationToken">The account confirmation token.</param>
        /// <returns></returns>
        public static bool ConfirmAccount(string accountConfirmationToken)
        {
            return VerifyProvider().ConfirmAccount(accountConfirmationToken);
        }

        /// <summary>
        /// Confirms the account.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="accountConfirmationToken">The account confirmation token.</param>
        /// <returns></returns>
        public static bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            return VerifyProvider().ConfirmAccount(userName, accountConfirmationToken);
        }

        /// <summary>
        /// Creates the account.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="requireConfirmationToken">if set to <c>true</c> [require confirmation token].</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a helper class, and we are not removing optional parameters from methods in helper classes")]
        public static string CreateAccount(string userName, string password, bool requireConfirmationToken = false)
        {
            return VerifyProvider().CreateAccount(userName, password, requireConfirmationToken);
        }

        /// <summary>
        /// Creates the user and account.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="propertyValues">The property values.</param>
        /// <param name="requireConfirmationToken">if set to <c>true</c> [require confirmation token].</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a helper class, and we are not removing optional parameters from methods in helper classes")]
        public static string CreateUserAndAccount(string userName, string password, object propertyValues = null, bool requireConfirmationToken = false)
        {
            IDictionary<string, object> values = null;
            if (propertyValues != null)
            {
                values = new RouteValueDictionary(propertyValues);
            }

            return VerifyProvider().CreateUserAndAccount(userName, password, requireConfirmationToken, values);
        }

        /// <summary>
        /// Generates the password reset token.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="tokenExpirationInMinutesFromNow">The token expiration in minutes from now.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a helper class, and we are not removing optional parameters from methods in helper classes")]
        public static string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow = 1440)
        {
            return VerifyProvider().GeneratePasswordResetToken(userName, tokenExpirationInMinutesFromNow);
        }

        /// <summary>
        /// Users the exists.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static bool UserExists(string userName)
        {
            VerifyProvider();
            return Membership.GetUser(userName) != null;
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static int GetUserId(string userName)
        {
            VerifyProvider();
            MembershipUser user = Membership.GetUser(userName);
            if (user == null)
            {
                return -1;
            }

            // REVIEW: This cast is breaking the abstraction for the membershipprovider, we basically assume that userids are ints
            return (int)user.ProviderUserKey;
        }

        /// <summary>
        /// Gets the user id from password reset token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public static int GetUserIdFromPasswordResetToken(string token)
        {
            return VerifyProvider().GetUserIdFromPasswordResetToken(token);
        }

        /// <summary>
        /// Determines whether [is current user] [the specified user name].
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        ///   <c>true</c> if [is current user] [the specified user name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCurrentUser(string userName)
        {
            VerifyProvider();
            return string.Equals(CurrentUserName, userName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified user name is confirmed.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        ///   <c>true</c> if the specified user name is confirmed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConfirmed(string userName)
        {
            return VerifyProvider().IsConfirmed(userName);
        }

        // Make sure the logged on user is same as the one specified by the id
        private static bool IsUserLoggedOn(int userId)
        {
            VerifyProvider();
            return CurrentUserId == userId;
        }

        /// <summary>
        /// Requires the authenticated user.
        /// </summary>
        public static void RequireAuthenticatedUser()
        {
            VerifyProvider();
            var user = Context.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }

        /// <summary>
        /// Requires the user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        public static void RequireUser(int userId)
        {
            VerifyProvider();
            if (!IsUserLoggedOn(userId))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }

        /// <summary>
        /// Requires the user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public static void RequireUser(string userName)
        {
            VerifyProvider();
            if (!string.Equals(CurrentUserName, userName, StringComparison.OrdinalIgnoreCase))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }

        /// <summary>
        /// Requires the roles.
        /// </summary>
        /// <param name="roles">The roles.</param>
        public static void RequireRoles(params string[] roles)
        {
            VerifyProvider();
            foreach (string role in roles)
            {
                if (!Roles.IsUserInRole(CurrentUserName, role))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="passwordResetToken">The password reset token.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public static bool ResetPassword(string passwordResetToken, string newPassword)
        {
            return VerifyProvider().ResetPasswordWithToken(passwordResetToken, newPassword);
        }

        /// <summary>
        /// Determines whether [is account locked out] [the specified user name].
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="allowedPasswordAttempts">The allowed password attempts.</param>
        /// <param name="intervalInSeconds">The interval in seconds.</param>
        /// <returns>
        ///   <c>true</c> if [is account locked out] [the specified user name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAccountLockedOut(string userName, int allowedPasswordAttempts, int intervalInSeconds)
        {
            return IsAccountLockedOut(userName, allowedPasswordAttempts, TimeSpan.FromSeconds(intervalInSeconds));
        }

        /// <summary>
        /// Determines whether [is account locked out] [the specified user name].
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="allowedPasswordAttempts">The allowed password attempts.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        ///   <c>true</c> if [is account locked out] [the specified user name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAccountLockedOut(string userName, int allowedPasswordAttempts, TimeSpan interval)
        {
            return IsAccountLockedOutInternal(VerifyProvider(), userName, allowedPasswordAttempts, interval);
        }

        /// <summary>
        /// Determines whether [is account locked out internal] [the specified provider].
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="allowedPasswordAttempts">The allowed password attempts.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        ///   <c>true</c> if [is account locked out internal] [the specified provider]; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsAccountLockedOutInternal(ExtendedMembershipProvider provider, string userName, int allowedPasswordAttempts, TimeSpan interval)
        {
            return provider.GetUser(userName, false) != null &&
                    provider.GetPasswordFailuresSinceLastSuccess(userName) > allowedPasswordAttempts &&
                    provider.GetLastPasswordFailureDate(userName).Add(interval) > DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the password failures since last success.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            return VerifyProvider().GetPasswordFailuresSinceLastSuccess(userName);
        }

        /// <summary>
        /// Gets the create date.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static DateTime GetCreateDate(string userName)
        {
            return VerifyProvider().GetCreateDate(userName);
        }

        /// <summary>
        /// Gets the password changed date.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static DateTime GetPasswordChangedDate(string userName)
        {
            return VerifyProvider().GetPasswordChangedDate(userName);
        }

        /// <summary>
        /// Gets the last password failure date.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static DateTime GetLastPasswordFailureDate(string userName)
        {
            return VerifyProvider().GetLastPasswordFailureDate(userName);
        }
    }
}
