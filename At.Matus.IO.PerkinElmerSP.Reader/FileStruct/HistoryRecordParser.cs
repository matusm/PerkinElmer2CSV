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

        public Dictionary<string, string> GetHistoryRecordsAsDictionary() => GetHistoryRecordsAsDictionary(false);

        public Dictionary<string, string> GetHistoryRecordsAsDictionary(bool includeUnknowns)
        {
            var historyRecords = GetHistoryRecordsAsObjects();
            Dictionary<string, string> records = new Dictionary<string, string>();
            for (int i = 0; i < historyRecords.Length; i++)
                if(includeUnknowns || historyRecords[i].IsKnownRecord)
                    records[historyRecords[i].KeyName] = historyRecords[i].RecordText;
            return records;
        }

        private (byte b1, byte b2) GetDelimiterBytes(HistoryRecordValueTypes type)
        {
            switch (type)
            {
                case HistoryRecordValueTypes.Text:
                    return (0x23, 0x75); // #u
                case HistoryRecordValueTypes.ShortInt:
                    return (0x2D, 0x75); // -u
                case HistoryRecordValueTypes.Double:
                    return (0x1C, 0x75); // .u
                default:
                    throw new ArgumentException($"Unsupported history record value type: {type}");
            }
        }

        private HistoryRecordEntry[] GetHistoryRecordsAsObjects(HistoryRecordValueTypes type)
        {
            var (b1, b2) = GetDelimiterBytes(type);
            List<HistoryRecordEntry> records = new List<HistoryRecordEntry>();
            for (int i = 6; i < _tmb.Data.Length - 4; i++)
            {
                var historyRecord = new HistoryRecordEntry();
                if (_tmb.Data[i] == b1 && _tmb.Data[i + 1] == b2)
                {
                    short id1 = BitConverter.ToInt16(new byte[] { _tmb.Data[i - 6], _tmb.Data[i - 5] }, 0);
                    short id2 = BitConverter.ToInt16(new byte[] { _tmb.Data[i - 4], _tmb.Data[i - 3] }, 0);
                    short id3 = BitConverter.ToInt16(new byte[] { _tmb.Data[i - 2], _tmb.Data[i - 1] }, 0);
                    if (id3 != 0)
                        throw new ArgumentException($"Unexpected non-zero id3 for record with id1: {id1}, id2: {id2}, id3: {id3}.");
                    historyRecord.ID = (id1 + 29839); // to make the id positive
                    historyRecord.Delimiter = $"{b1:X2}{b2:X2}";
                    switch (type)
                    {
                        case HistoryRecordValueTypes.Text:
                            short len = BitConverter.ToInt16(new byte[] { _tmb.Data[i + 2], _tmb.Data[i + 3] }, 0);
                            if (id2 - len != 4)
                                throw new ArgumentException($"Inconsistent record length for text record with id1: {id1}, id2: {id2}, id3: {id3}.");
                            historyRecord.RecordText = Encoding.ASCII.GetString(_tmb.Data, i + 4, len);
                            break;
                        case HistoryRecordValueTypes.ShortInt:
                            if (id2 != 8)
                                throw new ArgumentException($"Unexpected value id2 for short int record with id1: {id1}, id2: {id2}, id3: {id3}.");
                            historyRecord.RecordText = BitConverter.ToInt16(new byte[] { _tmb.Data[i + 2], _tmb.Data[i + 3] }, 0).ToString();
                            break;
                        case HistoryRecordValueTypes.Double:
                            if (id2 != 14)
                                throw new ArgumentException($"Unexpected value id2 for double record with id1: {id1}, id2: {id2}, id3: {id3}.");
                            historyRecord.RecordText = BitConverter.ToDouble(new byte[] { _tmb.Data[i + 2], _tmb.Data[i + 3], _tmb.Data[i + 4], _tmb.Data[i + 5], _tmb.Data[i + 6], _tmb.Data[i + 7], _tmb.Data[i + 8], _tmb.Data[i + 9] }, 0).ToString();
                            break;
                        default:
                            throw new ArgumentException($"Unsupported history record value type: {type}");
                    }
                }
            }
            return records.ToArray();
        }

        public HistoryRecordEntry[] GetHistoryRecordsAsObjects()
        {
            List<HistoryRecordEntry> records = new List<HistoryRecordEntry>();
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
                    var historyRecord = new HistoryRecordEntry();
                    // get the length of the current record (the 2 bytes after #u)
                    short len = BitConverter.ToInt16(new byte[] { raw[i + 2], raw[i + 3] }, 0);
                    string record = Encoding.Default.GetString(raw, i + 4, len);
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
                    historyRecord.ID = (id1 + 29839); // to make the id positive
                    historyRecord.Delimiter = "2375";
                    records.Add(historyRecord);
                }
            }
            // now find the "-u"-records, which are similar to the "#u"-records
            for (int i = 6; i < raw.Length - 4; i++)
            {
                if (raw[i] == 0x2D && raw[i + 1] == 0x75) // -u is the start of a new mysterious record
                {
                    var historyRecord = new HistoryRecordEntry();
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
                    historyRecord.ID = (id1 + 29839); // to make the id positive
                    historyRecord.Delimiter = "2D75";
                    records.Add(historyRecord);
                }
            }
            // now find the ".u"-records, which are similar to the "#u"-records
            for (int i = 6; i < raw.Length - 4; i++)
            {
                if (raw[i] == 0x1C && raw[i + 1] == 0x75) // .u is the start of a new mysterious record
                {
                    var historyRecord = new HistoryRecordEntry();
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
                    historyRecord.ID = (id1 + 29839); // to make the id positive
                    historyRecord.Delimiter = "1C75";
                    records.Add(historyRecord);
                }
            }
            return records.ToArray();
        }

        private readonly TypedMemberBlock _tmb;
    }

}
