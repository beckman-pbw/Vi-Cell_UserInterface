namespace ScoutLanguageResources
{
    public class ResourceHelper
    {
        public string this[string key] => LanguageResourceHelper.ResourceManager.GetString(key);
    }
}