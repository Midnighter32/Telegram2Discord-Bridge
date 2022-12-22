using Newtonsoft.Json;
using System.Text;
public abstract class IConfiguration<T> : IDisposable where T : class, new()
{
    public void Save(string storage_path = "")
    {
        if (storage_path == string.Empty)
            storage_path = typeof(T).Name.ToLower() + ".stg";

        byte[] data = Encoding.Default.GetBytes(ToString());
        using (FileStream fs = File.Open("Storage/" + storage_path, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fs.Write(data, 0, data.Length);
        }
    }

    public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

    public static T Load(string storage_path = "")
    {
        if (storage_path == string.Empty)
            storage_path = typeof(T).Name.ToLower() + ".stg";

        using (FileStream fs = File.Open("Storage/" + storage_path, FileMode.OpenOrCreate, FileAccess.Read))
        {
            byte[] data = new byte[fs.Length];

            fs.Read(data, 0, data.Length);

            var str = Encoding.Default.GetString(data);

            return JsonConvert.DeserializeObject<T>(str) ?? new T();
        }
    }

    public void Dispose() => Save();
}
