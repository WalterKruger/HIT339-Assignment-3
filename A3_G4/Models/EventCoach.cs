namespace A3_G4.Models
{
    public class EventCoach
    {
        public int Id { get; set; }

        // Foreign keys. Can't get entity framework to work with external tables, so the FK relationship is enforced by the controller
        public int CoachId { get; set; }
        public int ScheduleId { get; set; }
    }
}
