using SQLite;


namespace TDMDEindopdracht.Domain.Model
{
    [Table("route")]
    public class Route
    {
        public string Name { get; set; }
        [PrimaryKey, AutoIncrement, Column("Index")]
        public int Index { get; set; }
        public double Distance { get; set; }
        public string TotalRunTime { get; set; }
        public double AveradgeSpeed { get; set; }
    }
}
