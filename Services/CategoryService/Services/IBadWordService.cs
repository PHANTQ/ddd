namespace CategoriesService.Services
{
    public interface IBadWordService
    {
        Task<(bool IsBad, List<string> BadWords)> CheckProfanityAsync(string text);
    }
}
