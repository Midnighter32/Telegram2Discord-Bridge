namespace Test.Services
{
    public interface IConfigurationService
    {
        public T GetValue<T>(string key);

        public void SetValue<T>(string key, T value);

        public void Save();
    }
}
