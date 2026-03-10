namespace QuickStock.Controllers
{
    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Usernames { get; set; } = new();
    }
}
