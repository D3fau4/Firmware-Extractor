using System.IO;
using System.Text;
using LibHac.Common.Keys;
using LibHac.Boot;
using LibHac.Fs;
using LibHac.FsSystem;
using System.Runtime.CompilerServices;

namespace Firmware_Extractor
{
    class PK11
    {
        public static void ProcessPk11(KeySet keyset, string path, string output)
        {
            using (var file = new LocalStorage(path, FileAccess.Read))
            {
                var package1 = new Package1();
                package1.Initialize(keyset, file);
                string outDir = output;

                if (outDir != null)
                {
                    Directory.CreateDirectory(outDir);

                    IStorage decryptedStorage = package1.OpenDecryptedPackage1Storage();

                    WriteFile(decryptedStorage, "Decrypted.bin");
                    WriteFile(package1.OpenWarmBootStorage(), "Warmboot.bin");
                    WriteFile(package1.OpenNxBootloaderStorage(), "NX_Bootloader.bin");
                    WriteFile(package1.OpenSecureMonitorStorage(), "Secure_Monitor.bin");

                    if (package1.IsMariko)
                    {
                        WriteFile(package1.OpenDecryptedWarmBootStorage(), "Warmboot_Decrypted.bin");

                        var marikoOemLoader = new SubStorage(decryptedStorage, Unsafe.SizeOf<Package1MarikoOemHeader>(),
                            package1.MarikoOemHeader.Size);

                        WriteFile(marikoOemLoader, "Mariko_OEM_Bootloader.bin");
                    }
                }

                void WriteFile(IStorage storage, string filename)
                {
                    string path = Path.Combine(outDir, filename);
                }
            }
        }
    }
}
