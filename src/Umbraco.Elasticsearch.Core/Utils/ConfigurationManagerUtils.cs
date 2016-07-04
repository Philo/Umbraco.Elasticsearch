using System.Configuration;

namespace Umbraco.Elasticsearch.Core.Utils
{
    public static class ConfigurationManagerUtils
    {
        public static string FromAppSettings(this string key)
        {
            return GetAppSetting(key, null, null);
        }

        public static string FromConnectionStrings(this string key)
        {
            return GetConnectionString(key, null, null);
        }

        public static string FromAppSettingsWithPrefix(this string key, string prefix, string @default = null)
        {
            return GetAppSetting(key, @default, prefix);
        }

        public static string FromAppSettings(this string key, string @default)
        {
            return GetAppSetting(key, @default, null);
        }
        
        public static string FromConnectionStrings(this string key, string @default)
        {
            return GetConnectionString(key, @default, null);
        }

        public static string FromAppSettings(this string key, string @default, string prefix)
        {
            return GetAppSetting(key, @default, prefix);
        }

        public static string FromConnectionStrings(this string key, string @default, string prefix)
        {
            return GetConnectionString(key, @default, prefix);
        }

        public static string FromConnectionStringsWithPrefix(this string key, string prefix, string @default = null)
        {
            return GetConnectionString(key, @default, prefix);
        }

        public static string GetAppSetting(string key, string @default = null, string prefix = "")
        {
            var keyToRead = string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}{key}";
            var value = ConfigurationManager.AppSettings.Get(keyToRead);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (@default == null) throw new ConfigurationErrorsException(
                    $"Unable to read file system configuration from app settings section [{keyToRead}]");
                return @default;
            }
            return value;
        }

        public static string GetConnectionString(string key, string @default = null, string prefix = "")
        {
            var keyToRead = string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}{key}";
            var connectionStringValue = ConfigurationManager.ConnectionStrings[keyToRead];
            if (string.IsNullOrWhiteSpace(connectionStringValue?.ConnectionString))
            {
                if (@default == null) throw new ConfigurationErrorsException(
                    $"Unable to read file system configuration from connection strings section [{keyToRead}]");
                return @default;
            }
            return connectionStringValue.ConnectionString;
        }
    }
}