using CompanyService.Entities;

namespace CompanyService.DTO
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public UserType UserType { get; set; }

    }
}
