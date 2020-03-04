using System;
using Spear.Tests.Contracts.Enums;

namespace Spear.Tests.Contracts.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public DateTime Birthday { get; set; }
    }
}
