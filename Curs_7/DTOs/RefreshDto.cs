using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curs_7.DTOs
{
    public class RefreshDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
