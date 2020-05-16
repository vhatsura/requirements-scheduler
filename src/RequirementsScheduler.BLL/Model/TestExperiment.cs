namespace RequirementsScheduler.BLL.Model
{
    public class TestExperiment
    {
        public ProcessingTime[] J1 { get; set; }
        public ProcessingTime[] J2 { get; set; }

        //public Tuple<ProcessingTime[], ProcessingTime[]> J12 { get; set; }
        public Test J21 { get; set; }
        public Test J12 { get; set; }
    }

    public class Test
    {
        public ProcessingTime[] OnFirst { get; set; }
        public ProcessingTime[] OnSecond { get; set; }
    }

    //public 
}
