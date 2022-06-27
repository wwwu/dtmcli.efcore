using DtmCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtm.EFCore
{
    public class DtmOptionsExt : DtmOptions
    {
        public string HostName { get; set; }

        public string QueryPreparedPath { get; set; } = "/dtm/queryprepared";
    }
}
