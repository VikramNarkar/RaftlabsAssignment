using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalProvider.Models.Dto
{
    public class UserResponseDto
    {
        public UserDto Data { get; set; }
        public SupportDto Support { get; set; }
    }
}
