namespace SPTS_Shared.Constants
{
    public static class GradeThresholds
    {
        public const decimal PassingScore = 5.0m;
        public const decimal AlertThreshold = 5.0m;

        public const decimal ExcellentGpa = 3.6m;
        public const decimal GoodGpa = 3.2m;
        public const decimal AverageGpa = 2.5m;
        public const decimal BelowAverageGpa = 2.0m;

        public const int GpaRoundingScale = 2;
        public const int ScoreRoundingScale = 1;
    }
}