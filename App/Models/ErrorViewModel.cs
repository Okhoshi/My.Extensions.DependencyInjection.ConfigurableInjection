using System;

namespace My.Extensions.DependencyInjection.ConfigurableInjection.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}