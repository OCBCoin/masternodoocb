using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MasterNodoOCB
{
    public class UserService
    {
        public static IEnumerable<User> GetAll()
        {
            var result = new List<User>();

            using (var ctx = DbContext.GetInstance())
            {
                var query = "SELECT * FROM Users";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new User
                            {
                                Id = Convert.ToInt32(reader["id"].ToString()),
                                Label = reader["Label"].ToString(),
                                LLave = reader["LLave"].ToString(),
                                LLavepriv = reader["LLavepriv"].ToString(),
                                Monedas = reader["Monedas"].ToString(),
                                LLavedestino = reader["LLavedestino"].ToString(),
                                Birthday = Convert.ToDateTime(reader["Birthday"]),
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
