namespace ScryfallDownloader.Models.Dtos
{
    public class SetListDto : PaginatedListDto
    {
        public List<SetModel> Data { get; set; }
    }
}
