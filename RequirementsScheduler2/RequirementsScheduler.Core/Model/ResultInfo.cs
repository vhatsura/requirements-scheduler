namespace RequirementsScheduler.Core.Model
{
    public enum ResultType
    {
        STOP1_1,
        STOP1_2
    }

    public class ResultInfo
    {
        public ResultType Type { get; set; }
    }
}
