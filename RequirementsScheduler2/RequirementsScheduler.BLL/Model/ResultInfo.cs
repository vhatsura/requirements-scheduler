namespace RequirementsScheduler.BLL.Model
{
    public enum ResultType
    {
        STOP1_1,
        STOP1_2,
        STOP1_3,
        STOP1_4
    }

    public class ResultInfo
    {
        public ResultType? Type { get; set; }
    }
}
