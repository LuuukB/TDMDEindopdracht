using SQLite;


namespace TDMDEindopdracht.Domain.Model
{
    [Table("route")]
    public class Route
    {
        public string Name { get; set; }
        [PrimaryKey, AutoIncrement, Column("Index")]
        public int Index { get; set; }
        public RouteType Type { get; set; }
        public string Distance { get; set; }
        public string TotalRunTime { get; set; }
        public string AveradgeSpeed { get; set; }
    }

    public enum RouteType
    {
        HardLopen,
        Fietsen,
        Skaten,
        Wandelen,
    }
}
