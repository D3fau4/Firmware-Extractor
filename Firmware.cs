using System;
using System.Linq;
using System.IO;
using LibHac;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;

namespace Firmware_Extractor
{
    class Firmware
    {
        public static void ProccesFw(string path, string keypath)
        {
            Keyset keyset = ExternalKeyReader.ReadKeyFile(keypath);
            SwitchFs switchFs;
            var baseFs = new LocalFileSystem(path);
            switchFs = SwitchFs.OpenNcaDirectory(keyset, baseFs);
            foreach (SwitchFsNca nca in switchFs.Ncas.Values.OrderBy(x => x.Nca.Header.TitleId))
            {
                if (NCA.GetName(nca.Nca) != null)
                {
                    if (nca.Nca.Header.ContentType == NcaContentType.Meta)
                    {
                        NCA.ExtractNca(keyset, Path.Combine(path, nca.Filename), Path.Combine("OutFirm", "Meta", nca.Nca.Header.TitleId.ToString("X16")  + " - " + NCA.GetName(nca.Nca)));
                    }
                    else
                    {
                        NCA.ExtractNca(keyset, Path.Combine(path, nca.Filename), Path.Combine("OutFirm", "Program", nca.Nca.Header.TitleId.ToString("X16") + " - " + NCA.GetName(nca.Nca)));
                    }
                }
                else
                {
                    if (nca.Nca.Header.ContentType == NcaContentType.Meta)
                    {
                        NCA.ExtractNca(keyset, Path.Combine(path, nca.Filename), Path.Combine("OutFirm", "Meta", nca.Nca.Header.TitleId.ToString("X16")));
                    }
                    else
                    {
                        NCA.ExtractNca(keyset, Path.Combine(path, nca.Filename), Path.Combine("OutFirm", "Program", nca.Nca.Header.TitleId.ToString("X16")));
                    }
                }
            }
        }
    }
}