namespace Shop_BE.Entities
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string NextLink { get; set; }
        public string PrevLink { get; set; }

        public T Data { get; set; }
    }
}
