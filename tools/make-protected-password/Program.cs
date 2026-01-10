using System;
using System.IO;
using Microsoft.AspNetCore.DataProtection;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            // Locate repository root relative to the compiled binary location
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var keyPath = Path.Combine(repoRoot, "GuestFlow.Api", "App_Data", "Keys");
            if (!Directory.Exists(keyPath))
            {
                Console.Error.WriteLine($"Key folder not found: {keyPath}");
                return 2;
            }

            var provider = DataProtectionProvider.Create(new DirectoryInfo(keyPath));
            var protector = provider.CreateProtector("GuestFlow-security-v1");
            var plain = args.Length > 0 ? args[0] : "Password123!";
            Console.WriteLine(protector.Protect(plain));
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
            return 1;
        }
    }
}
