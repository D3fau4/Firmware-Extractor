using System;
using System.IO;
using System.Text;
using LibHac;
using LibHac.Boot;
using LibHac.Fs;
using LibHac.FsSystem;

namespace Firmware_Extractor
{
    class PK21
    {
        public static void ProcessPk21(Keyset keyset, string path, string output)
        {
            using (var file = new CachedStorage(new LocalStorage(path, FileAccess.Read), 0x4000, 4, false))
            {
                var package2 = new Package2StorageReader();
                package2.Initialize(keyset, file).ThrowIfFailure();

                string INI1 = Path.Combine(output, "INI1");
                string pack2dir = Path.Combine(output, "Package2");

                Directory.CreateDirectory(pack2dir);
                /* Extract PKG2 */
                package2.OpenPayload(out IStorage kernelStorage, 0).ThrowIfFailure();
                kernelStorage.WriteAllBytes(Path.Combine(pack2dir, "Kernel.bin"));

                package2.OpenIni(out IStorage ini1Storage).ThrowIfFailure();
                ini1Storage.WriteAllBytes(Path.Combine(pack2dir, "INI1.bin"));

                package2.OpenDecryptedPackage(out IStorage decPackageStorage).ThrowIfFailure();
                decPackageStorage.WriteAllBytes(Path.Combine(pack2dir, "Decrypted.bin"));

                /* Extract INI1 */
                Directory.CreateDirectory(INI1);
                KIP1.ExtractIni1(ini1Storage, INI1);
            }
        }
    }
}
