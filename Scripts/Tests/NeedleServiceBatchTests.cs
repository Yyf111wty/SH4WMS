// Standalone contract tests: compile this file with the production NeedleService.cs and Newtonsoft.Json.
// SqlSugar is replaced only at the database boundary. No database connection or credentials are used.
// These tests verify business guards, batch payloads and call counts, not MySQL execution/locking.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using SqlSugar;
using TRXWMS.BILL.WCS;

internal static class NeedleServiceBatchTests
{
    private static int passed;

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Run(string name, Action test)
    {
        test();
        passed++;
        Console.WriteLine("PASS " + name);
    }

    private static JArray Needles(int count)
    {
        JArray result = new JArray();
        for (int i = 0; i < count; i++) result.Add(new JObject { ["No"] = "N-" + i, ["Pos"] = i + 1 });
        return result;
    }

    private static int Bind(SqlSugarClient db, JArray needles)
    {
        return NeedleService.BindNeedles(db, "W-BOX", "NEW-ITEM", null, needles, "tester");
    }

    private static void Old(SqlSugarClient db, int index, string status, string box = "W-BOX", int enabled = 1)
    {
        db.Ado.Old.Rows.Add(index, "ID-" + index, status, "OLD-ITEM-" + index, box, 50 - index, enabled, "ORDER-" + index);
    }

    private static void Reject(SqlSugarClient db, JArray needles, string expected)
    {
        bool rejected = false;
        try { Bind(db, needles); }
        catch (Exception ex)
        {
            Check(ex.Message.Contains(expected), "Unexpected error: " + ex.Message);
            rejected = true;
        }
        Check(rejected, "Expected rejection: " + expected);
        Check(!db.Ado.Calls.Any(c => c.Kind == "write"), "Guard failure must not write inventory/history");
    }

    private static Call History(SqlSugarClient db)
    {
        return db.Ado.Calls.Single(c => c.Kind == "write" && c.Sql.Contains("wms_needle_transaction") && c.Sql.Contains("VALUES"));
    }

    private static void Main()
    {
        Run("null/empty list performs no SQL", () => {
            SqlSugarClient db = new SqlSugarClient();
            Check(Bind(db, null) == 0 && Bind(db, new JArray()) == 0, "Empty return value");
            Check(db.Ado.Calls.Count == 0, "Empty SQL count");
        });

        Run("50 new needles use four database calls and retain ID links", () => {
            SqlSugarClient db = new SqlSugarClient();
            Check(Bind(db, Needles(50)) == 50, "Bound count");
            Check(db.Ado.Calls.Count == 4 && db.Ado.Calls.Count(c => c.Kind == "query") == 1, "Expected four calls/one read");
            Call insert = db.Ado.Calls[1];
            Call history = History(db);
            HashSet<string> ids = new HashSet<string>();
            for (int i = 0; i < 50; i++)
            {
                string id = insert.Parameters["@id" + i].ToString();
                Check(ids.Add(id) && Guid.Parse(id) != Guid.Empty, "Unique new ID");
                Check(history.Parameters["@id" + i].Equals(id), "Inventory/history ID mismatch");
                Check(history.Parameters["@no" + i].Equals("N-" + i), "Needle mapping");
                Check(history.Parameters["@pos" + i].Equals(i + 1), "Position mapping");
                Check(history.Parameters["@fs" + i] == DBNull.Value && history.Parameters["@rm" + i].Equals("新绑"), "New history");
            }
            Check(insert.Parameters["@src"] == DBNull.Value && history.Parameters["@src"] == DBNull.Value, "Nullable source");
        });

        Run("50 existing needles preserve original snapshots regardless of query order", () => {
            SqlSugarClient db = new SqlSugarClient();
            string[] states = { "INBOUND_PENDING", "AVAILABLE", "OUTBOUND", "BIND_CANCELLED", "ADJUSTED_OUT" };
            for (int i = 49; i >= 0; i--) Old(db, i, states[i % states.Length]);
            Check(Bind(db, Needles(50)) == 50 && db.Ado.Calls.Count == 4, "Existing batch count");
            Call history = History(db);
            Call update = db.Ado.Calls.Single(c => c.Sql.Contains("JOIN (") && c.Kind == "write");
            Check(db.Ado.Calls.IndexOf(history) < db.Ado.Calls.IndexOf(update), "History must precede update");
            for (int i = 0; i < 50; i++)
            {
                Check(history.Parameters["@id" + i].Equals("ID-" + i), "Existing ID");
                Check(history.Parameters["@fs" + i].Equals(states[i % states.Length]), "Original state");
                Check(history.Parameters["@fi" + i].Equals("OLD-ITEM-" + i), "Original item");
                Check(history.Parameters["@fp" + i].Equals(50 - i), "Original position");
                Check(history.Parameters["@rm" + i].Equals("重绑"), "Existing remark");
                Check(update.Parameters["@id" + i].Equals("ID-" + i) && update.Parameters["@pos" + i].Equals(i + 1), "Update mapping");
            }
        });

        Run("mixed batch and removals use six calls with history before cancellation", () => {
            SqlSugarClient db = new SqlSugarClient();
            Old(db, 0, "OUTBOUND", "OLD-BOX"); Old(db, 2, "BIND_CANCELLED");
            db.Ado.CancelCount = 2;
            Check(Bind(db, Needles(4)) == 4 && db.Ado.Calls.Count == 6, "Mixed batch count");
            Call history = History(db);
            Check(history.Parameters["@fc0"].Equals("OLD-BOX") && history.Parameters["@fc1"] == DBNull.Value, "Mixed snapshots");
            Check(db.Ado.Calls[4].Sql.Contains("'BIND_CANCEL'") && db.Ado.Calls[5].Sql.Contains("SET STATUS='BIND_CANCELLED'"), "Cancel ordering");
            Check(db.Ado.Calls[4].Sql.Contains("STATUS='INBOUND_PENDING' AND NEEDLENO NOT IN"), "Cancel scope");
            for (int i = 0; i < 4; i++) Check(db.Ado.Calls[4].Parameters["@k" + i].Equals("N-" + i), "Kept list");
        });

        Run("invalid last slot rejects entire box before SQL", () => {
            SqlSugarClient db = new SqlSugarClient(); JArray rows = Needles(50); rows[49]["Pos"] = 51;
            Reject(db, rows, "必须在1~50"); Check(db.Ado.Calls.Count == 0, "Late validation read database");
        });
        Run("needle and slot duplicates are rejected before SQL", () => {
            SqlSugarClient db = new SqlSugarClient(); JArray rows = Needles(2); rows[1]["No"] = " n-0 ";
            Reject(db, rows, "重复针号"); Check(db.Ado.Calls.Count == 0, "Duplicate needle queried DB");
            rows = Needles(2); rows[1]["Pos"] = 1; Reject(db, rows, "槽位重复");
            Check(db.Ado.Calls.Count == 0, "Duplicate position queried DB");
        });
        Run("missing/overlong needle and malformed position are rejected", () => {
            SqlSugarClient db = new SqlSugarClient(); JArray rows = Needles(1); rows[0]["No"] = " "; Reject(db, rows, "缺少针号");
            rows = Needles(1); rows[0]["No"] = new string('A', 101); Reject(db, rows, "长度超过");
            rows = Needles(1); rows[0]["Pos"] = "1.5"; Reject(db, rows, "位置(Position)无效");
        });

        foreach (string status in new[] { "RESERVED", "QUARANTINED", "DISABLED" })
        {
            string current = status;
            Run(current + " guard rejects mixed batch without any writes", () => {
                SqlSugarClient db = new SqlSugarClient();
                Old(db, 1, current == "DISABLED" ? "OUTBOUND" : current, "W-BOX", current == "DISABLED" ? 0 : 1);
                Reject(db, Needles(2), current == "RESERVED" ? "ORDER-1" : current == "QUARANTINED" ? "隔离" : "已停用");
            });
        }
        foreach (string status in new[] { "INBOUND_PENDING", "AVAILABLE", "UNKNOWN_STATE" })
        {
            string current = status;
            Run("cross-box " + current + " rejects even without inventory book", () => {
                SqlSugarClient db = new SqlSugarClient(); Old(db, 1, current, "OTHER-BOX");
                Reject(db, Needles(2), "针号N-1已绑定到盒OTHER-BOX（状态" + current + "），禁止重复绑定到盒W-BOX");
                Check(db.Ado.Calls.Count == 1 && db.Ado.Calls[0].Kind == "query", "Must reject without stock lookup or writes");
            });
        }
        foreach (string status in new[] { "OUTBOUND", "BIND_CANCELLED", "ADJUSTED_OUT" })
        {
            string current = status;
            Run("cross-box ended binding " + current + " allows return with history", () => {
                SqlSugarClient db = new SqlSugarClient(); Old(db, 0, current, "OTHER-BOX");
                Check(Bind(db, Needles(1)) == 1 && db.Ado.Calls.Count == 4, "Allowed return");
                Call history = History(db);
                Check(history.Parameters["@fs0"].Equals(current) && history.Parameters["@fc0"].Equals("OTHER-BOX"), "Return must retain old binding history");
            });
        }
        Run("same box comparison ignores case and surrounding whitespace", () => {
            SqlSugarClient db = new SqlSugarClient(); Old(db, 0, "INBOUND_PENDING", " w-box ");
            Check(Bind(db, Needles(1)) == 1, "Same box update should remain allowed");
        });
        Run("database-equivalent existing needle aliases are rejected", () => {
            SqlSugarClient db = new SqlSugarClient(); Old(db, 0, "OUTBOUND"); Old(db, 1, "OUTBOUND");
            db.Ado.Old.Rows[1]["ID"] = "ID-0";
            Reject(db, Needles(2), "重复针号");
        });

        foreach (string kind in new[] { "inventory", "history", "update" })
        {
            string current = kind;
            Run("partial " + current + " write throws for caller rollback", () => {
                SqlSugarClient db = new SqlSugarClient(); db.Ado.ShortWrite = current;
                if (current == "update") Old(db, 0, "OUTBOUND");
                bool rejected = false;
                try { Bind(db, Needles(2)); }
                catch (Exception ex) { Check(ex.Message.Contains("数量与清单不一致"), ex.Message); rejected = true; }
                Check(rejected, "Partial write was accepted");
                Check(!db.Ado.Calls.Any(c => c.Sql.Contains("'BIND_CANCEL'")), "Must stop after partial write");
            });
        }
        Run("values stay parameterized", () => {
            SqlSugarClient db = new SqlSugarClient(); JArray rows = Needles(1); string no = "N'/*@unexpected*/--"; rows[0]["No"] = no;
            Check(Bind(db, rows) == 1, "Special character needle");
            Check(db.Ado.Calls.All(c => !c.Sql.Contains(no)), "Input embedded in SQL");
            Check(History(db).Parameters["@no0"].Equals(no), "Input lost");
        });
        Run("available counts with no candidates perform no SQL", () => {
            SqlSugarClient db = new SqlSugarClient();
            Check(NeedleService.GetAvailableCountMap(db, null).Count == 0, "Null candidates");
            Check(NeedleService.GetAvailableCountMap(db, new string[0]).Count == 0, "Empty candidates");
            Check(NeedleService.GetAvailableCountMap(db, new[] { "", " ", (string)null }).Count == 0, "Blank candidates");
            Check(db.Ado.Calls.Count == 0, "Empty scope must not read all inventory");
        });
        Run("available counts include only candidate boxes and retain historical fallback", () => {
            SqlSugarClient db = new SqlSugarClient();
            db.Ado.AvailableCounts["BOX-A"] = 12; db.Ado.AvailableCounts["BOX-B"] = 37;
            db.Ado.AvailableCounts["UNRELATED"] = 50;
            var map = NeedleService.GetAvailableCountMap(db, new[] { "BOX-A", "BOX-B", "HISTORICAL", "box-a" });
            Check(db.Ado.Calls.Count == 1 && db.Ado.Calls[0].Parameters.Count == 3, "Batch scope/deduplication");
            Check(map.Count == 2 && map["box-a"] == 12 && map["BOX-B"] == 37, "Candidate counts");
            Check(!map.ContainsKey("UNRELATED") && !map.ContainsKey("HISTORICAL"), "Unrelated/historical count fabricated");
        });
        Run("candidate box identifiers stay parameterized", () => {
            SqlSugarClient db = new SqlSugarClient(); string box = "BOX' OR 1=1 --";
            db.Ado.AvailableCounts[box] = 7;
            Check(NeedleService.GetAvailableCountMap(db, new[] { box })[box] == 7, "Special character box");
            Check(!db.Ado.Calls[0].Sql.Contains(box) && db.Ado.Calls[0].Parameters.Values.Single().Equals(box), "Box embedded in SQL");
        });
        Run("large candidate scope merges batches without missing or duplicate boxes", () => {
            SqlSugarClient db = new SqlSugarClient();
            string[] boxes = Enumerable.Range(0, 1201).Select(i => "BOX-" + i).ToArray();
            for (int i = 0; i < boxes.Length; i++) db.Ado.AvailableCounts[boxes[i]] = i + 1;
            var map = NeedleService.GetAvailableCountMap(db, boxes.Concat(new[] { boxes[0] }));
            Check(db.Ado.Calls.Count == 3 && db.Ado.Calls.All(c => c.Parameters.Count <= 500), "Query batch limit");
            Check(db.Ado.Calls.Sum(c => c.Parameters.Count) == boxes.Length, "Duplicate/missing candidate");
            Check(map.Count == boxes.Length && boxes.Select((box, i) => map[box] == i + 1).All(x => x), "Batch results lost");
        });
        Console.WriteLine("Passed " + passed + " tests. No live MySQL execution or concurrency test was performed.");
    }
}

internal sealed class Call
{
    public string Kind;
    public string Sql;
    public Dictionary<string, object> Parameters;
}

namespace SqlSugar
{
    public sealed class SugarParameter
    {
        public string ParameterName;
        public object Value;
        public SugarParameter(string name, object value) { ParameterName = name; Value = value; }
    }
    public sealed class SqlSugarClient
    {
        public readonly RecordingAdo Ado = new RecordingAdo();
    }
    public sealed class RecordingAdo
    {
        internal readonly List<Call> Calls = new List<Call>();
        internal readonly DataTable Old = new DataTable();
        internal readonly Dictionary<string, int> AvailableCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        internal int CancelCount;
        internal string ShortWrite;

        public RecordingAdo()
        {
            Old.Columns.Add("BINDINDEX", typeof(int)); Old.Columns.Add("ID"); Old.Columns.Add("STATUS");
            Old.Columns.Add("ITEMCODE"); Old.Columns.Add("CONTAINER"); Old.Columns.Add("POSITIONCODE", typeof(int));
            Old.Columns.Add("ENABLED", typeof(int)); Old.Columns.Add("RESERVEORDER");
        }
        private Call Record(string kind, string sql, List<SugarParameter> parameters)
        {
            Call call = new Call { Kind = kind, Sql = sql, Parameters = (parameters ?? new List<SugarParameter>()).ToDictionary(p => p.ParameterName, p => p.Value) };
            foreach (Match match in Regex.Matches(sql, @"@[A-Za-z][A-Za-z0-9]*"))
                if (!call.Parameters.ContainsKey(match.Value)) throw new Exception("Missing SQL parameter " + match.Value);
            Calls.Add(call);
            return call;
        }
        public DataTable GetDataTable(string sql, List<SugarParameter> parameters = null)
        {
            Call call = Record("query", sql, parameters);
            if (sql.Contains("COUNT(*) AS CNT"))
            {
                if (!sql.Contains("CONTAINER IN (") || !sql.Contains("STATUS='AVAILABLE'")
                    || !sql.Contains("ENABLED=1") || !sql.Contains("GROUP BY CONTAINER") || call.Parameters.Count == 0)
                    throw new Exception("Needle counts must be restricted to candidate boxes and enabled available needles");
                DataTable counts = new DataTable(); counts.Columns.Add("CONTAINER"); counts.Columns.Add("CNT", typeof(int));
                foreach (object value in call.Parameters.Values)
                {
                    int count;
                    if (AvailableCounts.TryGetValue(value.ToString(), out count)) counts.Rows.Add(value.ToString(), count);
                }
                return counts;
            }
            if (!sql.Contains("FOR UPDATE")) throw new Exception("Existing rows must be locked");
            return Old;
        }
        public object GetScalar(string sql, List<SugarParameter> parameters)
        {
            throw new Exception("BindNeedles must not query the inventory book to permit cross-box transfer.");
        }
        public int ExecuteCommand(string sql, List<SugarParameter> parameters)
        {
            Call call = Record("write", sql, parameters);
            if (sql.Contains("'BIND_CANCEL'") || sql.Contains("SET STATUS='BIND_CANCELLED'")) return CancelCount;
            string kind = sql.Contains("wms_needle_transaction") ? "history" : sql.TrimStart().StartsWith("UPDATE") ? "update" : "inventory";
            int count = call.Parameters.Keys.Count(p => Regex.IsMatch(p, @"^@id\d+$"));
            return ShortWrite == kind ? count - 1 : count;
        }
    }
}
