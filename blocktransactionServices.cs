using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MasterNodoOCB
{
    class blocktransactionServices
    {
        public static IEnumerable<blocktransaction> GetAll()
        {
            var result = new List<blocktransaction>();

            using (var ctx = DbContext.GetInstance())
            {
                var query = "SELECT * FROM blocktransaction";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new blocktransaction
                            {
                                track = Convert.ToInt32(reader["track"].ToString()),
                                height = Convert.ToInt32(reader["height"].ToString()),
                                tracking = reader["tracking"].ToString(),
                                addressinput = reader["addressinput"].ToString(),
                                addressoutput = reader["addressoutput"].ToString(),
                                type = reader["type"].ToString(),
                                typeo = reader["typeo"].ToString(),
                                status = reader["status"].ToString(),
                                amountoutput = reader["amountoutput"].ToString(),
                                amountinput = reader["amountinput"].ToString(),
                                commission = reader["commission"].ToString(),
                                datetimetrack = Convert.ToDateTime(reader["datetimetrack"]),
                                confirmation = Convert.ToInt32(reader["confirmation"].ToString()),                                
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
