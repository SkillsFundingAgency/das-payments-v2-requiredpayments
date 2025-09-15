using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.StepDefinitions
{
    public class ProviderService
    {
        private readonly TestSessionDataContext dataContext;
        private string appGuid;

        public ProviderService(TestSessionDataContext dataContext, string appGuid )
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.appGuid = appGuid;
        }

        public int GenerateUkprn()
        {
            //string appGuid =
            //    ((GuidAttribute)Assembly.GetExecutingAssembly().
            //        GetCustomAttributes(typeof(GuidAttribute), false).
            //        GetValue(0)).Value.ToString();

            Provider provider = null;
            using (var mutex = new Mutex(false, $"Global\\{{{appGuid}}}"))
            {
                if (mutex.WaitOne(TimeSpan.FromMinutes(1)))
                {
                    provider = GetProvider();
                    // check job queue for ukprn - looking for status 2 or 3 which will block queue
                    //var blockedList = jobService.GetJobsByStatus(provider.Ukprn, 2, 3).Result;
                    //if (blockedList.Any())
                    //{
                    //    provider = GetProvider();
                    //}

                    dataContext.ClearPaymentsData(provider.Ukprn);
                    mutex.ReleaseMutex();
                }
                else
                {
                    throw new ApplicationException("Unable to obtain a Ukprn due to a locked Mutex");
                }
            }

            return provider.Ukprn;
        }

        private Provider GetProvider()
        {
            var provider = dataContext.LeastRecentlyUsed();
            provider.Use();
            dataContext.SaveChanges();
            return provider;
        }
    }
}
