using SecretHitler.Application.Common.Interfaces;
using System;

namespace SecretHitler.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }
}
