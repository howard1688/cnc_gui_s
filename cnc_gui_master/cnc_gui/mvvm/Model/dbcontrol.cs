using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using HandyControl.Controls;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using MySqlX.XDevAPI.Common;
using static cnc_gui.homeViewModel;
namespace cnc_gui
{
    public class DatabaseModel
    {
        public class PlcControl
        {
            public int AddressId { get; set; }
            public string AddressType { get; set; }
            public int Handl { get; set; }
            public int Address { get; set; }
            public int Decimal { get; set; }
        }
        public class FlusherTimeSetting
        {
            public int Total_flusher_time { get; set; }
        }
        public class Level_Set
        {
            public int LevelId { get; set; }
            public string LevelType { get; set; }
            public int Level1 { get; set; }
            public int Level2 { get; set; }
            public int Level3 { get; set; }
            public int Level4 { get; set; }
        }
        public class Ip_port
        {
            public string Cncip { get; set; }
            public string Cncport { get; set; }
        }
        public class Level_result
        {
            public int Flusher_level_result { get; set; }
            public int Excluder_level_result { get; set; }
        }
        public class setting
        {
            public int Excluder_Period { get; set; }
        }
        public class Level_set_to_string
        {
            public int Level_id_str { get; set; }
            public string Level_Type_str { get; set; }
            public string Level_1_to_str { get; set; }
            public string Level_2_to_str { get; set; }
            public string Level_3_to_str { get; set; }
            public string Level_4_to_str { get; set; }
            public string Level_5_to_str { get; set; }
        }
        public class Level_set_time
        {
            public int Level_id { get; set; }
            public string level_type { get; set; }
            public int Level_1_time { get; set; }
            public int Level_2_time { get; set; }
            public int Level_3_time { get; set; }
            public int Level_4_time { get; set; }
            public int Level_5_time { get; set; }
        }
        public class flusher_excluder_onoff
        {
            public int flusher_state { get; set; }
            public int excluder_state { get; set; }
        }


        private string connectionString = "server=localhost;database=cnc_db;user=root;password=ncut2024;";
        public Level_Set GetLevelSetById(int levelId)
        {
            Level_Set levelSet = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Level_id, Level_type, Level_1, Level_2, Level_3, Level_4 FROM Level_set WHERE Level_id = @LevelId;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LevelId", levelId);
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        levelSet = new Level_Set
                        {
                            LevelId = Convert.ToInt32(reader["Level_id"]),
                            LevelType = reader["Level_type"].ToString(),
                            Level1 = Convert.ToInt32(reader["Level_1"]),
                            Level2 = Convert.ToInt32(reader["Level_2"]),
                            Level3 = Convert.ToInt32(reader["Level_3"]),
                            Level4 = Convert.ToInt32(reader["Level_4"])
                        };
                    }
                }
            }

            return levelSet;
        }

        public object GetColumnValueById(string tableName, string columnName, int id)
        {
            object result = null;
            string query = $"SELECT {columnName} FROM {tableName} WHERE id = @Id";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = reader[columnName];
                        }
                    }
                }
            }

            return result;
        }

        public void UpdateDataById(string tableName, int id, TimeSpan newTime, double newMagnitude, double newThreshold)
        {
            string query = $"UPDATE {tableName} SET 時間 = @Time, 量級 = @Magnitude, 累積門檻值 = @Threshold WHERE id = @Id";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Time", newTime);
                    cmd.Parameters.AddWithValue("@Magnitude", newMagnitude);
                    cmd.Parameters.AddWithValue("@Threshold", newThreshold);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /*public ObservableCollection<his_data> GetLatest20Data(string tableName)
        {
            ObservableCollection<his_data> dataList = new ObservableCollection<his_data>();

            string query = $"SELECT id, open_timestamp, result, threshold FROM {tableName} ORDER BY open_timestamp DESC LIMIT 20";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            his_data dataItem = new his_data
                            {
                                his_id_value = Convert.ToInt32(reader["id"]),
                                his_timestamp_value = (TimeSpan)reader["open_timestamp"],
                                his_result_value = (int)(reader["result"]),
                                his_threshold_value = (int)(reader["threshold"])
                            };
                            dataList.Add(dataItem);
                        }
                    }
                }
            }

            return dataList;
        }*/
        //寫入量級區間
        public void UpdateLevelSetColumn(int id, string columnName, int newValue)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 確保欄位名稱是合法的，防止 SQL Injection
                var validColumns = new[] { "Level_1", "Level_2", "Level_3", "Level_4" };
                if (!validColumns.Contains(columnName))
                {
                    throw new ArgumentException("指定的欄位名稱無效。");
                }

                // 更新指定欄位的數值
                string updateQuery = $@"
                    UPDATE Level_set 
                    SET {columnName} = @NewValue 
                    WHERE id = @Id;";
                using (var updateCmd = new MySqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@NewValue", newValue);
                    updateCmd.Parameters.AddWithValue("@Id", id);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }
        public int GetLatestId(string tableName)
        {
            int latestId = -1;
            string query = $"SELECT MAX(id) FROM {tableName}";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        latestId = Convert.ToInt32(result);
                    }
                }
            }

            return latestId;
        }
        /*
        public void UpdateLevelSet(int levelId, string columnName, object newValue)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $"UPDATE Level_set SET {columnName} = @newValue WHERE Level_id = @levelId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@newValue", newValue);
                    cmd.Parameters.AddWithValue("@LevelId", levelId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        */
        /*
        public PlcControl GetPlcControlById(int addressId)
        {
            PlcControl plcControl = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, Address_type, Handl, Address, `Decimal_num` FROM Plc_control WHERE id = @AddressId;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        plcControl = new PlcControl
                        {
                            AddressId = Convert.ToInt32(reader["id"]), 
                            AddressType = reader["Address_type"].ToString(),
                            Handl = Convert.ToInt32(reader["Handl"]),
                            Address = Convert.ToInt32(reader["Address"]),
                            Decimal = Convert.ToInt32(reader["Decimal_num"])
                        };
                    }
                }
            }

            return plcControl;
        }
        */

        //依序輸入要修改哪一筆資料，哪一欄以及要寫進的參數
        public void UpdatePlcControl(int id, string columnName, object newValue)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 確保欄位名稱是合法的，防止 SQL Injection
                var validColumns = new[] { "Handl", "Address", "Decimal_num"};
                if (!validColumns.Contains(columnName))
                {
                    throw new ArgumentException("指定的欄位名稱無效。");
                }

                // 更新指定欄位的數值
                string updateQuery = $@"
                    UPDATE Plc_control
                    SET {columnName} = @NewValue 
                    WHERE id = @Id;";
                using (var updateCmd = new MySqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@NewValue", newValue);
                    updateCmd.Parameters.AddWithValue("@Id", id);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }
        //底沖r/w
        public FlusherTimeSetting GetFlusherTimeSetting()
        {
            FlusherTimeSetting flusherTimeSetting = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Total_flusher_time FROM Flusher_time_setting;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        flusherTimeSetting = new FlusherTimeSetting
                        {
                            Total_flusher_time = Convert.ToInt32(reader["Total_flusher_time"]),
                        };
                    }
                }
            }

            return flusherTimeSetting;
        }
        public void Updatelevelhistory(TimeSpan Open_timestamp,int result,int threshold,float flusher_energy_data, float excluder_energy_data, float total_energy_data,float total_energy_sum, float total_kwh, float total_kwh_sum)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string insertQuery = "insert into flusher_history(open_timestamp, result, threshold, flusher_energy_data, excluder_energy_data, total_energy_data,total_energy_sum, total_kwh,total_kwh_sum) values(@open_timestamp, @result, @threshold,@flusher_energy_data,@excluder_energy_data,@total_energy_data,@total_energy_sum,@total_kwh,@total_kwh_sum)";
                using (var insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@open_timestamp", Open_timestamp);
                    insertCmd.Parameters.AddWithValue("@result", result);
                    insertCmd.Parameters.AddWithValue("@threshold", threshold);
                    insertCmd.Parameters.AddWithValue("@flusher_energy_data", flusher_energy_data);
                    insertCmd.Parameters.AddWithValue("@excluder_energy_data", excluder_energy_data);
                    insertCmd.Parameters.AddWithValue("@total_energy_data", total_energy_data);
                    insertCmd.Parameters.AddWithValue("@total_energy_sum", total_energy_sum);
                    insertCmd.Parameters.AddWithValue("@total_kwh", total_kwh);
                    insertCmd.Parameters.AddWithValue("@total_kwh_sum", total_kwh_sum);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
        public void Initializeflusherhistory()
        {
            
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "TRUNCATE TABLE flusher_history";
                using (var deleteCmd = new MySqlCommand(deleteQuery, connection))
                {
                    deleteCmd.ExecuteNonQuery();
                }
                /*
                string insertQuery = "insert into flusher_history(open_timestamp, result, threshold, flusher_energy_data, excluder_energy_data, total_energy_data,total_energy_sum, total_kwh,total_kwh_sum) values(@open_timestamp, @result, @threshold,@flusher_energy_data,@excluder_energy_data,@total_energy_data,@total_energy_sum,@total_kwh,@total_kwh_sum)";
                using (var insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    DateTime currentTime = DateTime.Now;
                    string timeString = currentTime.ToString("HH:mm:ss");
                    TimeSpan timeValue = TimeSpan.Parse(timeString);
                    insertCmd.Parameters.AddWithValue("@open_timestamp", timeValue);
                    insertCmd.Parameters.AddWithValue("@result", 1);
                    insertCmd.Parameters.AddWithValue("@threshold", 0);
                    insertCmd.Parameters.AddWithValue("@flusher_energy_data", 0);
                    insertCmd.Parameters.AddWithValue("excluder_energy_data", 0);               
                    insertCmd.Parameters.AddWithValue("total_energy_data", 0);
                    insertCmd.Parameters.AddWithValue("@total_energy_sum", 0);
                    insertCmd.Parameters.AddWithValue("total_kwh", 0);
                    insertCmd.Parameters.AddWithValue("total_kwh_sum", 0);
                    insertCmd.ExecuteNonQuery();
                }*/
            }
           

        }
        public void UpdateFlusherTimeSetting(int newTotalFlusherTime)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 刪除舊資料
                string deleteQuery = "DELETE FROM Flusher_time_setting;";
                using (var deleteCmd = new MySqlCommand(deleteQuery, connection))
                {
                    deleteCmd.ExecuteNonQuery();
                }

                // 插入新資料
                string insertQuery = "INSERT INTO Flusher_time_setting (Total_flusher_time) VALUES (@TotalFlusherTime);";
                using (var insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@TotalFlusherTime", newTotalFlusherTime);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
        //ip port r/w
        public Ip_port GetIp_Port()
        {
            Ip_port ip_port = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Cncip, Cncport FROM Ip_port;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        ip_port = new Ip_port
                        {
                            Cncip = reader["Cncip"].ToString(),
                            Cncport = reader["Cncport"].ToString()
                        };
                    }
                }
            }

            return ip_port;
        }
        public object ExecuteScalar(string sql)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    return cmd.ExecuteScalar();
                }
            }
        }

        public int GetMaxId(string tableName)
        {
            string sql = $"SELECT MAX(id) FROM {tableName}";
            object result = ExecuteScalar(sql); // 用你的資料庫方法
            return result != null && int.TryParse(result.ToString(), out int id) ? id : -1;
        }

        public void UpdateIpPort(string newCncip, string newCncport)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 刪除舊資料
                string deleteQuery = "DELETE FROM Ip_port;";
                using (var deleteCmd = new MySqlCommand(deleteQuery, connection))
                {
                    deleteCmd.ExecuteNonQuery();
                }

                // 插入新資料
                string insertQuery = "INSERT INTO Ip_port (Cncip, Cncport) VALUES (@Cncip, @Cncport);";
                using (var insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@Cncip", newCncip);
                    insertCmd.Parameters.AddWithValue("@Cncport", newCncport);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
        //量級r/w
        public Level_result GetLevelResult()
        {
            Level_result level_result = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Flusher_level_result, Excluder_level_result FROM Level_result;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        level_result = new Level_result
                        {
                            Flusher_level_result = Convert.ToInt32(reader["Flusher_level_result"]),
                            Excluder_level_result = Convert.ToInt32(reader["Excluder_level_result"])
                        };
                    }
                }
            }

            return level_result;
        }
        public void UpdateLevelResult(string columnName, object newValue)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $"UPDATE Level_result SET {columnName} = @newValue";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@newValue", newValue);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        //排屑機閾值r/w
        public setting GetSetting()
        {
            setting setting = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Excluder_Period FROM setting;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        setting = new setting
                        {
                            Excluder_Period = Convert.ToInt32(reader["Excluder_Period"])
                        };
                    }
                }
            }

            return setting;
;
        }
        public void UpdateSetting(int newExcluderPeriod)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Setting;";
                using (var deleteCmd = new MySqlCommand(deleteQuery, connection))
                {
                    deleteCmd.ExecuteNonQuery();
                }

                string insertQuery = "INSERT INTO Setting (Excluder_Period) VALUES (@ExcluderPeriod);";
                using (var insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@ExcluderPeriod", newExcluderPeriod);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
        //抵充排屑機開關狀態
        public flusher_excluder_onoff Getflusher_excluder_onoff()
        {
            flusher_excluder_onoff flusher_excluder_onoff = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT flusher_state, excluder_state FROM flusher_excluder_onoff;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        flusher_excluder_onoff = new flusher_excluder_onoff
                        {
                            flusher_state = Convert.ToInt32(reader["flusher_state"]),
                            excluder_state = Convert.ToInt32(reader["excluder_state"])
                        };
                    }
                }
            }

            return flusher_excluder_onoff;
        }
        public void Updateflusher_excluder_onoff(string columnName, object newValue)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $"UPDATE flusher_excluder_onoff SET {columnName} = @newValue";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@newValue", newValue);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        ///
        public void UpdateTimeByIdAndColumn(int id, string columnName, int newValue)
        {
            string query = $"UPDATE Level_set_time SET {columnName} = @Value WHERE id = @Id";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Value", newValue);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateLevelSetToString(int id, string columnName, string levelDescription)
        {
            string query = $"UPDATE Level_set_to_string SET {columnName} = @LevelDescription WHERE id = @Id";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LevelDescription", levelDescription);
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public int GetLastThresholdZeroIdBefore(int currentId)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT id FROM flusher_history WHERE threshold = 0 AND id < @currentId ORDER BY id DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@currentId", currentId);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0; // 若找不到就回傳 0 或其他適合的預設值
            }
        }


    }
}
