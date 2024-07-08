using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Configuration;

namespace CallAugger.Utilities.Sqlite
{
    public class UserRepository
    { 
        // Instance property
        private static readonly object lockObject = new object();
        private static UserRepository instance;

        public static UserRepository Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new UserRepository();
                    }
                    return instance;
                }
            }
        }

        // Cache
        public List<User> Users = new List<User>();



        private UserRepository()
        {
            All();
        }


        ///////////// Fetching /////////////
        public List<User> All()
        {
            List<User> users = new List<User>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(QueryStore.SelectAllUsers, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    User user = CreateFromReader(reader);

                    user.AddCalls(CallRecordRepository.Instance.ByUser(connection, user.id));
                    users.Add(user);
                }
            }

            Users = users.OrderByDescending(usr => usr.TotalDuration).ToList();
            return users;
        }

        private User CreateFromReader(SQLiteDataReader reader)
        {
            User user = new User
            {
                id = Convert.ToInt32(reader["id"]),
                Name = reader["Name"].ToString(),
                Extention = reader["Extention"].ToString()
            };

            return user;
        }

        public User Ext(SQLiteConnection connection, string userExtention)
        {
            User user = new User();
            connection.Open();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllUsers + " WHERE Extention = @Extention", connection);

            command.Parameters.AddWithValue("@Extention", userExtention);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                user = CreateFromReader(reader);

                user.AddCalls(CallRecordRepository.Instance.ByUser(connection, user.id));
            }

            return user;
        }

        public User Find(SQLiteConnection connection, string name)
        {
            User user = new User();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllUsers + " WHERE Name = @Name", connection);

            command.Parameters.AddWithValue("@Name", name);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                user = CreateFromReader(reader);

                user.AddCalls(CallRecordRepository.Instance.ByUser(connection, user.id));
            }

            return user;
        }


        ///////////// CRUD /////////////
        public User Insert(SQLiteConnection connection, User user)
        {
            bool exists = this.Contains(user);
            if (!this.Contains(user))
            {
                SQLiteCommand command = new SQLiteCommand(
                    QueryStore.InsertUser, connection);

                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Extention", user.Extention);

                command.ExecuteNonQuery();

                command.CommandText = "SELECT last_insert_rowid()";
                object id = command.ExecuteScalar();
                user.id = Convert.ToInt32(id);

                // Update Cache
                Users.Add(user);
                return user;
            }
            else if (exists)
            {
                return Users.Find(usr => usr.Name == user.Name);
            }
            else
            {
                throw new Exception($"ATTEMPTING TO INSERT User FAILED : {user.InlineDetails()}");
            }
        }

        public void Delete(SQLiteConnection connection, User user)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.DeleteUser, connection))
            {
                command.Parameters.AddWithValue("@id", user.id);

                int rowsDeleted = command.ExecuteNonQuery();

                if (rowsDeleted == 0)
                {
                    throw new Exception($"ATTEMPTING TO DELETE User FAILED : {user.InlineDetails()}");
                }

                // Update Cache
                Users.Remove(user);

            }
        }

        public User Update(SQLiteConnection connection, User user)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdateUser, connection))
            {
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Extention", user.Extention);
                command.Parameters.AddWithValue("@id", user.id);

                int rowsUpdated = command.ExecuteNonQuery();

                if (rowsUpdated == 0)
                {
                    throw new Exception($"ATTEMPTING TO UPDATE User FAILED : {user.InlineDetails()}");
                }

                // Update Cache
                Users.Remove(user);
                Users.Add(user);
                return user;
            }
        }


        ///////////// Helpers /////////////
        public bool Contains(User newUser)
        {
            if (Users.Any(usr => usr.Name == newUser.Name &&
                usr.Extention == newUser.Extention))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void TimeToAccess()
        {
            var allStartTime = DateTime.Now;
            var all = All();
            var allEndTime = DateTime.Now;
            var allTimeSpan = allEndTime - allStartTime;

            var message = $"Users:        {Math.Round(allTimeSpan.TotalSeconds, 2)}s for {all.Count()} records\n";
            Console.WriteLine(Logger.Database(message));
        }
    }
}
