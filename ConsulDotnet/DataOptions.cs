using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace ConsulDotnet
{
    public class DataOptions : IOptions<DataOptions>
    {
        public string ConsulUrl { get; set; }

        #region IOptions<DataOptions> Members

        DataOptions IOptions<DataOptions>.Value => this;

        #endregion
    }
}
