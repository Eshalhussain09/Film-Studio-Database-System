using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace FilmStudioWinForms
{
    public static class DatabaseHelper
    {
        private const string Host     = "localhost";
        private const string Username = "system";
        private const string Password = "system";

        private static string BuildConnectionString(string port, string sid) =>
            $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Host})(PORT={port}))" +
            $"(CONNECT_DATA=(SID={sid})));User Id={Username};Password={Password};" +
            $"Connection Timeout=15;Pooling=true;Min Pool Size=1;Max Pool Size=10;";

        public static string ActiveConnectionString { get; private set; } = BuildConnectionString("1522", "xe");
        public static string ActiveSid  { get; private set; } = "xe";
        public static string ActivePort { get; private set; } = "1522";

        // ── Session Info ─────────────────────────────────────────────────
        public static int    CurrentUserId   { get; private set; }
        public static int    CurrentPartyId  { get; private set; }
        public static string CurrentRole     { get; private set; } = "";
        public static string CurrentFullName { get; private set; } = "";
        public static bool   ShouldLogout    { get; set; } = false;

        public static void SetSession(int userId, int partyId, string role, string fullName)
        {
            CurrentUserId   = userId;
            CurrentPartyId  = partyId;
            CurrentRole     = role;
            CurrentFullName = fullName;
        }

        public static OracleConnection GetConnection() => new OracleConnection(ActiveConnectionString);

        // ── Auto-detect connection ────────────────────────────────────────
        public static bool TestConnection(out string message)
        {
            var candidates = new (string port, string sid)[] {
                ("1522","xe"),("1522","XE"),("1521","xe"),("1521","XE"),("1522","orcl"),("1521","orcl"),
            };
            foreach (var (port, sid) in candidates)
            {
                string cs = BuildConnectionString(port, sid);
                try
                {
                    using var conn = new OracleConnection(cs); conn.Open();
                    using var cmd = new OracleCommand("SELECT 1 FROM DUAL", conn); cmd.ExecuteScalar();
                    ActiveConnectionString = cs; ActivePort = port; ActiveSid = sid;
                    message = $"Oracle {conn.ServerVersion}  |  Port:{port}  |  SID:{sid}  |  User:{Username}";
                    return true;
                }
                catch { }
            }
            message = "Could not connect to Oracle 11g.\nTried ports 1521,1522 / SIDs xe,XE,orcl";
            return false;
        }

        // ── Authenticate login ────────────────────────────────────────────
       // ── Authenticate login ────────────────────────────────────────────
        public static (string? role, int partyId, string fullName, int userId) ValidateLogin(string username, string password)
        {
            try
            {
                using var conn = GetConnection(); 
                conn.Open();
                
                string sql = "SELECT role, party_id, full_name, user_id FROM app_user WHERE LOWER(username) = LOWER(:u) AND password = :p";
                using var cmd = new OracleCommand(sql, conn);
                cmd.BindByName = true; 
                
                // Explicitly defining the Oracle types ensures no formatting mismatches
                cmd.Parameters.Add(new OracleParameter("u", OracleDbType.Varchar2, username, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("p", OracleDbType.Varchar2, password, ParameterDirection.Input));
                
                using var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    return (
                        rdr["role"].ToString(), 
                        Convert.ToInt32(rdr["party_id"]), 
                        rdr["full_name"].ToString()!, 
                        Convert.ToInt32(rdr["user_id"])
                    );
                }
            }
            catch (Exception ex)
            {
                // THIS POP-UP WILL REVEAL THE TRUE ERROR
                System.Windows.Forms.MessageBox.Show(
                    $"Oracle Error during login:\n\n{ex.Message}", 
                    "Database Diagnostic", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return (null, 0, "", 0);
        }

        // ── Query Helpers ─────────────────────────────────────────────────
        public static object? ExecuteScalar(string sql, OracleParameter[]? p = null)
        {
            using var conn = GetConnection(); conn.Open();
            using var cmd  = new OracleCommand(sql, conn);
            cmd.BindByName = true;
            if (p != null) cmd.Parameters.AddRange(p);
            return cmd.ExecuteScalar();
        }

        public static int ExecuteNonQuery(string sql, OracleParameter[]? p = null)
        {
            using var conn = GetConnection(); conn.Open();
            using var cmd  = new OracleCommand(sql, conn);
            cmd.BindByName = true;
            if (p != null) cmd.Parameters.AddRange(p);
            return cmd.ExecuteNonQuery();
        }
    }
}