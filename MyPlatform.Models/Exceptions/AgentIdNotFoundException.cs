namespace MyPlatformModels.Exceptions
{
    public class AgentIdNotFoundException : Exception
    {
        public AgentIdNotFoundException() : base("Agent ID could not be retrieved.")
        {
        }
    }
}
