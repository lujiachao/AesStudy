using Gaea.MySql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AESTest
{
    public class ApplicationAuthUserAdapter : GaeaMySqlPower<ApplicationAuthUser>
    {
        public void UpdateAuthUser(string mobilephone, string name, string identityCode)
        {
            var commandText = $"Update {GaeaName} set name = @name, identityCode = @identityCode where mobilephone = @mobilephone and channel = 13";
            Execute(commandText, new { name, identityCode, mobilephone});

        }
    }
}
