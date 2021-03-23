using System;
using System.IO;
using LibHac;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Npdm;

namespace Firmware_Extractor
{
    class NCA
    {
        private class NcaHolder
        {
            public Nca Nca;
            public Validity[] Validities = new Validity[4];
        }

        public static string GetName(Nca nca)
        {
            if (nca.CanOpenSection(NcaSectionType.Code))
            {
                IFileSystem fs = nca.OpenFileSystem(NcaSectionType.Code, IntegrityCheckLevel.None);
                Result r = fs.OpenFile(out IFile file, "/main.npdm".ToU8String(), OpenMode.Read);
                if (r.IsSuccess())
                {
                    var npdm = new NpdmBinary(file.AsStream(), null);
                    return npdm.TitleName;
                }
            }
            return null;
        }
        public static void ExtractNca(KeySet keyset, string pathnca, string outdir)
        {
            try
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
                        fs.Register("output".ToU8Span(), new LocalFileSystem(Path.Combine(outdir, "Rom")));

                        FsUtils.CopyDirectoryWithProgress(fs, "rom:/".ToU8Span(), "output:/".ToU8Span(), logger: ctx.Logger).ThrowIfFailure();

                        fs.Unmount("rom".ToU8Span());
                        fs.Unmount("output".ToU8Span());
                    }
                    // Extract Exefs
                    if (nca.SectionExists(NcaSectionType.Code))
                    {
                        FileSystemClient fs = new FileSystemClient(new StopWatchTimeSpanGenerator());

                        fs.Register("code".ToU8Span(), OpenFileSystemByType(NcaSectionType.Code));
                        fs.Register("output".ToU8Span(), new LocalFileSystem(Path.Combine(outdir, "Code")));

                        FsUtils.CopyDirectoryWithProgress(fs, "code:/".ToU8Span(), "output:/".ToU8Span()).ThrowIfFailure();
                        // Unmount 
                        fs.Unmount("code".ToU8Span());
                        fs.Unmount("output".ToU8Span());
                    }
                    Console.WriteLine(nca.Header.TitleId.ToString("X16"));
                    // extract PKGs
                    if (nca.Header.ContentType != NcaContentType.Meta)
                    {
                        if (nca.Header.TitleId.ToString("X16") == "0100000000000819" || nca.Header.TitleId.ToString() == "010000000000081A" || nca.Header.TitleId.ToString() == "010000000000081B" || nca.Header.TitleId.ToString() == "010000000000081C")
                        {
                            /* Extract Package1*/
                            if (Directory.Exists(Path.Combine(outdir, "Rom", "a")))
                            {
                                try
                                {
                                    PK11.ProcessPk11(keyset, Path.Combine(outdir, "Rom", "a", "package1"), Path.Combine(outdir, "Package1", "Mariko"));
                                    PK11.ProcessPk11(keyset, Path.Combine(outdir, "Rom", "nx", "package1"), Path.Combine(outdir, "Package1", "Erista"));
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("");
                                }
                            }
                            else
                            {
                                try
                                {
                                    PK11.ProcessPk11(keyset, Path.Combine(outdir, "Rom", "nx", "package1"), Path.Combine(outdir, "Package1", "Erista"));
                                }
                                catch(Exception e)
                                {
                                    Console.WriteLine("");
                                }
                            }

                            /* Extract Package2 */
                            PK21.ProcessPk21(keyset, Path.Combine(outdir, "Rom", "nx", "package2"), Path.Combine(outdir, "Package2"));
                        }
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
            catch (Exception e)
            {
                pathnca = pathnca.Replace(".cnmt.nca", ".nca");
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
                        fs.Register("output".ToU8Span(), new LocalFileSystem(Path.Combine(outdir, "Rom")));

                        FsUtils.CopyDirectoryWithProgress(fs, "rom:/".ToU8Span(), "output:/".ToU8Span(), logger: ctx.Logger).ThrowIfFailure();

                        fs.Unmount("rom".ToU8Span());
                        fs.Unmount("output".ToU8Span());
                    }
                    // Extract Exefs
                    if (nca.SectionExists(NcaSectionType.Code))
                    {
                        FileSystemClient fs = new FileSystemClient(new StopWatchTimeSpanGenerator());

                        fs.Register("code".ToU8Span(), OpenFileSystemByType(NcaSectionType.Code));
                        fs.Register("output".ToU8Span(), new LocalFileSystem(Path.Combine(outdir, "Code")));

                        FsUtils.CopyDirectoryWithProgress(fs, "code:/".ToU8Span(), "output:/".ToU8Span()).ThrowIfFailure();
                        // Unmount 
                        fs.Unmount("code".ToU8Span());
                        fs.Unmount("output".ToU8Span());
                    }
                    Console.WriteLine(nca.Header.TitleId.ToString("X16"));
                    // extract PKGs
                    if (nca.Header.ContentType != NcaContentType.Meta)
                    {
                        if (nca.Header.TitleId.ToString("X16") == "0100000000000819" || nca.Header.TitleId.ToString() == "010000000000081A" || nca.Header.TitleId.ToString() == "010000000000081B" || nca.Header.TitleId.ToString() == "010000000000081C")
                        {
                            /* Extract Package1*/
                            if (Directory.Exists(Path.Combine(outdir, "Rom", "a")))
                            {
                                //PK11.ProcessPk11(keyset, Path.Combine(outdir, "Rom", "a", "package1"), Path.Combine(outdir, "Package1", "Mariko"));
                                //PK11.ProcessPk11(keyset, Path.Combine(outdir, "Rom", "nx", "package1"), Path.Combine(outdir, "Package1", "Erista"));
                            }
                            else
                            {
                                //PK11.ProcessPk11(keyset, Path.Combine(outdir, "Rom", "nx", "package1"), Path.Combine(outdir, "Package1", "Erista"));
                            }

                            /* Extract Package2 */
                            PK21.ProcessPk21(keyset, Path.Combine(outdir, "Rom", "nx", "package2"), Path.Combine(outdir, "Package2"));
                        }
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
}
