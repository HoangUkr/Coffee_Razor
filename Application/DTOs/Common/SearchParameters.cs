namespace Application.DTOs.Common
{
    public class SearchParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
