using System;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public class HistoryRecordEntry
    {
        public int TitleCode { get; set; } // positive definite version
        public string RecordText { get; set; }
        public string KeyName => ToKeyName(TitleCode);
        public bool IsKnownRecord => Enum.IsDefined(typeof(HistoryRecordTitles), TitleCode);
        public string Delimiter { get; set; } // either "#u" or "-u" (in Hex: 0x23 0x75 or 0x2D 0x75)

        private string ToKeyName(int code)
        {
            if (IsKnownRecord)
                return ((HistoryRecordTitles)code).ToString();
            else
                return $"_UnknownRecord{Delimiter}_ID{code:D3}";
        }

        public override string ToString() => $"Id: {TitleCode,3}, Record: {RecordText}";
    }
}
