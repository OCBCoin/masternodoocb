using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MasterNodoOCB
{
    public class matrixServices
    {
        public static IEnumerable<matrix> GetAll()
        {
            var result = new List<matrix>();

            using (var ctx = DbContext.GetInstance())
            {
                var query = "SELECT * FROM matrix";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new matrix
                            {
                                id = Convert.ToInt32(reader["id"].ToString()),
                                coin = reader["coin"].ToString(),
                                matrixg = reader["matrix"].ToString(),
                                price = reader["price"].ToString(),
                                stock = reader["stock"].ToString(),
                                stop = reader["stop"].ToString(),
                                difficulty = reader["difficulty"].ToString(),
                                datestart = Convert.ToDateTime(reader["datestart"]),
                                lastblock = Convert.ToDateTime(reader["lastblock"]),
                                block = Convert.ToInt32(reader["block"].ToString()),
                                reward = reader["reward"].ToString(),
                                nodos = Convert.ToInt32(reader["nodos"].ToString()),
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
