namespace UAlbion
open System.IO
open FSharp.Data

type ConfigProvider = JsonProvider<"""
{
    "base_xld_path": "C:\\Games\\albion\\cd\\XLD",
    "xlds": [
        { 
            "name": "C:\\Games\\albion\\cd\\XLD\\3DBCKGR0.XLD",
            "objects": {
                "0": {
                    "type": "texture",
                    "name": "First image",
                    "width": 32
                }
            }
        },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DFLOOR0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DFLOOR1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DFLOOR2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DOBJEC0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DOBJEC1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DOBJEC2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DOBJEC3.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DOVERL0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DOVERL1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DOVERL2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DWALLS0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\3DWALLS1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\AUTOGFX0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\BLKLIST0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\COMBACK0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\COMGFX0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\EVNTSET0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\EVNTSET1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\EVNTSET2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\EVNTSET3.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\EVNTSET9.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\FBODPIX0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\FONTS0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ICONDAT0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ICONGFX0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ITEMGFX" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ITEMLIST.DAT" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ITEMNAME.DAT" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\LABDATA0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\LABDATA1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\LABDATA2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\MAPDATA1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\MAPDATA2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\MAPDATA3.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\MONCHAR0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\MONGFX0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\MONGRP0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\MONGRP1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\MONGRP2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\NPCGR0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\NPCGR1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\NPCKL0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\PALETTE.000" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\PALETTE0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\PARTGR0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\PARTKL0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\PICTURE0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SAMPLES0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SAMPLES1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SAMPLES2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SCRIPT0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SCRIPT2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SLAB" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SMLPORT0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SMLPORT1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SONGS0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\SPELLDAT.DAT" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\TACTICO0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\TRANSTB0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\WAVELIB0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\EVNTTXT0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\EVNTTXT1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\EVNTTXT2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\EVNTTXT3.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\EVNTTXT9.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\FLICS0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\MAPTEXT1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\MAPTEXT2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\MAPTEXT3.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\SYSTEXTS" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\ENGLISH\\WORDLIS0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\AUTOMAP1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\AUTOMAP2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\AUTOMAP3.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\CHESTDT0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\CHESTDT1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\CHESTDT2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\CHESTDT5.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\MERCHDT0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\MERCHDT1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\MERCHDT2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\NPCCHAR0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\NPCCHAR1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\NPCCHAR2.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\PRTCHAR0.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\PRTCHAR1.XLD" },
        { "name": "C:\\Games\\albion\\cd\\XLD\\INITIAL\\PRTCHAR2.XLD" }
    ]
}""">

type CXld (j : ConfigProvider.Xld) =
    member val name = j.Name

type ConfigFile (j : ConfigProvider.Root) = 
    let xlds = new System.Collections.Generic.List<CXld>()

    do
        for xld in j.Xlds do xlds.Add(new CXld(xld))

    member val BaseXldPath = j.BaseXldPath
    member x.Xlds = xlds

module ConfigFile =
    let load path =
        let json =
            if (File.Exists path) then ConfigProvider.Parse(path)
            else ConfigProvider.GetSample()

        new ConfigFile(json)
        

    let save (config : ConfigProvider.Root) path = File.WriteAllText(path, string config)
        
