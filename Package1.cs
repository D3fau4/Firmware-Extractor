using System.IO;
using System.Text;
using LibHac;
using LibHac.Boot;
using LibHac.Fs;
using LibHac.FsSystem;

namespace Firmware_Extractor
{
    class PK11
    {
        public static void ProcessPk11(Keyset keyset, string path, string output)
        {
            using (var file = new LocalStorage(path, FileAccess.Read))
            {
                var package1 = new Package1(keyset, file);
                string outDir = output;

                if (outDir != null)
                {
                    Directory.CreateDirectory(outDir);

                    package1.Pk11.OpenWarmboot().WriteAllBytes(Path.Combine(outDir, "Warmboot.bin"));
                    package1.Pk11.OpenNxBootloader().WriteAllBytes(Path.Combine(outDir, "NX_Bootloader.bin"));
                    package1.Pk11.OpenSecureMonitor().WriteAllBytes(Path.Combine(outDir, "Secure_Monitor.bin"));
                    package1.OpenDecryptedPackage().WriteAllBytes(Path.Combine(outDir, "Decrypted.bin"));
                }
            }
        }
    }
}
