using At.Matus.MetaData;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public class Spectrum2d
    {
        public MeasurementMetaData MetaData { get; set; } = new MeasurementMetaData();
        public double StartX { get; set; }
        public double EndX { get; set; }
        public double ResolutionX { get; set; }
        public string LabelX { get; set; }
        public string LabelY { get; set; }
        public Point2d[] Points { get; set; }
    }
}
