namespace QRHub.Api.Extensions;

public static class EnvLoader
{
    public static void Load()
    {
        // Search directories: current directory, base directory, and their parent directories
        var candidateDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            candidateDirs.Add(current.FullName);
            current = current.Parent;
        }

        // Priority order: .env first, then .env.local (which overrides .env)
        var envFiles = new[] { ".env", ".env.local" };

        foreach (var file in envFiles)
        {
            foreach (var dir in candidateDirs)
            {
                var filePath = Path.Combine(dir, file);
                if (File.Exists(filePath))
                {
                    LoadFile(filePath);
                    break;
                }
            }
        }
    }

    private static void LoadFile(string filePath)
    {
        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
            {
                continue;
            }

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = trimmed[..separatorIndex].Trim();
            var val = trimmed[(separatorIndex + 1)..].Trim();

            // Strip surrounding quotes
            if ((val.StartsWith('"') && val.EndsWith('"')) || (val.StartsWith('\'') && val.EndsWith('\'')))
            {
                val = val[1..^1];
            }

            Environment.SetEnvironmentVariable(key, val);

            // Shorthand mapping for ASP.NET Core configuration binding
            switch (key)
            {
                case "JWT_SECRET":
                    Environment.SetEnvironmentVariable("JwtSettings__Secret", val);
                    break;
                case "JWT_ISSUER":
                    Environment.SetEnvironmentVariable("JwtSettings__Issuer", val);
                    break;
                case "JWT_AUDIENCE":
                    Environment.SetEnvironmentVariable("JwtSettings__Audience", val);
                    break;
                case "JWT_EXPIRY_MINUTES":
                    Environment.SetEnvironmentVariable("JwtSettings__ExpiryMinutes", val);
                    break;
            }
        }
    }
}
