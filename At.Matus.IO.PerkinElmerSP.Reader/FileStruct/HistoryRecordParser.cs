using System;
using System.Collections.Generic;
using System.Text;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public class HistoryRecordParser
    {
        private const bool ENABLE_VALIDATION = true;

        public HistoryRecordParser(TypedMemberBlock tmb)
        {
            if (tmb.Id != (short)Members.DataSetHistoryRecord)
                throw new NotSupportedException("Not supported data type for history record.");
            _tmb = tmb;
        }

        public Dictionary<string, string> GetHistoryRecordsAsDictionary()
        {
            var historyRecords = GetHistoryRecordsAsObjects();
            Dictionary<string, string> records = new Dictionary<string, string>();
            for (int i = 0; i < historyRecords.Length; i++)
            {
                records[historyRecords[i].KeyName] = historyRecords[i].RecordText;//.Trim().Trim('"');
            }
            return records;
        }

        public HistoryRecord[] GetHistoryRecordsAsObjects()
        {
            List<HistoryRecord> records = new List<HistoryRecord>();
            var raw = _tmb.Data;
            // first find the "#u"-records
            for (int i = 6; i < raw.Length - 4; i++)
            {
                // The separator between records consists of the two bytes 0x23 0x75 "#u" followed
                // by the record length (2 bytes) and the record text (len bytes).
                // The separator also includes 3 short integers (6 bytes) before the "#u".
                // The first short integer (id1) is considered as the actual record id,
                // the second short integer (id2) is the length of the record text plus 4,
                // and the third short integer (id3) is always 0.

                if (raw[i] == 0x23 && raw[i + 1] == 0x75) // #u is the start of a new record
                {
                    var historyRecord = new HistoryRecord();
                    // get the length of the current record (the 2 bytes after #u)
                    short len = BitConverter.ToInt16(new byte[] { raw[i + 2], raw[i + 3] }, 0);
                    string record = Encoding.ASCII.GetString(raw, i + 4, len);
                    short id1 = BitConverter.ToInt16(new byte[] { raw[i - 6], raw[i - 5] }, 0);
                    short id2 = BitConverter.ToInt16(new byte[] { raw[i - 4], raw[i - 3] }, 0);
                    short id3 = BitConverter.ToInt16(new byte[] { raw[i - 2], raw[i - 1] }, 0);
                    if (ENABLE_VALIDATION)
                    {
                        if (id3 != 0)
                            throw new ArgumentException($"Unexpected non-zero id3 for record '{record}' with id1: {id1}, id2: {id2}, id3: {id3} and len: {len}");
                        if (id2 - len != 4)
                            throw new ArgumentException($"Inconsistent record length for record '{record}' with id1: {id1}, id2: {id2}, id3: {id3} and len: {len}");
                    }
                    historyRecord.RecordText = record;
                    historyRecord.RecordLength = len;
                    historyRecord.TitleCode = (id1 + 29839); // to make the id positive
                    historyRecord.Offset = i - 6;
                    historyRecord.Delimiter = "2375";
                    records.Add(historyRecord);
                }
            }
            // now find the "-u"-records, which are similar to the "#u"-records
            for (int i = 6; i < raw.Length - 4; i++)
            {
                if (raw[i] == 0x2D && raw[i + 1] == 0x75) // -u is the start of a new mysterious record
                {
                    var historyRecord = new HistoryRecord();
                    short val = BitConverter.ToInt16(new byte[] { raw[i + 2], raw[i + 3] }, 0);
                    short id1 = BitConverter.ToInt16(new byte[] { raw[i - 6], raw[i - 5] }, 0);
                    short id2 = BitConverter.ToInt16(new byte[] { raw[i - 4], raw[i - 3] }, 0);
                    short id3 = BitConverter.ToInt16(new byte[] { raw[i - 2], raw[i - 1] }, 0);
                    if (ENABLE_VALIDATION)
                    {
                        if (id3 != 0)
                            throw new ArgumentException($"Unexpected non-zero id3 for record with id: {id1}.");
                        if (id2 != 8)
                            throw new ArgumentException($"Unexpected value id2 for record with id: {id1}.");
                    }
                    historyRecord.RecordText = val.ToString();
                    historyRecord.TitleCode = (id1 + 29839); // to make the id positive
                    historyRecord.RecordLength = 0;
                    historyRecord.Offset = i - 6;
                    historyRecord.Delimiter = "2D75";
                    records.Add(historyRecord);
                }
            }
            // now find the ".u"-records, which are similar to the "#u"-records
            for (int i = 6; i < raw.Length - 4; i++)
            {
                if (raw[i] == 0x1C && raw[i + 1] == 0x75) // .u is the start of a new mysterious record
                {
                    var historyRecord = new HistoryRecord();
                    double val = BitConverter.ToDouble(new byte[] { raw[i + 2], raw[i + 3], raw[i + 4], raw[i + 5], raw[i + 6], raw[i + 7], raw[i + 8], raw[i + 9] }, 0);
                    short id1 = BitConverter.ToInt16(new byte[] { raw[i - 6], raw[i - 5] }, 0);
                    short id2 = BitConverter.ToInt16(new byte[] { raw[i - 4], raw[i - 3] }, 0);
                    short id3 = BitConverter.ToInt16(new byte[] { raw[i - 2], raw[i - 1] }, 0);
                    if (ENABLE_VALIDATION)
                    {
                        if (id3 != 0)
                            throw new ArgumentException($"Unexpected non-zero id3 for record with id: {id1}.");
                        if (id2 != 14)
                            throw new ArgumentException($"Unexpected value id2 for record with id: {id1}.");
                    }
                    historyRecord.RecordText = $"{val:f1}";
                    historyRecord.TitleCode = (id1 + 29839); // to make the id positive
                    historyRecord.RecordLength = 0;
                    historyRecord.Offset = i - 6;
                    historyRecord.Delimiter = "1C75";
                    records.Add(historyRecord);
                }
            }
            return records.ToArray();
        }

        public string[] GetHistoryRecords()
        {
            var historyRecords = GetHistoryRecordsAsObjects();
            string[] records = new string[historyRecords.Length];
            for (int i = 0; i < historyRecords.Length; i++)
                records[i] = historyRecords[i].RecordText.Trim().Trim('"');
            return records;
        }

        private readonly TypedMemberBlock _tmb;
    }

    public class HistoryRecord
    {
        public int TitleCode { get; set; } // positive definite version
        public string RecordText { get; set; }
        public string KeyName => ToKeyName(TitleCode);
        public short RecordLength { get; set; }
        public int Offset { get; set; } // within the byte array of the DataSetHistoryRecord-TypedMemberBlock
        public string Delimiter { get; set; } // either "#u" or "-u" (in Hex: 0x23 0x75 or 0x2D 0x75)
        
        private string ToKeyName(int code)
        {
            if (Enum.IsDefined(typeof(HistoryRecordTitles), code))
                return ((HistoryRecordTitles)code).ToString();
            else
                return $"_UnknownRecord{Delimiter}_ID{code:D3}";
        }

        public override string ToString() => $"Id: {TitleCode,3}, RecordLength: {RecordLength,3}, Offset: {Offset,4}, Record: {RecordText}";
    }

}
