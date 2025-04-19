namespace LowLand.Utils
{
    public static class TableStatuses
    {
        public const string Empty = "Trống";
        public const string Occupied = "Có khách";
        public const string Maintenance = "Đang bảo trì";

        public static readonly string[] All = new[]
        {
            Empty,
            Occupied,
            Maintenance
        };
    }

}
