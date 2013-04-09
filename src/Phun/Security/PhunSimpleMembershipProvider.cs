namespace Phun.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Security;

    using Phun.Data;

    using WebMatrix.WebData;

    /// <summary>
    /// Copied from here: https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/WebMatrix.WebData/SimpleMembershipProvider.cs
    /// Apache License copyright 2.0 blah blah...
    /// to support more database engine.  This is one horribly big file.  Somebody should be smacked with a large trout.
    /// Half way through I feel like a coding monkey, the rest of the CMS feel like it's not even this big.  GRRRRR!!!!
    /// </summary>
    public class PhunSimpleMembershipProvider : ExtendedMembershipProvider
    {
        /// <summary>
        /// The token size in bytes
        /// </summary>
        private const int TokenSizeInBytes = 16;

        /// <summary>
        /// The _previous provider
        /// </summary>
        private readonly MembershipProvider _previousProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunSimpleMembershipProvider"/> class.
        /// </summary>
        public PhunSimpleMembershipProvider()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunSimpleMembershipProvider"/> class.
        /// </summary>
        /// <param name="previousProvider">The previous provider.</param>
        public PhunSimpleMembershipProvider(MembershipProvider previousProvider)
        {
            this._previousProvider = previousProvider;
            if (this._previousProvider != null)
            {
                this._previousProvider.ValidatingPassword += (sender, args) =>
                    {
                        if (!this.InitializeCalled)
                        {
                            this.OnValidatingPassword(args);
                        }
                    };
            }
        }

        /// <summary>
        /// Gets the previous provider.
        /// </summary>
        /// <value>
        /// The previous provider.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Initialize must be called first.</exception>
        private MembershipProvider PreviousProvider
        {
            get
            {
                if (this._previousProvider == null)
                {
                    throw new InvalidOperationException("Initialize must be called first.");
                }
                else
                {
                    return this._previousProvider;
                }
            }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>
        public override bool EnablePasswordRetrieval
        {
            get
            {
                return this.InitializeCalled ? false : this.PreviousProvider.EnablePasswordRetrieval;
            }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset
        {
            get
            {
                return this.InitializeCalled ? false : this.PreviousProvider.EnablePasswordReset;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return this.InitializeCalled ? false : this.PreviousProvider.RequiresQuestionAndAnswer;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
        public override bool RequiresUniqueEmail
        {
            get
            {
                return this.InitializeCalled ? false : this.PreviousProvider.RequiresUniqueEmail;
            }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat" /> values indicating the format for storing passwords in the data store.</returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return this.InitializeCalled ? MembershipPasswordFormat.Hashed : this.PreviousProvider.PasswordFormat;
            }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return this.InitializeCalled ? Int32.MaxValue : this.PreviousProvider.MaxInvalidPasswordAttempts;
            }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
        public override int PasswordAttemptWindow
        {
            get
            {
                return this.InitializeCalled ? Int32.MaxValue : this.PreviousProvider.PasswordAttemptWindow;
            }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <returns>The minimum length required for a password. </returns>
        public override int MinRequiredPasswordLength
        {
            get
            {
                return this.InitializeCalled ? 0 : this.PreviousProvider.MinRequiredPasswordLength;
            }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return this.InitializeCalled ? 0 : this.PreviousProvider.MinRequiredNonAlphanumericCharacters;
            }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <returns>A regular expression used to evaluate a password.</returns>
        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return this.InitializeCalled ? String.Empty : this.PreviousProvider.PasswordStrengthRegularExpression;
            }
        }

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <returns>The name of the application using the custom membership provider.</returns>
        /// <exception cref="System.NotSupportedException">
        /// </exception>
        public override string ApplicationName
        {
            get
            {
                if (this.InitializeCalled)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    return this.PreviousProvider.ApplicationName;
                }
            }
            set
            {
                if (this.InitializeCalled)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    this.PreviousProvider.ApplicationName = value;
                }
            }
        }

        /// <summary>
        /// Gets the name of the membership table.
        /// </summary>
        /// <value>
        /// The name of the membership table.
        /// </value>
        public virtual string MembershipTableName
        {
            get
            {
                return "simplemembership";
            }
        }

        /// <summary>
        /// Gets the name of the O auth membership table.
        /// </summary>
        /// <value>
        /// The name of the O auth membership table.
        /// </value>
        public virtual string OAuthMembershipTableName
        {
            get
            {
                return "simpleoauthmembership";
            }
        }

        /// <summary>
        /// Gets the name of the O auth token table.
        /// </summary>
        /// <value>
        /// The name of the O auth token table.
        /// </value>
        public virtual string OAuthTokenTableName
        {
            get
            {
                return "simpleoauthtoken";
            }
        }

        /// <summary>
        /// Gets or sets the name of the user table.
        /// </summary>
        /// <value>
        /// The name of the user table.
        /// </value>
        public string UserTableName { get; set; }

        /// <summary>
        /// Gets or sets the user name column.
        /// </summary>
        /// <value>
        /// The user name column.
        /// </value>
        public string UserNameColumn { get; set; }

        /// <summary>
        /// Gets or sets the user id column.
        /// </summary>
        /// <value>
        /// The user id column.
        /// </value>
        public string UserIdColumn { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection string.
        /// </summary>
        /// <value>
        /// The name of the connection string.
        /// </value>
        public string ConnectionStringName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [initialize called].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [initialize called]; otherwise, <c>false</c>.
        /// </value>
        protected internal virtual bool InitializeCalled { get; set; }

        /// <summary>
        /// Verifies the initialized.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Initialize must be called first.</exception>
        protected internal virtual void VerifyInitialized()
        {
            if (!this.InitializeCalled)
            {
                throw new InvalidOperationException("Initialize must be called first.");
            }
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="System.ArgumentNullException">config</exception>
        /// <exception cref="System.Configuration.Provider.ProviderException"></exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "PhunSimpleMembershipProvider";
            }

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "PhunCms Simple Membership Provider");
            }

            base.Initialize(name, config);

            config.Remove("connectionStringName");
            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("applicationName");
            config.Remove("requiresUniqueEmail");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("passwordFormat");
            config.Remove("name");
            config.Remove("description");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            config.Remove("hashAlgorithmType");

            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);

                if (!string.IsNullOrEmpty(attribUnrecognized))
                {
                    throw new ProviderException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Provider configuration attribute '{0}' is not recognized.",
                            attribUnrecognized));
                }
            }
        }

        /// <summary>
        /// Creates the tables if needed.
        /// </summary>
        protected internal virtual void CreateTablesIfNeeded()
        {
            using (var db = this.ConnectToDatabase())
            {
                if (!db.TableExists(this.UserTableName))
                {
                    var sql =
                        string.Format(this.profileTableSql, this.UserTableName, this.UserIdColumn, this.UserNameColumn)
                              .Split(new string[] {"GO"}, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var sqlString in sql)
                    {
                        db.ExecuteSchema(sqlString);
                    }
                }

                if (!db.TableExists(OAuthMembershipTableName))
                {
                    db.ExecuteSchema(
                        string.Format(
                            "create table {0} ( Provider varchar(50) not null, ProviderUserId varchar(128) not null, UserId integer not null, primary key (Provider, ProviderUserId))",
                            this.OAuthMembershipTableName));
                }

                if (!db.TableExists(MembershipTableName))
                {
                    db.ExecuteSchema(string.Format(@"
create table {0} (                           
    UserId                                  integer primary key not null, 
    ConfirmationToken                       varchar(128) null, 
    CreateDate                              timestamp null, 
    LastPasswordFailureDate                 timestamp null, 
    Password                                varchar(128) not null, 
    PasswordChangedDate                     timestamp null, 
    PasswordFailureCount                    integer not null, 
    PasswordSalt                            varchar(128) not null, 
    PasswordVerifyToken                     varchar(128) null, 
    PasswordVerifyTokenExpireDate           timestamp null, 
    ConfirmDate                             timestamp null
)", this.MembershipTableName));
                }
            }
        }

        /// <summary>
        /// Creates the O auth token table if needed.
        /// </summary>
        /// <param name="db">The db.</param>
        private void CreateOAuthTokenTableIfNeeded(DapperContext db)
        {
            if (!db.TableExists(OAuthTokenTableName))
            {
                db.ExecuteSchema(
                    string.Format(
                        "create table {0} (Secret varchar(128) not null, Token varchar(128) not null primary key)",
                        this.OAuthTokenTableName));
            }
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public int GetUserId(string userName)
        {
            this.VerifyInitialized();

            using (var db = this.ConnectToDatabase())
            {
                return this.GetUserId(db, UserTableName, UserNameColumn, UserIdColumn, userName);
            }
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="userTableName">Name of the user table.</param>
        /// <param name="userNameColumn">The user name column.</param>
        /// <param name="userIdColumn">The user id column.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns>The user id.</returns>
        protected internal virtual int GetUserId(
            DapperContext db, string userTableName, string userNameColumn, string userIdColumn, string userName)
        {
            var result =
                db.Query<int>(
                    string.Format(
                        "SELECT {0} FROM {1} WHERE {2} = @UserName", userIdColumn, userTableName, userNameColumn),
                    new { UserName = userNameColumn }).FirstOrDefault();

            return result > 0 ? result : -1;
        }

        /// <summary>
        /// When overridden in a derived class, returns an ID for a user based on a password reset token.
        /// </summary>
        /// <param name="token">The password reset token.</param>
        /// <returns>
        /// The user ID.
        /// </returns>
        public override int GetUserIdFromPasswordResetToken(string token)
        {
            this.VerifyInitialized();

            using (var db = this.ConnectToDatabase())
            {
                var result =
                    db.Query<int>(
                        string.Format("SELECT UserId FROM {0} WHERE (PasswordVerifyToken = @Token)", this.MembershipTableName), new { Token = token })
                      .FirstOrDefault();
                return result > 0 ? result : -1;
            }
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <param name="username">The user to change the password question and answer for.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <param name="newPasswordQuestion">The new password question for the specified user.</param>
        /// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override bool ChangePasswordQuestionAndAnswer(
            string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.ChangePasswordQuestionAndAnswer(
                    username, password, newPasswordQuestion, newPasswordAnswer);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the confirmed flag for the username if it is correct.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="accountConfirmationToken">A confirmation token to pass to the authentication provider.</param>
        /// <returns>
        /// True if the account could be successfully confirmed. False if the username was not found or the confirmation token is invalid.
        /// </returns>
        /// <remarks>
        /// Inherited from ExtendedMembershipProvider ==&gt; Simple Membership MUST be enabled to use this method
        /// </remarks>
        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            this.VerifyInitialized();
            using (var db = this.ConnectToDatabase())
            {
                // We need to compare the token using a case insensitive comparison however it seems tricky to do this uniformly across databases when representing the token as a string. 
                // Therefore verify the case on the client
                var row =
                    db.Query<dynamic>(
                        string.Format(
                            "SELECT {0}.UserId, {0}.ConfirmationToken FROM {0} JOIN {1} ON {0}.UserId = {1}.{2} WHERE {0}.ConfirmationToken = @Token AND {1}.{3} = @UserName",
                            this.MembershipTableName,
                            this.UserTableName,
                            this.UserIdColumn,
                            this.UserNameColumn),
                        new { UserName = userName, Token = accountConfirmationToken }).FirstOrDefault();
   
                if (row == null)
                {
                    return false;
                }

                if (string.Equals(accountConfirmationToken, row.ConfirmationToken, StringComparison.Ordinal))
                {
                    db.Execute(string.Format("UPDATE {0} SET ConfirmDate = TIMESTAMP WHERE UserId = @UserId", this.MembershipTableName), new { UserId = int.Parse(row.UserId + string.Empty) });
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Sets the confirmed flag for the username if it is correct.
        /// </summary>
        /// <param name="accountConfirmationToken">A confirmation token to pass to the authentication provider.</param>
        /// <returns>
        /// True if the account could be successfully confirmed. False if the username was not found or the confirmation token is invalid.
        /// </returns>
        /// <remarks>
        /// Inherited from ExtendedMembershipProvider ==&gt; Simple Membership MUST be enabled to use this method.
        /// There is a tiny possibility where this method fails to work correctly. Two or more users could be assigned the same token but specified using different cases.
        /// A workaround for this would be to use the overload that accepts both the user name and confirmation token.
        /// </remarks>
        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            this.VerifyInitialized();
            using (var db = this.ConnectToDatabase())
            {
                // We need to compare the token using a case insensitive comparison however it seems tricky to do this uniformly across databases when representing the token as a string. 
                // Therefore verify the case on the client
                var row =
                    db.Query<dynamic>(
                       string.Format("SELECT UserId, ConfirmationToken FROM {0} WHERE ConfirmationToken = @Token", this.MembershipTableName),
                       new {Token = accountConfirmationToken}).FirstOrDefault();

                if (row == null)
                {
                    return false;
                }

                if (string.Equals(accountConfirmationToken, row.ConfirmationToken, StringComparison.Ordinal))
                {
                    db.Execute(string.Format("UPDATE {0} SET ConfirmDate = TIMESTAMP WHERE UserId = @UserId", this.MembershipTableName), new { UserId = int.Parse(row.UserId + string.Empty) });
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Connects to database.
        /// </summary>
        /// <returns></returns>
        internal virtual DapperContext ConnectToDatabase()
        {
            return new DapperContext(this.ConnectionStringName);
        }

        /// <summary>
        /// When overridden in a derived class, creates a new user account using the specified user name and password, optionally requiring that the new account must be confirmed before the account is available for use.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="requireConfirmationToken">(Optional) true to specify that the account must be confirmed; otherwise, false. The default is false.</param>
        /// <returns>
        /// A token that can be sent to the user to confirm the account.
        /// </returns>
        /// <exception cref="System.Web.Security.MembershipCreateUserException">
        /// </exception>
        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            VerifyInitialized();

            if (string.IsNullOrEmpty(password))
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            string hashedPassword = Crypto.HashPassword(password);
            if (hashedPassword.Length > 128)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            if (string.IsNullOrEmpty(userName))
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
            }

            using (var db = this.ConnectToDatabase())
            {
                // Step 1: Check if the user exists in the Users table
                int uid = this.GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, userName);
                if (uid == -1)
                {
                    // User not found
                    throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
                }

                // Step 2: Check if the user exists in the Membership table: Error if yes.
                var result = db.Query<int>(string.Format("SELECT COUNT(*) FROM {0} WHERE UserId = @UserId", this.MembershipTableName), new { UserId = uid }).FirstOrDefault();
                if (result > 0)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.DuplicateUserName);
                }

                // Step 3: Create user in Membership table
                string token = null;
                object dbtoken = DBNull.Value;
                if (requireConfirmationToken)
                {
                    token = this.GenerateToken();
                    dbtoken = token;
                }

                int defaultNumPasswordFailures = 0;

                db.Execute(
                    string.Format(
                        "INSERT INTO {0} (UserId, Password, PasswordSalt, ConfirmationToken, CreateDate, PasswordChangedDate, PasswordFailureCount) VALUES (@UserId, @Password, @PasswordSalt, @ConfirmationToken, @CreateDate, @PasswordChangedDate, @PasswordFailureCount)",
                        this.MembershipTableName),
                    new
                        {
                            UserId = uid,
                            Password = hashedPassword,
                            PasswordSalt = string.Empty,
                            ConfirmationToken = dbtoken,
                            PasswordChangedDate = DateTime.UtcNow,
                            PasswordFailureCount = defaultNumPasswordFailures
                        });
                return token;
            }
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus" /> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the information for the newly created user.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override MembershipUser CreateUser(
            string username,
            string password,
            string email,
            string passwordQuestion,
            string passwordAnswer,
            bool isApproved,
            object providerUserKey,
            out MembershipCreateStatus status)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.CreateUser(
                    username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, creates a new user profile and a new membership account.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="requireConfirmation">(Optional) true to specify that the user account must be confirmed; otherwise, false. The default is false.</param>
        /// <param name="values">(Optional) A dictionary that contains additional user attributes to store in the user profile. The default is null.</param>
        /// <returns>
        /// A token that can be sent to the user to confirm the user account.
        /// </returns>
        public override string CreateUserAndAccount(
            string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            this.VerifyInitialized();

            using (var db = this.ConnectToDatabase())
            {
                // Make sure user doesn't exist
                int userId = this.GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, userName);
                if (userId != -1)
                {
                    throw new MembershipCreateUserException(MembershipCreateStatus.DuplicateUserName);
                }

                if (values != null && values.Keys.Count > 0)
                {
                    throw new MembershipCreateUserException("Create method for custom user table is not implemented.  Please provide your own user creation method.");
                }

                // simple create
                db.Execute(string.Format("INSERT INTO {0} ({1}) VALUES (@UserName)", this.UserTableName, this.UserNameColumn), new { UserName = userName });
                return this.CreateAccount(userName, password, requireConfirmation);
            }
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override string GetPassword(string username, string answer)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.GetPassword(username, answer);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        private bool SetPassword(DapperContext db, int userId, string newPassword)
        {
            string hashedPassword = Crypto.HashPassword(newPassword);
            if (hashedPassword.Length > 128)
            {
                throw new ArgumentException("Password is too long.");
            }

            // Update new password
                db.Execute(
                    string.Format(
                        "UPDATE {0} SET Password= @Password, PasswordSalt = @PasswordSalt, PasswordChangedDate = @ChangeDate WHERE UserId = @UserId", this.MembershipTableName),
                        new
                            {
                                Password = hashedPassword,
                                PasswordSalt = string.Empty,
                                ChangeDate = DateTime.UtcNow,
                                UserId = userId
                            });
            return true;
        }

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">The current password for the specified user.</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// username
        /// or
        /// oldPassword
        /// or
        /// newPassword
        /// </exception>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.ChangePassword(username, oldPassword, newPassword);
            }

            // REVIEW: are commas special in the password?
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "username");
            }
            if (string.IsNullOrEmpty(oldPassword))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "oldPassword");
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "newPassword");
            }

            using (var db = ConnectToDatabase())
            {
                int userId = this.GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, username);
                if (userId == -1)
                {
                    return false; // User not found
                }

                // First check that the old credentials match
                if (!this.CheckPassword(db, userId, oldPassword))
                {
                    return false;
                }

                return this.SetPassword(db, userId, newPassword);
            }
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user.</param>
        /// <returns>
        /// The new password for the specified user.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override string ResetPassword(string username, string answer)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.ResetPassword(username, answer);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the specified user's information from the data source.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.GetUser(providerUserKey, userIsOnline);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.GetUser(username, userIsOnline);
            }

            // Due to a bug in v1, GetUser allows passing null / empty values.
            using (var db = this.ConnectToDatabase())
            {
                int userId = this.GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, username);
                if (userId == -1)
                {
                    return null; // User not found
                }

                return new MembershipUser(
                    Membership.Provider.Name,
                    username,
                    userId,
                    null,
                    null,
                    null,
                    true,
                    false,
                    DateTime.MinValue,
                    DateTime.MinValue,
                    DateTime.MinValue,
                    DateTime.MinValue,
                    DateTime.MinValue);
            }
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override string GetUserNameByEmail(string email)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.GetUserNameByEmail(email);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, deletes the specified membership account.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>
        /// true if the user account was deleted; otherwise, false.
        /// </returns>
        public override bool DeleteAccount(string userName)
        {
            VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, userName);
                if (userId == -1)
                {
                    return false;
                }

                db.Execute(string.Format("DELETE FROM {0} WHERE UserId = @UserId", this.MembershipTableName), new { UserId = userId });
                return true;
            }
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteUser(string userName, bool deleteAllRelatedData)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.DeleteUser(userName, deleteAllRelatedData);
            }

            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, userName);
                if (userId == -1)
                {
                    return false;
                }

                db.Execute(string.Format("DELETE FROM {0} WHERE {1} = @UserId", this.UserTableName, this.UserIdColumn), new {UserId = userId});
                return true;
            }
        }

        /// <summary>
        /// Deletes the user and account internal.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        internal bool DeleteUserAndAccountInternal(string userName)
        {
            return (DeleteAccount(userName) && DeleteUser(userName, false));
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex" /> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection" /> collection that contains a page of <paramref name="pageSize" /><see cref="T:System.Web.Security.MembershipUser" /> objects beginning at the page specified by <paramref name="pageIndex" />.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.GetAllUsers(pageIndex, pageSize, out totalRecords);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override int GetNumberOfUsersOnline()
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.GetNumberOfUsersOnline();
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex" /> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection" /> collection that contains a page of <paramref name="pageSize" /><see cref="T:System.Web.Security.MembershipUser" /> objects beginning at the page specified by <paramref name="pageIndex" />.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override MembershipUserCollection FindUsersByName(
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex" /> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection" /> collection that contains a page of <paramref name="pageSize" /><see cref="T:System.Web.Security.MembershipUser" /> objects beginning at the page specified by <paramref name="pageIndex" />.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override MembershipUserCollection FindUsersByEmail(
            string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the password failures since last success.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        private int GetPasswordFailureCount(DapperContext db, int userId)
        {
            var failure =
                db.Query<int>(
                    string.Format("SELECT PasswordFailureCount FROM {0} WHERE UserId = @UserId", this.MembershipTableName), new { User = userId}).FirstOrDefault();

            return failure >= 0 ? failure : -1;
        }

        /// <summary>
        /// When overridden in a derived class, returns the number of times that the password for the specified user account was incorrectly entered since the most recent successful login or since the user account was created.
        /// </summary>
        /// <param name="userName">The user name of the account.</param>
        /// <returns>
        /// The count of failed password attempts for the specified user account.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            using (var db = this.ConnectToDatabase())
            {
                int userId = GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, userName);
                if (userId == -1)
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, "User not found '{0}'", userName));
                }

                return this.GetPasswordFailureCount(db, userId);
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns the date and time when the specified user account was created.
        /// </summary>
        /// <param name="userName">The user name of the account.</param>
        /// <returns>
        /// The date and time the account was created, or <see cref="F:System.DateTime.MinValue" /> if the account creation date is not available.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override DateTime GetCreateDate(string userName)
        {
            using (var db = ConnectToDatabase())
            {
                int userId = GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, userName);
                if (userId == -1)
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, "User not found '{0}'", userName));
                }

                var createDate =
                    db.Query<DateTime?>(string.Format("SELECT CreateDate FROM {0} WHERE UserId = @UserId", this.MembershipTableName), new { UserId = userId }).FirstOrDefault();
                return createDate.HasValue ? createDate.Value : DateTime.MinValue;
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns the date and time when the password was most recently changed for the specified membership account.
        /// </summary>
        /// <param name="userName">The user name of the account.</param>
        /// <returns>
        /// The date and time when the password was more recently changed for membership account, or <see cref="F:System.DateTime.MinValue" /> if the password has never been changed for this user account.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override DateTime GetPasswordChangedDate(string userName)
        {
            using (var db = this.ConnectToDatabase())
            {
                int userId = GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, userName);
                if (userId == -1)
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, "User not found '{0}'", userName));
                }

                var pwdChangeDate =
                    db.Query<DateTime?>(string.Format("SELECT PasswordChangedDate FROM {0} WHERE UserId = @UserId", this.MembershipTableName), new { UserId =  userId }).FirstOrDefault();
                return pwdChangeDate.HasValue ? pwdChangeDate.Value : DateTime.MinValue;
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns the date and time when an incorrect password was most recently entered for the specified user account.
        /// </summary>
        /// <param name="userName">The user name of the account.</param>
        /// <returns>
        /// The date and time when an incorrect password was most recently entered for this user account, or <see cref="F:System.DateTime.MinValue" /> if an incorrect password has not been entered for this user account.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            using (var db = this.ConnectToDatabase())
            {
                int userId = GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, userName);
                if (userId == -1)
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, "User not found '{0}'", userName));
                }

                var failureDate =
                    db.Query<DateTime?>(string.Format("SELECT LastPasswordFailureDate FROM {0} WHERE UserId = @UserId", this.MembershipTableName), new { UserId =  userId }).FirstOrDefault();
                
                return failureDate.HasValue ? failureDate.Value : DateTime.MinValue;
            }
        }

        /// <summary>
        /// Checks the password.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private bool CheckPassword(DapperContext db, int userId, string password)
        {
            string hashedPassword = GetHashedPassword(db, userId);
            bool verificationSucceeded = (hashedPassword != null
                                          && Crypto.VerifyHashedPassword(hashedPassword, password));
            if (verificationSucceeded)
            {
                // Reset password failure count on successful credential check
                db.Execute(string.Format("UPDATE {0} SET PasswordFailureCount = 0 WHERE UserId = @UserId", this.MembershipTableName),
                    new { UserId = userId });
            }
            else
            {
                int failures = this.GetPasswordFailureCount(db, userId);
                if (failures != -1)
                {
                    db.Execute(
                        string.Format(
                            "UPDATE {0} SET PasswordFailureCount = @Failures, LastPasswordFailureDate = @LastFailureDate WHERE UserId = @UserId",
                            this.MembershipTableName),
                        new { UserId = userId, Failures = failures + 1, LastFailureDate = DateTime.UtcNow });
                }
            }

            return verificationSucceeded;
        }

        /// <summary>
        /// Gets the hashed password.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>
        /// Hashed password.
        /// </returns>
        private string GetHashedPassword(DapperContext db, int userId)
        {
            var pwdQuery =
                db.Query<string>(
                    string.Format("SELECT Password FROM {0} WHERE UserId = @UserId", this.MembershipTableName),
                    new { UserId = userId }).ToList();

            if (pwdQuery.Any())
            {
                return pwdQuery.First();
            }

            return null;
        }

        /// <summary>
        /// Verifies the user name has confirmed account.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="username">The username.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        private int VerifyUserNameHasConfirmedAccount(DapperContext db, string username, bool throwException)
        {
            int userId = this.GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, username);
            if (userId == -1)
            {
                if (throwException)
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, "User '{0}' not found.", username));
                }
                else
                {
                    return -1;
                }
            }

            int result =
                db.Query<int>(string.Format("SELECT COUNT(*) FROM {0} WHERE UserId = @UserId AND ConfirmDate is not null", this.MembershipTableName), new {UserId = userId}).FirstOrDefault();
            if (result == 0)
            {
                if (throwException)
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, "No account found with user name '{0}'", username));
                }
                else
                {
                    return -1;
                }
            }
            return userId;
        }

        /// <summary>
        /// Generates the token.
        /// </summary>
        /// <returns></returns>
        private string GenerateToken()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                return this.GenerateToken(prng);
            }
        }

        /// <summary>
        /// Generates the token.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <returns></returns>
        internal string GenerateToken(RandomNumberGenerator generator)
        {
            byte[] tokenBytes = new byte[TokenSizeInBytes];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }

        // Inherited from ExtendedMembershipProvider ==> Simple Membership MUST be enabled to use this method
        /// <summary>
        /// When overridden in a derived class, generates a password reset token that can be sent to a user in email.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="tokenExpirationInMinutesFromNow">(Optional) The time, in minutes, until the password reset token expires. The default is 1440 (24 hours).</param>
        /// <returns>
        /// A token to send to the user.
        /// </returns>
        /// <exception cref="System.ArgumentException">Argument cannot be null or empty.;userName</exception>
        /// <exception cref="System.Configuration.Provider.ProviderException">Unable to update simple membership database.</exception>
        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            this.VerifyInitialized();
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "userName");
            }

            using (var db = this.ConnectToDatabase())
            {
                int userId = this.VerifyUserNameHasConfirmedAccount(db, userName, true);

                var token =
                    db.Query<string>(string.Format("SELECT PasswordVerifyToken FROM {0} WHERE UserId = @UserId AND PasswordVerifyTokenExpireDate > @ExpireDate", this.MembershipTableName),
                        new { UserId = userId, ExpireDate = DateTime.UtcNow }).FirstOrDefault();
                if (string.IsNullOrEmpty(token))
                {
                    token = this.GenerateToken();

                    db.Execute(string.Format("UPDATE {0} SET PasswordVerifyToken = @Token, PasswordVerifyTokenExpireDate = @ExpireDate WHERE UserId = @UserId", this.MembershipTableName),
                        new { UserId = userId, ExpireDate = DateTime.UtcNow.AddMinutes(tokenExpirationInMinutesFromNow), Token = token });
                   
                }

                return token;
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns a value that indicates whether the user account has been confirmed by the provider.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>
        /// true if the user is confirmed; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentException">userName</exception>
        public override bool IsConfirmed(string userName)
        {
            VerifyInitialized();
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "userName");
            }

            using (var db = this.ConnectToDatabase())
            {
                int userId = this.VerifyUserNameHasConfirmedAccount(db, userName, false);
                return (userId != -1);
            }
        }

        /// <summary>
        /// When overridden in a derived class, resets a password after verifying that the specified password reset token is valid.
        /// </summary>
        /// <param name="token">A password reset token.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>
        /// true if the password was changed; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentException">newPassword</exception>
        /// <exception cref="System.Configuration.Provider.ProviderException"></exception>
        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            this.VerifyInitialized();

            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "newPassword");
            }

            using (var db = this.ConnectToDatabase())
            {
                var userId =
                    db.Query<int>(
                        string.Format(
                            "SELECT UserId FROM {0} WHERE PasswordVerifyToken = @Token AND PasswordVerifyTokenExpirationDate > @ExpireDate",
                            this.MembershipTableName),
                        new { Token = token, ExpireDate = DateTime.UtcNow }).FirstOrDefault();
                if (userId > 0)
                {
                    bool success = this.SetPassword(db, userId, newPassword);
                    if (success)
                    {
                        // Clear the Token on success
                        db.Execute(
                                string.Format(
                                    "UPDATE {0} SET PasswordVerifyToken = NULL, PasswordVerifyTokenExpirationDate = NULL WHERE UserId = @UserId",
                                    this.MembershipTableName),
                                new { UserId = userId });
                    }
                    return success;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser" /> object that represents the user to update and the updated information for the user.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void UpdateUser(MembershipUser user)
        {
            if (!this.InitializeCalled)
            {
                this.PreviousProvider.UpdateUser(user);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="userName">The membership user whose lock status you want to clear.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override bool UnlockUser(string userName)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.UnlockUser(userName);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Validates the user table.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        internal void ValidateUserTable()
        {
            using (var db = this.ConnectToDatabase())
            {
                // GetUser will fail with an exception if the user table isn't set up properly
                try
                {
                    this.GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, "z");
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.InvariantCulture, "Failed to find user table '{0}'.", this.UserTableName),
                        e);
                }
            }
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// username
        /// or
        /// password
        /// </exception>
        public override bool ValidateUser(string username, string password)
        {
            if (!InitializeCalled)
            {
                return this.PreviousProvider.ValidateUser(username, password);
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "username");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "password");
            }

            using (var db = this.ConnectToDatabase())
            {
                int userId = this.VerifyUserNameHasConfirmedAccount(db, username, throwException: false);
                if (userId == -1)
                {
                    return false;
                }
                else
                {
                    return this.CheckPassword(db, userId, password);
                }
            }
        }

        /// <summary>
        /// Returns the user name that is associated with the specified user ID.
        /// </summary>
        /// <param name="userId">The user ID to get the name for.</param>
        /// <returns>
        /// The user name.
        /// </returns>
        public override string GetUserNameFromId(int userId)
        {
            this.VerifyInitialized();

            using (var db = this.ConnectToDatabase())
            {
                var username =
                    db.Query<string>(
                        string.Format(
                            "SELECT {0} FROM {1} WHERE {2} = @UserId",
                            this.UserNameColumn,
                            this.UserTableName,
                            this.UserIdColumn),
                        new { UserId = userId }).FirstOrDefault();

                return username;
            }
        }

        /// <summary>
        /// Creates the or update O auth account.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <param name="userName">Name of the user.</param>
        public override void CreateOrUpdateOAuthAccount(string provider, string providerUserId, string userName)
        {
            this.VerifyInitialized();

            if (string.IsNullOrEmpty(userName))
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.ProviderError);
            }

            int userId = this.GetUserId(userName);
            if (userId == -1)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
            }

            var oldUserId = this.GetUserIdFromOAuth(provider, providerUserId);
            using (var db = this.ConnectToDatabase())
            {
                if (oldUserId == -1)
                {
                    // account doesn't exist. create a new one.
                    db.Execute(
                        string.Format(
                            "INSERT INTO {0} (Provider, ProviderUserId, UserId) VALUES (@Provider, @ProviderUserId, @UserId)",
                            this.OAuthMembershipTableName),
                        new { Provider = provider, ProviderId = providerUserId, UserId = userId });

                }
                else
                {
                    db.Execute(
                        string.Format(
                            "UPDATE {0} SET UserId = @UserId WHERE Provider = @ProviderId AND ProviderUserId = @ProviderUserId",
                            this.OAuthMembershipTableName),
                            new { Provider = provider, ProviderId = providerUserId, UserId = userId });
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, deletes the OAuth or OpenID account with the specified provider name and provider user ID.
        /// </summary>
        /// <param name="provider">The name of the OAuth or OpenID provider.</param>
        /// <param name="providerUserId">The OAuth or OpenID provider user ID. This is not the user ID of the user account, but the user ID on the OAuth or Open ID provider.</param>
        public override void DeleteOAuthAccount(string provider, string providerUserId)
        {
            this.VerifyInitialized();

            using (var db = this.ConnectToDatabase())
            {
                db.Execute(
                    string.Format(
                        "DELETE FROM {0} WHERE Provider = @Provider AND ProviderUserId = @ProviderId",
                            this.OAuthMembershipTableName),
                        new { Provider = provider, ProviderId = providerUserId });
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns the user ID for the specified OAuth or OpenID provider and provider user ID.
        /// </summary>
        /// <param name="provider">The name of the OAuth or OpenID provider.</param>
        /// <param name="providerUserId">The OAuth or OpenID provider user ID. This is not the user ID of the user account, but the user ID on the OAuth or Open ID provider.</param>
        /// <returns>User Id.</returns>
        public override int GetUserIdFromOAuth(string provider, string providerUserId)
        {
            this.VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                var id =
                    db.Query<int>(
                        string.Format(
                            "SELECT UserId FROM {0} WHERE Provider = @Provider AND ProviderUserId = @ProviderId",
                            this.OAuthMembershipTableName),
                        new { Provider = provider, ProviderId = providerUserId }).FirstOrDefault();

                return id > 0 ? (int)id : -1;
            }
        }

        /// <summary>
        /// Gets the O auth token secret.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The secret.</returns>
        public override string GetOAuthTokenSecret(string token)
        {
            this.VerifyInitialized();

            using (var db = this.ConnectToDatabase())
            {
                this.CreateOAuthTokenTableIfNeeded(db);

                // Note that token is case-sensitive depend on database engine?
                var secret =
                    db.Query<string>(
                        string.Format("SELECT Secret FROM {0} WHERE Token = @Token", this.OAuthTokenTableName),
                        new { Token = token }).FirstOrDefault();
                return secret;
            }
        }

        /// <summary>
        /// Stores the O auth request token.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="requestTokenSecret">The request token secret.</param>
        public override void StoreOAuthRequestToken(string requestToken, string requestTokenSecret)
        {
            this.VerifyInitialized();

            var existingSecret = this.GetOAuthTokenSecret(requestToken);
            if (existingSecret != null)
            {
                if (existingSecret == requestTokenSecret)
                {
                    // the record already exists
                    return;
                }

                using (var db = this.ConnectToDatabase())
                {
                    this.CreateOAuthTokenTableIfNeeded(db);

                    // the token exists with old secret, update it to new secret
                    db.Execute(
                        string.Format(
                            "UPDATE {0} SET Secret = @Secret WHERE Token = @Token",
                            this.OAuthTokenTableName),
                            new { Secret = requestTokenSecret, Token = requestToken });
                }
            }
            else
            {
                using (var db = this.ConnectToDatabase())
                {
                    this.CreateOAuthTokenTableIfNeeded(db);

                    // insert new record
                    db.Execute(string.Format("INSERT INTO {0} (Token, Secret) VALUES(@Token, @Secret)", this.OAuthTokenTableName), new { Secret = requestTokenSecret, Token = requestToken });
                }
            }
        }

        /// <summary>
        /// Replaces the request token with access token and secret.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="accessTokenSecret">The access token secret.</param>
        public override void ReplaceOAuthRequestTokenWithAccessToken(
            string requestToken, string accessToken, string accessTokenSecret)
        {
            this.VerifyInitialized();

            using (var db = ConnectToDatabase())
            {
                this.CreateOAuthTokenTableIfNeeded(db);

                // delete token
                db.Execute(string.Format("DELETE FROM {0} WHERE Token = @Token", this.OAuthTokenTableName), new { Token = requestToken });

                // Although there are two different types of tokens, request token and access token,
                // we treat them the same in database records.
                this.StoreOAuthRequestToken(accessToken, accessTokenSecret);
            }
        }

        /// <summary>
        /// Deletes the OAuth token from the backing store from the database.
        /// </summary>
        /// <param name="token">The token to be deleted.</param>
        public override void DeleteOAuthToken(string token)
        {
            this.VerifyInitialized();

            using (var db = this.ConnectToDatabase())
            {
                this.CreateOAuthTokenTableIfNeeded(db);

                // Note that token is case-sensitive
                db.Execute(
                    string.Format("DELETE FROM {0} WHERE Token = @Token", this.OAuthTokenTableName), new { Token = token });
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns all OAuth membership accounts associated with the specified user name.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>
        /// A list of all OAuth membership accounts associated with the specified user name.
        /// </returns>
        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            this.VerifyInitialized();
            var accounts = new List<OAuthAccountData>();

            var userId = this.GetUserId(userName);
            if (userId != -1)
            {
                using (var db = this.ConnectToDatabase())
                {
                    var records =
                        db.Query<dynamic>(
                            string.Format(
                                "SELECT Provider, ProviderUserId FROM {0} WHERE UserId = @UserId",
                                this.OAuthMembershipTableName),
                            new { UserId = userId });

                    accounts.AddRange(records.Select(row => new OAuthAccountData(row.Provider, row.ProviderUserId)));
                }
            }

            return accounts;
        }

        /// <summary>
        /// Determines whether there exists a local account (as opposed to OAuth account) with the specified userId.
        /// </summary>
        /// <param name="userId">The user id to check for local account.</param>
        /// <returns>
        ///   <c>true</c> if there is a local account with the specified user id]; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasLocalAccount(int userId)
        {
            this.VerifyInitialized();

            using (var db = this.ConnectToDatabase())
            {
                return
                    db.Query<int>(
                        string.Format("SELECT UserId FROM {0} WHERE UserId = @UserId", this.MembershipTableName),
                        new { UserId = userId }).FirstOrDefault() > 0;
            }
        }

        /// <summary>
        /// The profile table SQL
        /// </summary>
        private readonly string profileTableSql = @"
create table {0}
(
    {1} serial primary key not null, 
    {2} varchar(56) unique not null
)

GO
       
create sequence seq_{0} increment by 1 start with 1 nomaxvalue nocycle;

GO
      
create or replace trigger trg_{0}
before insert on {0}
for each row
begin           
  if :new.{1} is null then
    select seq_{0}.nextval into :new.{1} from dual;
  end if;
end;
";
    }
}

