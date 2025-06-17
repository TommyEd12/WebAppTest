namespace WebAppTest.Models
{
    public class ErrorViewModel
    {
        /// <summary>
        /// Класс для отображения ошибок
        /// </summary>
        /// <value></value>
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
