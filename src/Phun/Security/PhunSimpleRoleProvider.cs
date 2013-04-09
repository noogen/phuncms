namespace Phun.Security
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Linq;
    using System.Web.Security;

    using Phun.Data;

    using WebMatrix.WebData;

    /// <summary>
    /// Copied from: https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/WebMatrix.WebData/SimpleRoleProvider.cs
    /// Apache License copyright 2.0 blah blah...
    /// to support more database engines.
    /// </summary>
    public class PhunSimpleRoleProvider : RoleProvider
    {
        /// <summary>
        /// The _previous provider
        /// </summary>
        private RoleProvider _previousProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRoleProvider"/> class.
        /// </summary>
        public PhunSimpleRoleProvider()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRoleProvider"/> class.
        /// </summary>
        /// <param name="previousProvider">The previous provider.</param>
        public PhunSimpleRoleProvider(RoleProvider previousProvider)
        {
            this._previousProvider = previousProvider;
        }

        /// <summary>
        /// Gets the previous provider.
        /// </summary>
        /// <value>
        /// The previous provider.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Initialize must be called first</exception>
        private RoleProvider PreviousProvider
        {
            get
            {
                if (this._previousProvider == null)
                {
                    throw new InvalidOperationException("Initialize must be called first");
                }
                else
                {
                    return this._previousProvider;
                }
            }
        }

        /// <summary>
        /// Gets the name of the role table.
        /// </summary>
        /// <value>
        /// The name of the role table.
        /// </value>
        public virtual string RoleTableName
        {
            get
            {
                return "SimpleRoles";
            }
        }

        /// <summary>
        /// Gets the name of the users in role table.
        /// </summary>
        /// <value>
        /// The name of the users in role table.
        /// </value>
        public virtual string UsersInRoleTableName
        {
            get
            {
                return "SimpleUsersInRoles";
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
        internal string ConnectionStringName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [initialize called].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [initialize called]; otherwise, <c>false</c>.
        /// </value>
        internal bool InitializeCalled { get; set; }

        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <returns>The name of the application to store and retrieve role information for.</returns>
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
                    return PreviousProvider.ApplicationName;
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
        /// Verifies the initialized.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Initialize must be called first</exception>
        private void VerifyInitialized()
        {
            if (!this.InitializeCalled)
            {
                throw new InvalidOperationException("Initialize must be called first");
            }
        }

        /// <summary>
        /// Connects to database.
        /// </summary>
        /// <returns></returns>
        private DapperContext ConnectToDatabase()
        {
            return new DapperContext(this.ConnectionStringName);
        }

        /// <summary>
        /// Creates the tables if needed.
        /// </summary>
        internal void CreateTablesIfNeeded()
        {
            using (var db = ConnectToDatabase())
            {
                if (!db.TableExists(this.RoleTableName))
                {
                    var sql =
                        string.Format(this.profileTableSql, this.RoleTableName)
                              .Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var sqlString in sql)
                    {
                        db.Execute(sqlString);
                    }

                    db.Execute(
                        string.Format(
                            @"create table {0}
(
    roleid                      integer not null, 
    userid                      integer not null,
    constraint pk_{0}           primary key (roleid, userid),
    constraint fk_uir_roleid    foreign key (roleid) references {1} (roleid),
    constraint fk_uir_userid    foreign key (userid) references {2} ({3})
)", 
this.UsersInRoleTableName, 
this.RoleTableName, 
this.UserTableName, 
this.UserIdColumn));
                }
            }
        }

        /// <summary>
        /// Gets the user ids from names.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="usernames">The usernames.</param>
        /// <returns></returns>
        private List<int> GetUserIdsFromNames(DapperContext db, string[] usernames)
        {
            var userIds = new List<int>(usernames.Length);
            foreach (string username in usernames)
            {
                var provider = new PhunSimpleMembershipProvider();

                int id = provider.GetUserId(
                    db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, username);
                if (id == -1)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "No user found '{0}'.", username));
                }

                userIds.Add(id);
            }

            return userIds;
        }

        /// <summary>
        /// Gets the role ids from names.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="roleNames">The role names.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        private List<int> GetRoleIdsFromNames(DapperContext db, string[] roleNames)
        {
            var roleIds = new List<int>(roleNames.Length);
            foreach (string role in roleNames)
            {
                int id = this.FindRoleId(db, role);
                if (id == -1)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "No role found '{0}'.", role));
                }

                roleIds.Add(id);
            }

            return roleIds;
        }

        /// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (!this.InitializeCalled)
            {
                this.PreviousProvider.AddUsersToRoles(usernames, roleNames);
            }
            else
            {
                using (var db = this.ConnectToDatabase())
                {
                    int userCount = usernames.Length;
                    int roleCount = roleNames.Length;
                    var userIds = this.GetUserIdsFromNames(db, usernames);
                    var roleIds = this.GetRoleIdsFromNames(db, roleNames);

                    // Generate a INSERT INTO for each userid/rowid combination, where userIds are the first params, and roleIds follow
                    for (int uId = 0; uId < userCount; uId++)
                    {
                        for (int rId = 0; rId < roleCount; rId++)
                        {
                            if (this.IsUserInRole(usernames[uId], roleNames[rId]))
                            {
                                throw new InvalidOperationException(
                                    string.Format(
                                        CultureInfo.CurrentCulture,
                                        "User '{0}' already in role '{1}'.",
                                        usernames[uId],
                                        roleNames[rId]));
                            }

                            db.ExecuteCommand(
                                string.Format(
                                    "INSERT INTO {0} (UserId, RoleId) VALUES (@UserId, @RoleId)",
                                    this.UsersInRoleTableName),
                                new { UserId = userIds[uId], RoleId = roleIds[rId] });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override void CreateRole(string roleName)
        {
            if (!this.InitializeCalled)
            {
                this.PreviousProvider.CreateRole(roleName);
            }
            else
            {
                using (var db = ConnectToDatabase())
                {
                    int roleId = FindRoleId(db, roleName);
                    if (roleId != -1)
                    {
                        throw new InvalidOperationException(
                            String.Format(
                                CultureInfo.InvariantCulture, "Role '{0}' does not exists.", roleName));
                    }

                    db.ExecuteCommand(
                        string.Format("INSERT INTO {0} (RoleName) VALUES (@RoleName)", this.RoleTableName),
                        new { RoleName = roleName });
                }
            }
        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">If true, throw an exception if <paramref name="roleName" /> has one or more members and do not delete <paramref name="roleName" />.</param>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.DeleteRole(roleName, throwOnPopulatedRole);
            }

            using (var db = this.ConnectToDatabase())
            {
                int roleId = this.FindRoleId(db, roleName);
                if (roleId == -1)
                {
                    return false;
                }

                if (throwOnPopulatedRole)
                {
                    if (this.GetUsersInRole(roleName).Any())
                    {
                        throw new InvalidOperationException(
                            String.Format(
                                CultureInfo.InvariantCulture, "Role '{0}' is populated with users.", roleName));
                    }
                }
                else
                {
                    db.ExecuteCommand(
                        string.Format("DELETE FROM {0} WHERE RoleId = @RoleId", this.UsersInRoleTableName),
                        new { RoleId = roleId });
                }

                db.ExecuteCommand(
                        string.Format("DELETE FROM {0} WHERE RoleId = @RoleId", this.RoleTableName),
                        new { RoleId = roleId });
                return true;
            }
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches <paramref name="usernameToMatch" /> and the user is a member of the specified role.
        /// </returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.FindUsersInRole(roleName, usernameToMatch);
            }

            return this.GetUsersInRole(roleName)
                        .Where(u => string.Compare(usernameToMatch, u, StringComparison.OrdinalIgnoreCase) == 0)
                        .ToArray();
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.GetAllRoles();
            }

            using (var db = this.ConnectToDatabase())
            {
                return db.Query<string>(string.Format(@"SELECT RoleName FROM {0}", this.RoleTableName), null).ToArray();
            }
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.GetRolesForUser(username);
            }

            using (var db = this.ConnectToDatabase())
            {
                int userId = (new PhunSimpleMembershipProvider()).GetUserId(db, this.UserTableName, this.UserNameColumn, this.UserIdColumn, username);
                if (userId == -1)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "User '{0}' not found.", username));
                }

                string query = string.Format("SELECT RoleName FROM {0} INNER JOIN {1} ON {0}.UserId = {1}.{2} Where {0}.UserId = @UserId GROUP BY RoleName", this.UsersInRoleTableName, this.UserTableName, this.UserIdColumn);
                return db.Query<string>(query, new { UserId = userId }).ToArray();
            }
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.GetUsersInRole(roleName);
            }

            using (var db = this.ConnectToDatabase())
            {
                var roleId = this.FindRoleId(db, roleName);

                return
                    db.Query<string>(
                        string.Format(
                            "SELECT {2}.{0} FROM {1} INNER JOIN {2} ON {1}.UserId = {2}.{3} WHERE {1}.RoleId = @RoleId",
                            this.UserNameColumn,
                            this.UsersInRoleTableName,
                            this.UserTableName,
                            this.UserIdColumn),
                        new { RoleId = roleId }).ToArray();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.IsUserInRole(username, roleName);
            }

            return this.GetRolesForUser(username).Any(s => string.Compare(roleName, s, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (!this.InitializeCalled)
            {
                this.PreviousProvider.RemoveUsersFromRoles(usernames, roleNames);
            }
            else
            {
                foreach (string rolename in roleNames)
                {
                    if (!this.RoleExists(rolename))
                    {
                        throw new InvalidOperationException(
                            string.Format(CultureInfo.CurrentCulture, "No role '{0}' found.", rolename));
                    }
                }

                foreach (string username in usernames)
                {
                    foreach (string rolename in roleNames)
                    {
                        if (!IsUserInRole(username, rolename))
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    "User '{0}' not in role '{1}'.",
                                    username,
                                    rolename));
                        }
                    }
                }

                using (var db = this.ConnectToDatabase())
                {
                    var userIds = this.GetUserIdsFromNames(db, usernames);
                    var roleIds = this.GetRoleIdsFromNames(db, roleNames);

                    foreach (int userId in userIds)
                    {
                        foreach (int roleId in roleIds)
                        {
                            db.ExecuteCommand(string.Format("DELETE FROM {0} WHERE UserId = @UserId and RoleId = @RoleId", this.UsersInRoleTableName), new { UserId = userId, RoleId = roleId });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the role id.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        private int FindRoleId(DapperContext db, string roleName)
        {
            var result = db.Query<int>(string.Format("SELECT RoleId FROM {0} WHERE RoleName = @RoleName", this.RoleTableName), new {RoleName = roleName}).FirstOrDefault();
            return result > 0 ? result : -1;
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            if (!this.InitializeCalled)
            {
                return this.PreviousProvider.RoleExists(roleName);
            }

            using (var db = this.ConnectToDatabase())
            {
                return (this.FindRoleId(db, roleName) != -1);
            }
        }

        /// <summary>
        /// The profile table SQL
        /// </summary>
        private readonly string profileTableSql = @"
create table {0}
(
    roleid      integer primary key not null, 
    rolename    varchar(256) unique not null 
)

GO

create sequence seq_{0} increment by 1 start with 1 nomaxvalue nocycle;

GO

create or replace trigger trg_{0}
before insert on {0}
for each row
begin
  if :new.roleid is null then
    select seq_{0}.nextval into :new.roleid from dual;
  end if;
end;
";
    }
}
