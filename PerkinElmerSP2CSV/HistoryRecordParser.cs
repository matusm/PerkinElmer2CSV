using System;
using System.Collections.Generic;
using System.Text;

namespace PerkinElmerSP2CSV
{
    public class HistoryRecordParser
    {
        private const bool checkConsistency = true;

        public HistoryRecordParser(TypedMemberBlock tmb)
        {
            if(tmb.Id != (short)Members.DataSetHistoryRecord)
                throw new NotSupportedException("Not supported data type for history record.");
            _tmb = tmb;
        }

        public string GetHistoryRecordAsString()
        {
            var records = GetHistoryRecords();
            StringBuilder sb = new StringBuilder();
            foreach (string record in records)
            {
                sb.AppendLine($"'{record}'");
            }
            return sb.ToString();
        }

        public HistoryRecord[] GetHistoryRecordsAsObjects()
        {
            List<HistoryRecord> records = new List<HistoryRecord>();
            var raw = _tmb.Data;
            for (int i = 6; i < raw.Length - 4; i++)
            {
                if (raw[i] == 0x23 && raw[i + 1] == 0x75) // #u is the start of a new record
                {
                    var historyRecord = new HistoryRecord();
                    // get the length of the current record (the 2 bytes after #u)
                    short len = BitConverter.ToInt16(new byte[] { raw[i + 2], raw[i + 3] }, 0);
                    string record = Encoding.ASCII.GetString(raw, i + 4, len);
                    short id1 = BitConverter.ToInt16(new byte[] { raw[i - 6], raw[i - 5] }, 0);
                    short id2 = BitConverter.ToInt16(new byte[] { raw[i - 4], raw[i - 3] }, 0);
                    short id3 = BitConverter.ToInt16(new byte[] { raw[i - 2], raw[i - 1] }, 0);

                    if(checkConsistency)
                    {
                        if(id3!=0)
                            throw new ArgumentException($"Unexpected non-zero id3 for record '{record}' with id1: {id1}, id2: {id2}, id3: {id3} and len: {len}");
                        if(id2-len != 4)
                            throw new ArgumentException($"Inconsistent record length for record '{record}' with id1: {id1}, id2: {id2}, id3: {id3} and len: {len}");
                    }

                    historyRecord.RecordText = record;
                    historyRecord.RecordLength = len;
                    historyRecord.Id = (id1 + 29839);
                    records.Add(historyRecord);
                }
            }
            return records.ToArray();
        }

        public string[] GetHistoryRecords()
        {
            List<string> records = new List<string>();
            var raw = _tmb.Data;

            for (int i = 0; i < raw.Length-4; i++)
            {
                if (raw[i] == 0x23 && raw[i + 1] == 0x75) // #u is the start of a new record
                {
                    // get the length of the current record (the 2 bytes after #u)
                    short len = BitConverter.ToInt16(new byte[] { raw[i + 2], raw[i + 3] }, 0);
                    string record = Encoding.ASCII.GetString(raw, i + 4, len);
                    records.Add(record.Trim().Trim('"'));
                }
            }
            return records.ToArray();
        }

        private readonly TypedMemberBlock _tmb;
    }

    public class HistoryRecord
    {
        public int Id { get; set; }
        public string RecordText { get; set; }
        public short RecordLength { get; set; }

        public override string ToString()
        {
            return $"Id: {Id,3}, RecordLength: {RecordLength,3}, Record: {RecordText}";
        }
    }
}
