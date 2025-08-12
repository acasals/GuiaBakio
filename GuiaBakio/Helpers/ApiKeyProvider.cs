namespace GuiaBakio.Helpers
{
    using System.Text.Json;

    public static class ApiKeyProvider
    {
        public static string GetApiKey()
        {
            var path =  Path.Combine(AppContext.BaseDirectory,"secrets.json");

            if (!File.Exists(path))
                throw new FileNotFoundException("Archivo de configuración no encontrado.");

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            return config?["ApiKey"] ?? throw new Exception("API Key no encontrada.");
        }
    }
}
