namespace PerkinElmerSP2CSV
{
    // Block IDs
    public enum Blocks : short
    {
        DSet2DC1DI = 120,
        HistoryRecord = 121,
        InstrHdrHistoryRecord = 122,
        InstrumentHeader = 123,
        IRInstrumentHeader = 124,
        UVInstrumentHeader = 125,
        FLInstrumentHeader = 126
    }

    // Data member IDs
    public enum Members : short
    {
        DataSetDataType = -29839,
        DataSetAbscissaRange = -29838,
        DataSetOrdinateRange = -29837,
        DataSetInterval = -29836,
        DataSetNumPoints = -29835,
        DataSetSamplingMethod = -29834,
        DataSetXAxisLabel = -29833,
        DataSetYAxisLabel = -29832,
        DataSetXAxisUnitType = -29831,
        DataSetYAxisUnitType = -29830,
        DataSetFileType = -29829,
        DataSetData = -29828,
        DataSetName = -29827,
        DataSetChecksum = -29826,
        DataSetHistoryRecord = -29825,
        DataSetInvalidRegion = -29824,
        DataSetAlias = -29823,
        DataSetVXIRAccyHdr = -29822,
        DataSetVXIRQualHdr = -29821,
        DataSetEventMarkers = -29820
    }

    // Type code IDs
    public enum TypeCodes : short
    {
        Short = 29999,
        UShort = 29998,
        Int = 29997,
        UInt = 29996,
        Long = 29995,
        Bool = 29988,
        Char = 29987,
        CvCoOrdPoint = 29986,
        StdFont = 29985,
        CvCoOrdDimension = 29984,
        CvCoOrdRectangle = 29983,
        RGBColor = 29982,
        CvCoOrdRange = 29981,
        Double = 29980,
        CvCoOrd = 29979,
        ULong = 29978,
        Peak = 29977,
        CoOrd = 29976,
        Range = 29975,
        CvCoOrdArray = 29974,
        Enum = 29973,
        LogFont = 29972
    }

    // History record title codes
    public enum HistoryRecordTitles : int
    {
        OperatorName = 1,
        Modification = 2,
        ModificationDateTime = 3,
        Parameters = 4,
        SampleDescription = 5,
        InstrumentModel = 140,
        InstrumentSerialNumber = 141,
        InstrumentSoftwareRevision = 142,
        SlitWidth = 210,
        LampsUsed = 213,
        InstrumentAccessories = 218,
        UvVisSlitMode = 224,
        NirSlitMode = 225,
        NirSlitWidth = 226,
        UvVisIntegrationTime = 227,
        NirIntegrationTime = 228,
        NirDetectorGain = 230,
        DetectorChangeAt = 238,
        SampleBeamPosition = 239,
        CommonBeamDepolarizer = 241,
        AttenuatorsUsed = 242
    }
}
