using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MasterNodoOCB
{
    public class blockServices
    {
        public static IEnumerable<block> GetAll()
        {
            var result = new List<block>();

            using (var ctx = DbContext.GetInstance())
            {
                var query = "SELECT * FROM block";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new block
                            {
                                height = Convert.ToInt32(reader["height"].ToString()),
                                dateblock = Convert.ToDateTime(reader["dateblock"]),
                                hash = reader["hash"].ToString(),
                                sizeblock = reader["sizeblock"].ToString(),
                                shared = reader["shared"].ToString(),
                                resolvedby = reader["resolvedby"].ToString(),
                                difficulty = reader["difficulty"].ToString(),
                                rewardblock = reader["rewardblock"].ToString(),
                                status = Convert.ToInt32(reader["status"].ToString()),
                                raizmerkle = reader["raizmerkle"].ToString(),                                
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
