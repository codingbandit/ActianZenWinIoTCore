using Pervasive.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TempPressure
{
    public class ActianZenDataSource
    {

        private string _tableName = "UWP_SensorReadings";
        private PsqlConnection _conn = new PsqlConnection("Host=localhost;Port=1583;ServerDSN=DEMODATA;");
        private PsqlCommand _cmd = new PsqlCommand();

        private void _createTable()
        {
            if (!_tableExists())
            {
                var query = $@"CREATE TABLE {_tableName}(DeviceName varchar(50), Temperature double, Pressure double, Altitude double, ReadingTs datetime)";

                int recordsAffected = _executeNonQuery(query);
                Debug.WriteLine((recordsAffected == -1) ? "Table: '" + _tableName + "' Successfully Created !!\n" : string.Empty);

            }

        }

        public void DropTable()
        {
            if (_tableExists())
            {
                var query = $@"DROP TABLE {_tableName}";
                _executeNonQuery(query);
                if (_tableExists())
                {
                    Debug.WriteLine("Table not dropped\n");
                }
                else
                {
                    Debug.WriteLine("Table " + _tableName + " Successfully Dropped !!\n");
                }
            }
        }

        public void AddReading(Reading reading)
        {
            //Create Reading Record in database
            // **NOTE: SQL String is being generated for debug output reasons only, 
            //          IN PRODUCTION UTILIZE named parameters using the PsqlParameter object
            int recordsAffected = 0;
            var query = $@"INSERT INTO {_tableName} VALUES ( '{ reading.DeviceName }',{ reading.Temperature }, { reading.Pressure }, {reading.Altitude}, '{ reading.ReadingTs.ToString("yyyy-MM-dd HH:mm:ss") }'  )";
            if (!_tableExists())
            {
                this._createTable();
            }
            
            recordsAffected = _executeNonQuery(query);
            Debug.WriteLine("\nRecords Affected: " + recordsAffected + "\n\n");
        }

        public List<Reading> GetReadings()
        {
            var query = $"SELECT * FROM {_tableName}";
            PsqlDataReader rdr = null;
            string logText = string.Empty;
            List<Reading> retValue = new List<Reading>();

            if (_tableExists())
            {
                try
                {
                    if ((_conn.State != System.Data.ConnectionState.Open))
                    {
                        _conn.Open();
                    }
                    _cmd.Connection = _conn;
                    _cmd.CommandText = query;

                    logText = "Query Executed : " + query + "\n\n";
                    rdr = _cmd.ExecuteReader();
                    int rowCount = 0;

                    while (rdr.Read())
                    {
                        Reading rdg = new Reading();

                        rdg.DeviceName = rdr.GetString(0);
                        rdg.Temperature = rdr.GetDouble(1);
                        rdg.Pressure = rdr.GetDouble(2);
                        rdg.Altitude = rdr.GetDouble(3);
                        rdg.ReadingTs = rdr.GetDateTime(4);

                        retValue.Add(rdg);
                        rowCount++;
                    }
                    logText += "Total Rows :" + rowCount;
                }
                catch (Exception ex)
                {
                    logText = logText + "\nQuery execution failed with exception: " + ex.Message;
                }
                finally
                {
                    _conn.Close();
                }
                Debug.WriteLine(logText);
            }
            else
            {
                Debug.WriteLine("Table does not exist.");
            }
          
            return retValue;
        }


        private int _executeNonQuery(string query)
        {
            int rowsAffected = 0;
            var logText = string.Empty;
            try
            {
                if ((_conn.State != System.Data.ConnectionState.Open))
                {
                    _conn.Open();
                    Debug.WriteLine("Connection Opened: ");
                }
                _cmd.Connection = _conn;
                _cmd.CommandText = query;

                logText = "Query Executed : " + query + "\n\n";
                rowsAffected = _cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                logText = logText + "Query execution failed with exception: " + ex.Message;
            }
            finally
            {
                _conn.Close();
            }
            Debug.WriteLine(logText);
            return rowsAffected;
        }

        private bool _tableExists()
        {
            bool result = false;
            int count = 0;
            try
            {
                if ((_conn.State != System.Data.ConnectionState.Open))
                {
                    _conn.Open();
                    Debug.WriteLine("Connection Opened: ");
                }
                _cmd.Connection = _conn;
                _cmd.CommandText = $"select count(*) from X$File where Xf$Name = '{_tableName}'";

                count = (int)_cmd.ExecuteScalar();
                result = (count >= 1);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("**** Exception : " + ex.Message + " ****");
            }
            finally
            {
                _conn.Close();
            }
            return result;
        }
    }
}
