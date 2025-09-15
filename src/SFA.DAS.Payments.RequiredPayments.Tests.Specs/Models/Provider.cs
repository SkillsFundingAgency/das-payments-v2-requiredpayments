using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.Models
{
    public class Provider
    {
        public int Ukprn { get; private set; }

        public DateTime LastUsed { get; private set; }

        internal void Use() => LastUsed = DateTime.UtcNow;
    }
}
