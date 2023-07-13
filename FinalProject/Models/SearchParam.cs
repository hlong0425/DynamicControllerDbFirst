namespace FinalProject.Models
{
    public class SearchParam
    {
        public string Search { get; set; } = string.Empty;

        public string Filter { get; set; } = "Name";

        public string OrderBy { get; set; } = "Name";
    }
}