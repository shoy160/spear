using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spear.AutoMapper.Attributes;
using Spear.AutoMapper;
using Spear.Framework;

namespace Spear.Tests
{
    [TestClass]
    public class AutoMapperTest : DTest
    {
        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class UserDto
        {
            [MapFrom("Id")]
            public int UserId { get; set; }
            [MapFrom("Name")]
            public string UserName { get; set; }
        }

        [TestMethod]
        public void MapTest()
        {
            var user = new User
            {
                Id = 1,
                Name = "ddd"
            };

            var dto = user.MapTo<UserDto>();
            Print(dto);
        }
    }
}
