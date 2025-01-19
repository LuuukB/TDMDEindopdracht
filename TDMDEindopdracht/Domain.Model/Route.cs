using SQLite;


namespace TDMDEindopdracht.Domain.Model
{
    [Table("route")]
    public class Route
    {
        public string? Name { get; set; }
        [PrimaryKey, AutoIncrement, Column("Index")]
        public int Index { get; set; }
        public double Distance { get; set; }
        public string? TotalRunTime { get; set; }
        public double AveradgeSpeed { get; set; }
        public DateTime Time { get; set; }

        public void StartRoute()
        {
            Time = DateTime.Now;
        }

        public void StopRoute(double distance, string name)
        {
            var runTime = DateTime.Now - Time;
            var runTimeWhithoutMiliseconds = new TimeSpan(runTime.Days, runTime.Hours, runTime.Minutes, runTime.Seconds);
            TotalRunTime = runTimeWhithoutMiliseconds.ToString();
            Name = name;
            Distance = distance;
            AveradgeSpeed = distance / runTimeWhithoutMiliseconds.TotalHours;
        }
    }

    
}
