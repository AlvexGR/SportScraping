namespace TQI.Infrastructure.Entity
{
    public class ApiResult<T>
    {
        public bool Succeed { get; set; }

        public string Error { get; set; }

        public T Result { get; set; }

        public double ExecutionTime { get; set; }
    }
}
