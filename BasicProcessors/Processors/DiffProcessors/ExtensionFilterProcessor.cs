﻿using System;
using System.Linq;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings.Attributes;

namespace BasicProcessors.Processors.DiffProcessors
{
    /// <summary>
    /// Filteres files by an extension or by an extension type/group.
    ///
    /// Extension source from http://www.fileinfo.com/
    ///
    /// If the extensions are corrupted, there also exists another way to check the format of a file
    /// via File Signature which is
    /// "generally a short sequence of bytes (most are 2-4 bytes long) placed at the beginning of the file"
    ///
    /// List of file signatures: http://www.filesignatures.net/index.php?page=all
    /// </summary>
    [Processor(ProcessorTypeEnum.Diff, 10, DiffModeEnum.TwoWay | DiffModeEnum.ThreeWay)]
    public class ExtensionFilterProcessor : ProcessorAbstract
    {
        public enum FileTypesEnum { All, Images, Text, Developer }

        [Settings("Filter files by extension category.", "extension-category")]
        public FileTypesEnum FileType = FileTypesEnum.All;

        readonly string[] imagesExtensions = { ".001", ".2bp", ".360", ".411", ".73i", ".8pbs", ".8xi", ".9.png", ".abm", ".accountpicture-ms", ".acorn", ".acr", ".adc", ".afx", ".agif", ".agp", ".aic", ".ais", ".albm", ".apd", ".apm", ".apng", ".aps", ".apx", ".arr", ".art", ".artwork", ".arw", ".asw", ".avatar", ".avb", ".awd", ".awd", ".blkrt", ".blz", ".bm2", ".bmc", ".bmf", ".bmp", ".bmx", ".bmz", ".brk", ".brn", ".brt", ".bss", ".bti", ".c4", ".cal", ".cals", ".cam", ".can", ".cd5", ".cdc", ".cdg", ".ce", ".cimg", ".cin", ".cit", ".colz", ".cpbitmap", ".cpc", ".cpd", ".cpg", ".cps", ".cpt", ".cpx", ".csf", ".ct", ".cut", ".dc2", ".dcm", ".dcx", ".ddb", ".dds", ".ddt", ".dgt", ".dib", ".dicom", ".djv", ".djvu", ".dm3", ".dmi", ".dpx", ".drz", ".dt2", ".dtw", ".dvl", ".ecw", ".epp", ".exr", ".fac", ".face", ".fal", ".fax", ".fbm", ".fil", ".fits", ".fpg", ".fpos", ".fpx", ".frm", ".g3", ".gbr", ".gcdp", ".gfb", ".gfie", ".ggr", ".gif", ".gih", ".gim", ".gmbck", ".gmspr", ".gp4", ".gpd", ".gro", ".grob", ".gry", ".hdp", ".hdr", ".hdrp", ".hf", ".hpi", ".hr", ".hrf", ".i3d", ".ic1", ".ic2", ".ic3", ".ica", ".icb", ".icn", ".icon", ".icpr", ".ilbm", ".img", ".imj", ".info", ".ink", ".int", ".iphotoproject", ".ipick", ".ipx", ".itc2", ".ithmb", ".ivr", ".ivue", ".iwi", ".j", ".j2c", ".j2k", ".jas", ".jb2", ".jbf", ".jbig", ".jbig2", ".jbmp", ".jbr", ".jfi", ".jfif", ".jia", ".jif", ".jiff", ".jng", ".jp2", ".jpc", ".jpd", ".jpe", ".jpeg", ".jpf", ".jpg", ".jpg2", ".jps", ".jpx", ".jtf", ".jwl", ".jxr", ".kdi", ".kdk", ".kfx", ".kic", ".kodak", ".kpg", ".lb", ".lbm", ".lif", ".ljp", ".lzp", ".mac", ".mat", ".max", ".mbm", ".mbm", ".mcs", ".met", ".mic", ".mip", ".mix", ".mng", ".mnr", ".mpf", ".mpo", ".mrb", ".mrxs", ".msk", ".msp", ".mxi", ".myl", ".ncd", ".ncr", ".nct", ".neo", ".NET", ".nlm", ".oc3", ".oc4", ".oc5", ".oci", ".odi", ".omf", ".oplc", ".ora", ".org", ".ota", ".otb", ".oti", ".ozb", ".ozj", ".ozt", ".pac", ".pal", ".pano", ".pap", ".pat", ".pbm", ".pc1", ".pc2", ".pc3", ".pcd", ".pcx", ".pdd", ".pdn", ".pe4", ".pe4", ".pfi", ".pfr", ".pgf", ".pgm", ".pi1", ".pi2", ".pi2", ".pi3", ".pi4", ".pi5", ".pi6", ".pic", ".pic", ".pic", ".picnc", ".pict", ".pictclipping", ".pix", ".pix", ".pixadex", ".pjpeg", ".pjpg", ".pm", ".pm3", ".pmg", ".png", ".pni", ".pnm", ".pns", ".pnt", ".pntg", ".pop", ".pov", ".pov", ".pp4", ".pp5", ".ppf", ".ppm", ".prw", ".psb", ".psd", ".psdx", ".pse", ".psf", ".psp", ".pspbrush", ".pspimage", ".ptg", ".ptk", ".pts", ".ptx", ".ptx", ".pvr", ".pwp", ".px", ".pxd", ".pxicon", ".pxm", ".pxr", ".pza", ".pzp", ".pzs", ".qif", ".qmg", ".qti", ".qtif", ".ras", ".rcl", ".rcu", ".rgb", ".rgb", ".rgf", ".ric", ".rif", ".riff", ".rix", ".rle", ".rli", ".rpf", ".rri", ".rs", ".rsb", ".rsr", ".rvg", ".s2mv", ".sai", ".sar", ".sbp", ".scg", ".sci", ".scn", ".scp", ".sct", ".scu", ".sdr", ".sep", ".sfc", ".sff", ".sfw", ".sgi", ".shg", ".sid", ".sig", ".sim", ".skitch", ".skm", ".skypeemoticonset", ".sld", ".smp", ".sob", ".spa", ".spc", ".spe", ".sph", ".spiff", ".spj", ".spp", ".spr", ".sprite", ".spu", ".sr", ".ste", ".sumo", ".sun", ".suniff", ".sup", ".sva", ".svm", ".t2b", ".taac", ".tb0", ".tbn", ".tex", ".tfc", ".tg4", ".tga", ".thm", ".thm", ".thumb", ".tif", ".tif", ".tiff", ".tjp", ".tm2", ".tn", ".tn1", ".tn2", ".tn3", ".tny", ".tpf", ".tpi", ".tps", ".trif", ".tub", ".u", ".ufo", ".uga", ".urt", ".usertile-ms", ".v", ".vda", ".vff", ".vic", ".viff", ".vna", ".vpe", ".vrphoto", ".vss", ".vst", ".wb1", ".wbc", ".wbd", ".wbm", ".wbmp", ".wbz", ".wdp", ".webp", ".wi", ".wic", ".wmp", ".wpb", ".wpe", ".wvl", ".xbm", ".xcf", ".xpm", ".xwd", ".y", ".ysp", ".yuv", ".zif" };

        readonly string[] textExtensions = { ".1st", ".abw", ".act", ".aim", ".ans", ".apt", ".asc", ".asc", ".ascii", ".ase", ".aty", ".awp", ".awt", ".aww", ".bad", ".bbs", ".bdp", ".bdr", ".bean", ".bib", ".bib", ".bml", ".bna", ".boc", ".brx", ".btd", ".bzabw", ".charset", ".chart", ".chord", ".cnm", ".cod", ".crwl", ".cws", ".cyi", ".dca", ".dgs", ".diz", ".dne", ".doc", ".doc", ".docm", ".docx", ".docxml", ".docz", ".dotm", ".dotx", ".dox", ".dropbox", ".dsc", ".dsv", ".dvi", ".dwd", ".dx", ".dxb", ".dxp", ".eio", ".eit", ".emf", ".eml", ".emlx", ".emulecollection", ".epp", ".err", ".err", ".etf", ".etx", ".euc", ".fadein", ".fadein.template", ".faq", ".fbl", ".fcf", ".fdf", ".fdr", ".fds", ".fdt", ".fdx", ".fdxt", ".fft", ".flr", ".fodt", ".fountain", ".fpt", ".frt", ".fwdn", ".gdoc", ".gmd", ".gpd", ".gpn", ".gsd", ".gthr", ".gv", ".hbk", ".hht", ".hs", ".hwp", ".hwp", ".hz", ".idx", ".iil", ".ipf", ".ipspot", ".jarvis", ".jis", ".joe", ".jp1", ".jrtf", ".kes", ".klg", ".klg", ".knt", ".kon", ".kwd", ".latex", ".lbt", ".lis", ".lnt", ".log", ".lp2", ".lst", ".lst", ".ltr", ".ltx", ".lue", ".luf", ".lwp", ".lxfml", ".lyt", ".lyx", ".man", ".mbox", ".mcw", ".md5.txt", ".me", ".mell", ".mellel", ".min", ".mnt", ".msg", ".mw", ".mwd", ".mwp", ".nb", ".ndoc", ".nfo", ".ngloss", ".njx", ".notes", ".now", ".nwctxt", ".nwm", ".nwp", ".ocr", ".odif", ".odm", ".odo", ".odt", ".ofl", ".openbsd", ".ort", ".ott", ".p7s", ".pages", ".pages-tef", ".pfs", ".pfx", ".pjt", ".plantuml", ".pmo", ".prt", ".prt", ".psw", ".pu", ".pvj", ".pvm", ".pwd", ".pwdp", ".pwdpl", ".pwi", ".pwr", ".qdl", ".qpf", ".rad", ".readme", ".rft", ".ris", ".rpt", ".rst", ".rtd", ".rtf", ".rtfd", ".rtx", ".run", ".rzk", ".rzn", ".saf", ".safetext", ".sam", ".sam", ".save", ".scc", ".scm", ".scriv", ".scrivx", ".sct", ".scw", ".sdm", ".sdoc", ".sdw", ".se", ".session", ".sgm", ".sig", ".skcard", ".sla", ".sla.gz", ".smf", ".sms", ".ssa", ".story", ".strings", ".stw", ".sty", ".sub", ".sublime-project", ".sublime-workspace", ".sxg", ".sxw", ".tab", ".tab", ".tdf", ".tdf", ".template", ".tex", ".text", ".textclipping", ".thp", ".tlb", ".tm", ".tmd", ".tmv", ".tpc", ".trelby", ".tvj", ".txt", ".u3i", ".unauth", ".unx", ".uof", ".uot", ".upd", ".utf8", ".utxt", ".vct", ".vnt", ".vw", ".wbk", ".webdoc", ".wn", ".wp", ".wp4", ".wp5", ".wp6", ".wp7", ".wpa", ".wpd", ".wpd", ".wpd", ".wpl", ".wps", ".wps", ".wpt", ".wpt", ".wpw", ".wri", ".wsd", ".wtx", ".xbdoc", ".xbplate", ".xdl", ".xdl", ".xwp", ".xwp", ".xwp", ".xy", ".xy3", ".xyp", ".xyw", ".zabw", ".zrtf", ".zw" };

        readonly string[] developerExtensions = { ".11", ".19", ".2clk", ".3dfbat", ".3rf", ".4ge", ".4gl", ".4pk", ".4th", ".8xk", ".a", ".a2w", ".a2x", ".a51", ".a5r", ".a66", ".a86", ".a8s", ".abap", ".abc", ".abl", ".abs", ".acgi", ".acm", ".acr", ".act", ".action", ".actionscript", ".actproj", ".actx", ".acu", ".ad", ".ad2", ".ada", ".adb", ".adiumscripts", ".ads", ".adt", ".aem", ".aep", ".aex", ".afb", ".agc", ".agi", ".agls", ".ago", ".ags", ".ahk", ".ahtml", ".aidl", ".airi", ".akp", ".akt", ".alb", ".alg", ".alm", ".alx", ".amf", ".aml", ".amos", ".amw", ".ane", ".anjuta", ".anm", ".ap", ".ap?", ".apb", ".apg", ".api_filters", ".aplt", ".app", ".appcache", ".applescript", ".applet", ".appxmanifest", ".appxsym", ".appxupload", ".aps", ".apt", ".arb", ".armx", ".arnoldc", ".aro", ".arq", ".art", ".artproj", ".arxml", ".ary", ".as", ".as3", ".as?", ".asax", ".asbx", ".asc", ".ascx", ".asf", ".ash", ".asi", ".asic", ".asm", ".asmx", ".aso", ".asp", ".asp+", ".asproj", ".aspx", ".asr", ".ass", ".astx", ".asz", ".atl", ".atm", ".atmn", ".atomsvc", ".atp", ".ats", ".au3", ".au?", ".aut", ".ave", ".avs", ".avsi", ".awd", ".awk", ".awl", ".axb", ".axd", ".axe", ".axs", ".b", ".b24", ".b2d", ".bal", ".bas", ".bash", ".bat", ".bb", ".bbc", ".bbf", ".bcc", ".bcf", ".bcp", ".bdh", ".bdt", ".beam", ".bet", ".bgm", ".bhs", ".bi", ".bil", ".bin", ".bks", ".bli", ".bml", ".bmo", ".bms", ".boo", ".borland", ".box", ".bp", ".bpk", ".bpo", ".bpr", ".bps", ".bpt", ".brml", ".brs", ".brx", ".bs2", ".bsc", ".bsh", ".bsm", ".bsml", ".bsv", ".bufferedimage", ".build", ".builder", ".buildpath", ".bur", ".bxb", ".bxl", ".bxml", ".bxp", ".bzs", ".c", ".c#", ".c++", ".c--", ".c86", ".c__", ".cal", ".cap", ".capfile", ".car", ".cas", ".cb", ".cba", ".cbl", ".cbp", ".cbq", ".cbs", ".cc", ".ccbjs", ".ccs", ".ccxml", ".cd", ".cel", ".cfi", ".cfo", ".cfs", ".cg", ".cgi", ".cgvp", ".cgx", ".ch", ".chd", ".chh", ".cl", ".cla", ".class", ".classpath", ".clips", ".clj", ".clm", ".clp", ".cls", ".clu", ".clw", ".cma", ".cmake", ".cmd", ".cml", ".cmp", ".cms", ".cnt", ".cob", ".cobol", ".cod", ".codasite", ".coffee", ".cola", ".common", ".con", ".configure", ".confluence", ".cos", ".coverage", ".coveragexml", ".cp", ".cp?", ".cpb", ".cpp", ".cpr", ".cprr", ".cpy", ".cpz", ".cr", ".creole", ".cs", ".csb", ".csc", ".csf", ".csgrad", ".csh", ".cshrc", ".cshtml", ".csm", ".csml", ".csp", ".csproj", ".csview", ".csx", ".ctl", ".ctp", ".cus", ".cvsrc", ".cx", ".cxe", ".cxs", ".cxt", ".cxx", ".cya", ".d", ".d2j", ".d4", ".dadx", ".daemonscript", ".das", ".datasource", ".db", ".db2", ".db2tbl", ".db2tr", ".db2vw", ".dba", ".dbg", ".dbheader", ".dbml", ".dbo", ".dbp", ".dbpro", ".dbproj", ".dc", ".dcf", ".dcr", ".dd", ".ddb", ".ddp", ".deb", ".defi", ".dep", ".depend", ".derp", ".des", ".dev", ".devpak", ".dfb", ".dfd", ".dfm", ".dfn", ".dg", ".dgml", ".dgsl", ".dht", ".dhtml", ".di", ".dia", ".dic", ".dif", ".diff", ".dil", ".djg", ".dlg", ".dmb", ".dmc", ".dml", ".dms", ".do", ".dob", ".docstates", ".dor", ".dot", ".dpd", ".dpj", ".dpk", ".dplt", ".dpq", ".dpr", ".dqy", ".drc", ".dro", ".ds", ".dsb", ".dsd", ".dso", ".dsp", ".dsq", ".dsr", ".dsym", ".dt", ".dtd", ".dtml", ".dto", ".dts", ".dtx", ".dvb", ".dwarf", ".dwp", ".dws", ".dwt", ".dxl", ".e", ".eaf", ".ebc", ".ebm", ".ebs", ".ebs2", ".ebuild", ".ebx", ".ecore", ".ecorediag", ".edml", ".eek", ".egg", ".egg-info", ".ejs", ".ekm", ".el", ".elc", ".emakefile", ".emakerfile", ".enml", ".entitlements", ".ephtml", ".epj", ".epl", ".epp", ".ept", ".eql", ".eqn", ".erb", ".erl", ".erubis", ".es", ".ev3p", ".ex", ".exc", ".exe", ".exp", ".exsd", ".exu", ".exw", ".eze", ".f", ".f40", ".f77", ".f90", ".f95", ".factorypath", ".fas", ".fasl", ".fbz6", ".fcg", ".fcgi", ".fdml", ".fdt", ".ff", ".fgb", ".fgl", ".fil", ".flm", ".fmb", ".fmt", ".for", ".form", ".forth", ".fountain", ".fp", ".fpc", ".fpi", ".fpp", ".frj", ".frs", ".frt", ".fs", ".fsi", ".fsproj", ".fsscript", ".fsx", ".ftn", ".fus", ".fwx", ".fxcproj", ".fxl", ".fxml", ".g1m", ".gadgeprj", ".galaxy", ".gas", ".gbap", ".gbl", ".gc3", ".gch", ".gcl", ".gcode", ".gemfile", ".generictest", ".genmodel", ".gfa", ".gfe", ".ghc", ".git", ".gitignore", ".glade", ".gld", ".glf", ".gls", ".gml", ".gnt", ".gnumakefile", ".go", ".gobj", ".goc", ".gp", ".graphml", ".graphmlz", ".greenfoot", ".groovy", ".grxml", ".gs", ".gsb", ".gsc", ".gss", ".gst", ".gsym", ".gus", ".gv", ".gvy", ".gxl", ".gyp", ".gypi", ".h", ".h++", ".h--", ".h2o", ".h6h", ".h86", ".h__", ".hal", ".haml", ".has", ".hay", ".hbm", ".hbs", ".hbx", ".hbz", ".hc", ".hcw", ".hdf", ".hdl", ".hh", ".hhh", ".hic", ".hid", ".history", ".hkp", ".hla", ".hlsl", ".hms", ".hoic", ".hom", ".hp?", ".hpf", ".hpp", ".hrh", ".hrl", ".hs", ".hsc", ".hsdl", ".hse", ".hsm", ".ht4", ".htc", ".htd", ".htm", ".html5", ".htr", ".hx", ".hxa", ".hxml", ".hxp", ".hxproj", ".hxx", ".hydra", ".i", ".iap", ".ic", ".ice", ".icl", ".icn", ".idb", ".ide", ".idl", ".ifp", ".ig", ".ii", ".ijs", ".ik", ".il", ".ilk", ".image", ".iml", ".imp", ".inc", ".ino", ".inp", ".ins", ".io", ".ipb", ".ipch", ".ipf", ".ipp", ".ipproj", ".ipr", ".ips", ".iqy", ".irbrc", ".irc", ".irev", ".irobo", ".is", ".isa", ".ism", ".iss", ".isu", ".isym", ".itcl", ".ixx", ".j", ".jacl", ".jad", ".jade", ".jardesc", ".jav", ".java", ".javajet", ".jcl", ".jcm", ".jcs", ".jdp", ".jetinc", ".jgs", ".jks", ".jl", ".jlc", ".jml", ".jomproj", ".jpage", ".jpd", ".js", ".jsa", ".jsb", ".jsc", ".jse", ".jsf", ".jsfl", ".jsh", ".jsm", ".json", ".jsonp", ".jsp", ".jss", ".jsx", ".jsxinc", ".jtb", ".judo", ".kb", ".kbs", ".kcl", ".kdevprj", ".ked", ".kex", ".kix", ".kmdi", ".kml", ".kmt", ".komodo", ".kon", ".kpl", ".ksc", ".ksh", ".kst", ".kumac", ".l", ".l1i", ".lamp", ".lap", ".lasso", ".lay", ".lbi", ".lds", ".ldz", ".less", ".lex", ".lgt", ".lhs", ".li$", ".lib", ".licx", ".liquid", ".lisp", ".lit", ".ll", ".lml", ".lmp", ".lmv", ".lng", ".lnk", ".lnp", ".lnx", ".lo", ".loc", ".login", ".lol", ".lols", ".lp", ".lpr", ".lpx", ".lrf", ".lrs", ".ls1", ".lsp", ".lss", ".lst", ".lua", ".luac", ".luca", ".lwa", ".lwmw", ".lxk", ".lxsproj", ".m", ".m2", ".m2r", ".m3", ".m4", ".m4x", ".m51", ".m6m", ".mab", ".mac", ".magik", ".mak", ".make", ".maki", ".mako", ".maml", ".map", ".mash", ".master", ".mat", ".matlab", ".mb", ".mbas", ".mbs", ".mbtemmplate", ".mc", ".mca", ".mcml", ".mcp", ".mcr", ".md", ".mdex", ".mdf", ".mdp", ".mec", ".mediawiki", ".mel", ".mem", ".mex", ".mf", ".mfa", ".mfcribbon-ms", ".mfl", ".mfps", ".mg", ".mhl", ".mi", ".mingw", ".mingw32", ".mix", ".mjk", ".mk", ".mkb", ".mke", ".ml", ".mli", ".mln", ".mls", ".mlsxml", ".mly", ".mm", ".mmb", ".mmch", ".mmh", ".mmjs", ".mml", ".mnd", ".mo", ".moc", ".mod", ".mp?", ".mpd", ".mpkt", ".mpm", ".mpx", ".mqt", ".mrc", ".mrd", ".mrl", ".mrs", ".ms", ".msc", ".mscr", ".msct", ".msdev", ".msdl", ".msha", ".msil", ".msl", ".msm", ".msp", ".mss", ".mst", ".msvc", ".msym", ".mtp", ".mtx", ".mvc", ".mw", ".mwp", ".mx", ".mxe", ".myapp", ".mzp", ".nas", ".nbin", ".nbk", ".ncb", ".ncx", ".neko", ".nes", ".netboot", ".nhs", ".nk", ".nlc", ".nmk", ".nms", ".nokogiri", ".npi", ".nqc", ".nrs", ".nse", ".nsi", ".nt", ".nupkg", ".nvi", ".nxc", ".o", ".obj", ".obr", ".ocamlmakefile", ".ocb", ".ocr", ".odc", ".odh", ".odl", ".ods", ".ogl", ".ogr", ".ogs", ".ogx", ".okm", ".oks", ".ook", ".oplm", ".opt", ".opv", ".opx", ".oqy", ".orc", ".orl", ".osas", ".osax", ".osg", ".ow", ".owd", ".owl", ".ox", ".p", ".pag", ".par", ".param", ".pas", ".pb", ".pba", ".pbi", ".pbl", ".pbp", ".pbq", ".pbx5script", ".pbxbtree", ".pbxproj", ".pbxscript", ".pc", ".pcd", ".pch", ".pcs", ".pd", ".pdb", ".pde", ".pdl", ".pdml", ".pdo", ".pds", ".pem", ".perl", ".pf0", ".pf1", ".pf2", ".pf4", ".pf?", ".pfa", ".pfx", ".pgm", ".pgml", ".ph", ".phl", ".php", ".php1", ".php2", ".php3", ".php4", ".php5", ".phps", ".phpt", ".phs", ".phtml", ".pickle", ".pika", ".pike", ".pjt", ".pjx", ".pkb", ".pkh", ".pl", ".pl1", ".pl7", ".playground", ".plc", ".pli", ".pls", ".plx", ".pm", ".pmod", ".pmp", ".pnproj", ".pnpt", ".poc", ".policy", ".pom", ".pou", ".pp", ".ppa", ".ppam", ".ppml", ".ppo", ".prg", ".pri", ".prl", ".prm", ".pro", ".propertiesjet", ".proto", ".prx", ".psc1", ".psc2", ".psf", ".psl", ".psm1", ".psml", ".pspscript", ".psu", ".ptl", ".ptx", ".ptxml", ".pun", ".pvs", ".pwn", ".pwo", ".pxd", ".pxi", ".pxl", ".pxml", ".pxt", ".py", ".pyc", ".pym", ".pyo", ".pyw", ".pyx", ".qac", ".qcf", ".qdl", ".qlc", ".qpr", ".qry", ".qsc", ".qx", ".qxm", ".r", ".radius", ".rake", ".rakefile", ".rapc", ".rb", ".rbc", ".rbf", ".rbp", ".rbs", ".rbt", ".rbvcp", ".rbw", ".rbx", ".rc", ".rc2", ".rc3", ".rcc", ".rdf", ".rdoc", ".rdoff", ".re", ".reb", ".rej", ".res", ".resjson", ".resources", ".resx", ".rex", ".rexx", ".rfs", ".rfx", ".rgs", ".rguninst", ".rh", ".rhtml", ".rip", ".rkt", ".rml", ".rmn", ".rng", ".rnw", ".rob", ".robo", ".rpg", ".rpj", ".rprofile", ".rptproj", ".rpy", ".rpyc", ".rpym", ".rqy", ".rrc", ".rrh", ".rsl", ".rsm", ".rsp", ".rss", ".rssc", ".rsym", ".rts", ".rub", ".rule", ".rvb", ".rvt", ".rws", ".rxs", ".s", ".s2s", ".s43", ".s4e", ".s5d", ".saas", ".sal", ".sami", ".sar", ".sas", ".sass", ".sax", ".sb", ".sbh", ".sbi", ".sbml", ".sbr", ".sbs", ".sc", ".sca", ".scala", ".scar", ".scb", ".scm", ".sconstruct", ".scp", ".scpt", ".scptd", ".scr", ".script", ".scriptsuite", ".scriptterminology", ".scro", ".scs", ".scss", ".sct", ".scx", ".scz", ".sda", ".sdef", ".sdi", ".sdl", ".seestyle", ".seman", ".sf", ".sfl", ".sfm", ".sfp", ".sfr", ".sfx", ".sh", ".shit", ".sid", ".sim", ".simple", ".sit", ".sjava", ".sjc", ".sjs", ".skp", ".sl", ".slackbuild", ".slim", ".sln", ".slogo", ".slogt", ".slt", ".sltng", ".sm", ".sma", ".smali", ".smd", ".smi", ".sml", ".smm", ".smw", ".smx", ".snapx", ".snippet", ".sno", ".sp?", ".spk", ".spr", ".sps", ".spt", ".spx", ".sql", ".sqldataprovider", ".sqljet", ".sqlproj", ".src", ".srz", ".ss", ".ssc", ".ssi", ".ssml", ".ssq", ".st", ".stl", ".stm", ".sts", ".styl", ".sus", ".svc", ".svn-base", ".svo", ".svx", ".sw", ".swg", ".swift", ".swt", ".sxs", ".sxt", ".sxv", ".sym", ".syp", ".t", ".tab", ".tag", ".tal", ".targets", ".tcl", ".tcsh", ".tcz", ".tdo", ".tea", ".tec", ".tem", ".texinfo", ".text", ".textile", ".tgml", ".thor", ".thtml", ".ti", ".tig", ".tik", ".tikz", ".tilemap", ".tiprogram", ".tk", ".tkp", ".tla", ".tlc", ".tld", ".tlh", ".tli", ".tmh", ".tmo", ".tokend", ".tpl", ".tpm", ".tpr", ".tps", ".tpt", ".tpx", ".tql", ".tra", ".tracwiki", ".transcriptstyle", ".triple-s", ".trs", ".trt", ".tru", ".ts0", ".tsc", ".tsq", ".tst", ".ttcn", ".tu", ".tur", ".turboc3", ".twig", ".txc", ".txl", ".txml", ".txx", ".udf", ".ufdl", ".uih", ".uit", ".uix", ".ulp", ".umlclass", ".ump", ".unx", ".usi", ".uvproj", ".v", ".v3s", ".v4e", ".v4s", ".vad", ".vala", ".vap", ".vb", ".vba", ".vbe", ".vbg", ".vbi", ".vbp", ".vbproj", ".vbs", ".vbscript", ".vbw", ".vbx", ".vc", ".vc1", ".vc15", ".vc2", ".vc4", ".vc5", ".vc6", ".vc7", ".vce", ".vcp", ".vcproj", ".vcwin32", ".vcxproj", ".vd", ".vddproj", ".vdm", ".vdp", ".vdproj", ".vgc", ".vic", ".vim", ".vip", ".viw", ".vjp", ".vls", ".vlx", ".vmx", ".vpc", ".vpi", ".vps", ".vrm", ".vrp", ".vsmacros", ".vspolicy", ".vsprops", ".vssscc", ".vstemplate", ".vtm", ".vtml", ".vup", ".vxml", ".w", ".wam", ".was", ".wax", ".wbc", ".wbf", ".wbt", ".wch", ".wcm", ".wdi", ".wdk", ".wdl", ".wdproj", ".wdw", ".wdx9", ".wfs", ".wiki", ".win", ".win32manifest", ".wis", ".wix", ".wixout", ".wmc", ".wml", ".wmlc", ".wmls", ".wmlsc", ".wmw", ".woa", ".wod", ".wowproj", ".wpj", ".wpk", ".wpm", ".ws", ".wsc", ".wsd", ".wsdd", ".wsdl", ".wsf", ".wsh", ".wsil", ".wsp", ".wsym", ".wx", ".wxi", ".wxl", ".wxs", ".wzs", ".x", ".xaml", ".xamlx", ".xap", ".xbap", ".xbc", ".xbd", ".xbl", ".xblr", ".xcl", ".xcodeproj", ".xcp", ".xda", ".xds", ".xfm", ".xhtm", ".xib", ".xig", ".xin", ".xjb", ".xl", ".xla", ".xlm", ".xlm3", ".xlm4", ".xlm_", ".xlv", ".xmap", ".xme", ".xml", ".xmla", ".xmljet", ".xmta", ".xn", ".xnf", ".xojo_binary_project", ".xojo_project", ".xoml", ".xpb", ".xpgt", ".xr", ".xrc", ".xsc", ".xsd", ".xsl", ".xslt", ".xsql", ".xtx", ".xtxt", ".xui", ".xul", ".xys", ".y", ".yab", ".yajl", ".yml2", ".ywl", ".yxx", ".z", ".zasm", ".zbi", ".zcls", ".zero", ".zfd", ".zfrm", ".zfs", ".zh_tw", ".zms", ".zpd", ".zpk", ".zpl", ".zsc", ".zsh", ".zsrc", ".zts", ".zws" };

        [Settings("Exclude files by file extension.", "exclude-extension", "ee")]
        public string[] ExcludeFilter = null;

        [Settings("Include files by file extension.", "include-extension", "ie")]
        public string[] IncludeFilter = null;

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            // default settings -- all pass
            if (FileType == FileTypesEnum.All && (ExcludeFilter == null || ExcludeFilter.Length == 0)
                && (IncludeFilter == null || IncludeFilter.Length == 0))
                return;

            string ext = node.Info.Extension.ToLowerInvariant();

            // some settings were specified and this files does not have extension => exclude it
            if (string.IsNullOrEmpty(ext))
            {
                node.Status = NodeStatusEnum.IsIgnored;
                return;
            }

            // filter by mode
            if (FileType == FileTypesEnum.Images && Array.BinarySearch(imagesExtensions, ext) < 0)
            {
                node.Status = NodeStatusEnum.IsIgnored;
            } else if (FileType == FileTypesEnum.Text && Array.BinarySearch(textExtensions, ext) < 0)
            {
                node.Status = NodeStatusEnum.IsIgnored;
            } else if (FileType == FileTypesEnum.Developer && Array.BinarySearch(developerExtensions, ext) < 0)
            {
                node.Status = NodeStatusEnum.IsIgnored;
            }

            // filter by extensions

            if (ExcludeFilter != null && ExcludeFilter.Contains(ext))
            {
                node.Status = NodeStatusEnum.IsIgnored;
            }

            if (IncludeFilter != null && !IncludeFilter.Contains(ext))
            {
                node.Status = NodeStatusEnum.IsIgnored;
            }

        }
    }
}
