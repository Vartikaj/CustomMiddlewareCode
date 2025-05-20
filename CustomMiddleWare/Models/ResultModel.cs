namespace CustomMiddleWare.Models
{
    public class ResultModel<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int status { get; set; }
        public bool error { get; set; }
        public T data { get; set; }
        public List<T> LstModel { get; set; }
        public ResultModel() {
            success = false;
            error = true;
        }
    }
}
