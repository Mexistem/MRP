using MRP.Server.Models;

public interface IRatingRepository
{
    IEnumerable<RatingEntry> GetAll();

    IEnumerable<RatingEntry> GetByMediaTitle(string mediaTitle);

    RatingEntry? GetByMediaTitleAndUsername(string mediaTitle, string username);

    void Add(RatingEntry rating);

    void Update(RatingEntry rating);
    void DeleteByMediaTitle(string mediaTitle);
    void DeleteByUsername(string username);

    bool DeleteRating(string mediaTitle, string username);

    void RenameMediaTitle(string oldTitle, string newTitle);
}
