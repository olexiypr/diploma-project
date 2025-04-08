namespace Services.LlmService.Entities;

public class TripletRow
{
    public string head { get; set; }
    public string head_type { get; set; }
    public string relation { get; set; }
    public string tail { get; set; }
    public string tail_type { get; set; }
}