using System;
using System.Collections.Generic;
using System.Text;

namespace PerkinElmerSP2CSV
{
    public class HistoryRecordParser
    {
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

        public string[] GetHistoryRecords()
        {
            List<string> records = new List<string>();
            var raw = _tmb.Data;

            for (int i = 0; i < raw.Length-4; i++)
            {
                if (raw[i] == 0x23 && raw[i + 1] == 0x75) // #u is the start of a new record
                {
                    // get the length of the current record (the 2 bytes after #u)
                    byte[] arr = { raw[i+2], raw[i + 3] }; // arr[0]=low, arr[1]=high on little-endian systems
                    short len = BitConverter.ToInt16(arr, 0);
                    string record = Encoding.ASCII.GetString(raw, i+4, len);
                    records.Add(record);
                }
            }
            return records.ToArray();
        }

        private readonly TypedMemberBlock _tmb;

    }
}
