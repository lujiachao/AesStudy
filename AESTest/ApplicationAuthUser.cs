using Gaea;
using System;
using System.Collections.Generic;
using System.Text;

namespace AESTest
{
    [GaeaName("application_auth_user")]
    public class ApplicationAuthUser : GaeaSon
    {
        public int IdUser { get; set; }

        public string UID { get; set; }

        public int Status { get; set; }

        public int Channel { get; set; }

        public string ChannelName { get; set; }

        public string GuidUser { get; set; }

        public string RelationId { get; set; }

        public string Name { get; set; }

        public string NickName { get; set; }

        public string Mobilephone { get; set; }

        public string IdentityCode { get; set; }

        public string AuthCode { get; set; }

        public DateTime TimeCreate { get; set; }

        public DateTime TimeUpdate { get; set; }
    }
}
