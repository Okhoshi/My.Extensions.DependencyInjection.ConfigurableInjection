namespace MemoryInjectionSample.Models
{
    public class MemoryServiceConfigurationProvider : IServiceConfigurationProvider<string, MemoryServiceConfigurationProvider.MemoryConfig>
    {
        private string _In;
        private string _Out;

        public void SetInAndOut(string pIn, string pOut)
        {
            _In = pIn;
            _Out = pOut;
        }

        public MemoryConfig GetConfiguration(string key)
        {
            if (_In == key)
            {
                return new MemoryConfig {
                    InterfaceKey = _In,
                    ImplementationKey = _Out
                };
            }
            else
            {
                return null;
            }
        }

        public class MemoryConfig : ConfigurationBase<string>
     {

     }   
    }
}