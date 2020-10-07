using System.IO;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;

namespace Firmware_Extractor
{
    class NCA
    {
        private class NcaHolder
        {
            public Nca Nca;
            public Validity[] Validities = new Validity[4];
        }
        public static void ExtractNca(Keyset keyset, string pathnca, string outdir)
        {
            using (IStorage file = new LocalStorage(pathnca, FileAccess.Read))
            {
                var ctx = new Context();
                var nca = new Nca(keyset, file);
                Nca baseNca = null;

                var ncaHolder = new NcaHolder { Nca = nca };

                // extract RomFS
                if (nca.SectionExists(NcaSectionType.Data))
                {
                    FileSystemClient fs = new FileSystemClient(new StopWatchTimeSpanGenerator());

                    fs.Register("rom".ToU8Span(), OpenFileSystemByType(NcaSectionType.Data));
                    fs.Register("output".ToU8Span(), new LocalFileSystem(Path.Combine(outdir,"Rom")));

                    FsUtils.CopyDirectoryWithProgress(fs, "rom:/".ToU8Span(), "output:/".ToU8Span(), logger: ctx.Logger).ThrowIfFailure();

                    fs.Unmount("rom".ToU8Span());
                    fs.Unmount("output".ToU8Span());
                }
                // Extract Exefs
                if (nca.SectionExists(NcaSectionType.Code))
                {
                    FileSystemClient fs = new FileSystemClient(new StopWatchTimeSpanGenerator());

                    fs.Register("code".ToU8Span(), OpenFileSystemByType(NcaSectionType.Code));
                    fs.Register("output".ToU8Span(), new LocalFileSystem(Path.Combine(outdir,"Code")));

                    FsUtils.CopyDirectoryWithProgress(fs, "code:/".ToU8Span(), "output:/".ToU8Span()).ThrowIfFailure();
                    // Unmount 
                    fs.Unmount("code".ToU8Span());
                    fs.Unmount("output".ToU8Span());
                }

                IFileSystem OpenFileSystem(int index)
                {
                    if (baseNca != null) return baseNca.OpenFileSystemWithPatch(nca, index, 0);

                    return nca.OpenFileSystem(index, 0);
                }

                IFileSystem OpenFileSystemByType(NcaSectionType type)
                {
                    return OpenFileSystem(Nca.GetSectionIndexFromType(type, nca.Header.ContentType));
                }
            }
        }
    }
}
