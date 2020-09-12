module UAlbion.Exporter

open System
open System.IO
open System.Text
open FSharp.Json

let baseDir = Util.findBasePath()
let xldDir =
    let path1 = Path.Combine(baseDir, @"albion/CD/XLDLIBS")
    if(Directory.Exists(path1)) then path1 else
    let path2 = Path.Combine(baseDir, @"albion/CD/ALBION/XLDLIBS")
    if(Directory.Exists(path2)) then path2 else
    failwithf "Could not find XLD directory, tried %s and %s" path1 path2

let mainOutputDir = Path.Combine(baseDir, @"data\exported\raw")
let xmiToMidiPath = Path.Combine(baseDir, @"Tools\XmiToMidi.exe")
let bytesTo850String (bytes:byte array) = Encoding.GetEncoding(850).GetString(bytes).Replace("×", "ß").TrimEnd((char)0)

type OText =
    {
        strings : string array
    }
    static member load bytes = ()
    member x.toBytes () = ()
    member x.toJson () = ()

type TextureSpacing =
    | None
    | Mirror
    | Tile
    | Transparent

type OSprite =
    {
        palette_shift : uint32
        offset : uint32
        replacementAlpha : int option

        // Only MultiXld
        texNum : uint32 // /100=XLD %100=ObjNum
        texCoordA : single * single
        texCoordB : single * single

        // SingleObj & SingleObjPs only
        objCount : uint32
        data : byte array
        textCoords : ((single * single) array array) option // SingleObjPS only
    }

type OTile =
    {
        texture : int
    }

type OMap =
    {
        width : int
        height : int
        tiles : OTile array
    }

type OSound =
    {
        sampleRate : int
        samples : byte array
    }

type XldObject =
    | Raw of byte array
    | Text of OText
    | Sprite of OSprite
    | Map of OMap
    | Audio of OSound
    | Script of string

type WaveLibEntry =
    {
        isValid : int
        type1 : int
        type2 : int
        offset : uint32
        length : uint32
        unk14 : int
        unk18 : int
        sampleRate : uint32
    }

type XldFile =
    {
        path : string
    }

    static member raw extension (br:BinaryReader) outputDir i length =
        if(length = 0) then () else
        let bytes = br.ReadBytes(length)
        File.WriteAllBytes(Path.Combine(outputDir, sprintf "%02d.%s" i extension), bytes)

    static member graphics = XldFile.raw "bin"
    static member strings (br:BinaryReader) outputDir i length =
        if(length = 0) then () else
        let startOffset = br.BaseStream.Position
        let stringCount = int <| br.ReadUInt16()
        let stringLengths = Array.ofSeq <| seq { for _ in [1..stringCount] do yield int <| br.ReadUInt16() }
        let stringTable =
            stringLengths
            |> Seq.mapi (fun i l ->
                let bytes = br.ReadBytes(l)
                (sprintf "%2d" i), bytesTo850String bytes
            ) |> Map.ofSeq
        let json = Json.serialize stringTable
        File.WriteAllText(Path.Combine(outputDir, sprintf "%02d.json" i), json)
        assert (br.BaseStream.Position = startOffset + (int64 length))

    static member chunkedStrings chunkSize (br:BinaryReader) outputDir i length =
        if (length = 0) then () else
        assert (length % chunkSize = 0)
        let stringTable =
            seq {
                for stringid in [1..(length/chunkSize)] do
                    let bytes = br.ReadBytes(chunkSize)
                    let str = bytesTo850String bytes
                    if (Util.stringNotEmpty str) then yield (string stringid), str
            } |> Map.ofSeq
        let json = Json.serialize stringTable
        File.WriteAllText(Path.Combine(outputDir, sprintf "%02d.json" i), json)

    static member samples (br:BinaryReader) outputDir i length =
        if (length = 0) then () else
        let bytes = br.ReadBytes(length)
        let filename = sprintf "%s\\%02d.wav" outputDir i
        Util.writeWavFile filename bytes 11025u 1us 1us

    static member wavelib (br:BinaryReader) outputDir i length =
        if (length = 0) then () else // Ignore empty files
        let startPos = br.BaseStream.Position

        let headers = Array.ofSeq <| seq {
            for _ in [1u..512u] do
                let isValid    = br.ReadInt32()
                let type1      = br.ReadInt32()
                let type2      = br.ReadInt32()
                let offset     = br.ReadUInt32()
                let length     = br.ReadUInt32()
                let unk14      = br.ReadInt32()
                let unk18      = br.ReadInt32()
                let sampleRate = br.ReadUInt32()

                // Check for new patterns
                assert(isValid = 0 || isValid = -1)
                assert([119; 120; 121; 122; 123; 124; 125; 126; 127; -1] |> List.contains type1)
                assert([56; 58; 60; 62; 63; 64; 66; 69; 76; 80] |> List.contains type2)
                assert(unk14 = 0)
                assert(unk18 = 0)
                assert(sampleRate = 11025u || sampleRate = UInt32.MaxValue)
                yield { isValid=isValid; type1=type1; type2=type2; offset=offset; length=length; unk14=unk14; unk18=unk18; sampleRate=sampleRate }
        }

        headers |> Array.iteri (fun j header ->
            if (header.isValid = -1) then () else
            assert (br.BaseStream.Position - startPos = (int64 header.offset))
            let bytes = br.ReadBytes(int header.length)
            let filename = sprintf "%s\\%02d_%03d_%d_%d.wav" outputDir i j header.type1 header.type2
            Util.writeWavFile filename bytes header.sampleRate 1us 1us
        )

        let bytesRemaining = length - (int (br.BaseStream.Position - startPos))
        if(bytesRemaining > 0) then
            br.ReadBytes(bytesRemaining) |> ignore
            assert false
        (* WaveLib Reversing notes:
Total size: 0x1ae3b (110,139 bytes)

0x00: int ??         = 0
0x04: int Type1      = 0x79 (121)
0x08: int Type2      = 0x45 (69)
0x0c: int Offset     = 0x4000 (16,384)
0x10: int Length     = 0x682a (26,666)
0x14: int ??         = 0
0x18: int ??         = 0
0x1c: int SampleRate = 2b11 (11,025)
0000000: 0000 0000 7900 0000 4500 0000 0040 0000  ....y...E....@..
0000010: 2a68 0000 0000 0000 0000 0000 112b 0000  *h...........+..

0x20: int ??         = 0
0x24: int Type1      = 0x7a (122)
0x28: int Type2      = 3c (60)
0x2c: int Offset     = a22a
0x30: int Length     = 38af
0x34: int ??         = 0
0x38: int ??         = 0
0x3c: int SampleRate = 2b11
0000020: 0000 0000 7a00 0000 3c00 0000 2aa8 0000  ....z...<...*...
0000030: af38 0000 0000 0000 0000 0000 112b 0000  .8...........+..

0x40: int ??         = 0
0x44: int Type1      = 7b (123)
0x48: int Type2      =
0x4c: int Offset     =
0x50: int Length     =
0x54: int ??         =
0x58: int ??         =
0x5c: int SampleRate =
0000040: 0000 0000 7b00 0000 3c00 0000 d9e0 0000  ....{...<.......
0000050: e62e 0000 0000 0000 0000 0000 112b 0000  .............+..

0x60: int ??         =
0x64: int Type1      = 7c (124)
0x68: int Type2      =
0x6c: int Offset     =
0x70: int Length     =
0x74: int ??         =
0x78: int ??         =
0x7c: int SampleRate =
0000060: 0000 0000 7c00 0000 3c00 0000 bf0f 0100  ....|...<.......
0000070: 9e0c 0000 0000 0000 0000 0000 112b 0000  .............+..

0x80: int ??         =
0x84: int Type1      = 7d (125)
0x88: int Type2      = 0x3c
0x8c: int Offset     = 0x11c5d
0x90: int Length     = 0x29b4
0x94: int ??         = 0
0x98: int ??         = 0
0x9c: int SampleRate = 2b11 (11,025)
0000080: 0000 0000 7d00 0000 3c00 0000 5d1c 0100  ....}...<...]...
0000090: b429 0000 0000 0000 0000 0000 112b 0000  .)...........+..

0xa0: int ??         = 0
0xa4: int Type1      = 0x79 (121)
0xa8: int Type2      = 0x3c
0xac: int Offset     = 0x14611 (83,473)
0xb0: int Length     = 0x682a (26,666)
0xb4: int ??         = 0
0xb8: int ??         = 0
0xbc: int SampleRate = 2b11 (11,025)

00000a0: 0000 0000 7900 0000 3c00 0000 1146 0100  ....y...<....F..
00000b0: 2a68 0000 0000 0000 0000 0000 112b 0000  *h...........+..

0xc0: int ??         = -1
0xc4: int Type1      = -1
0xa8: int Type2      = 0x3c (60)
0xac: int Offset     = 0
0xb0: int Length     = 0
0xb4: int ??         = 0
0xb8: int ??         = 0
0xbc: int SampleRate = -1
00000c0: ffff ffff ffff ffff 3c00 0000 0000 0000  ........<.......
00000d0: 0000 0000 0000 0000 0000 0000 ffff ffff  ................

... (total of 1fa 32-byte records = 506, w/ 6 genuine)

0003fe0: ffff ffff ffff ffff 3c00 0000 0000 0000  ........<.......
0003ff0: 0000 0000 0000 0000 0000 0000 ffff ffff  ................

0004000: 7f7e 7e7e 7e7e 7e7e 7e7e 7e7e 7e7e 7e7e  .~~~~~~~~~~~~~~~
... wave data
001ae30: 7d7d 7d7d 7e7e 7e7e 7e7e 7e              }}}}~~~~~~~
        *)

    static member xmi (br:BinaryReader) outputDir i length =
        if (File.Exists xmiToMidiPath |> not) then () else

        let bytes = br.ReadBytes(length)
        let xmiPath = Path.Combine(outputDir, sprintf "%02d.xmi" i)
        File.WriteAllBytes(xmiPath, bytes)

        Util.runExe xmiToMidiPath xmiPath (fun (result, errors) -> // Convert to midi
            if (Util.stringNotEmpty result) then printfn "XmiToMidi output: %s" result
            if (Util.stringNotEmpty errors) then printfn "XmiToMidi errors: %s" errors
        )

    static member export exportFunc path outputDir =
        let header = [| byte 'X'; byte 'L'; byte 'D'; byte '0'; byte 'I'; 0uy |]
        use stream = File.OpenRead(path)
        use br = new BinaryReader(stream)
        if (br.ReadBytes(header.Length) <> header) then () else
        if(not <| Directory.Exists(outputDir)) then Directory.CreateDirectory(outputDir) |> ignore

        let objectCount = int (br.ReadUInt16())
        let objectLengths = Array.ofSeq <| seq { for _ in [1..objectCount] do yield int <| br.ReadUInt32() }
        objectLengths |> Seq.iteri (fun i length -> exportFunc br outputDir i length)

let copyRaw extension path outputDir =
    use stream = File.OpenRead(path)
    use br = new BinaryReader(stream)
    if(not <| Directory.Exists(outputDir)) then Directory.CreateDirectory(outputDir) |> ignore

    let length = br.BaseStream.Length |> int
    if(length = 0) then () else
    let bytes = br.ReadBytes(length)
    File.WriteAllBytes(Path.Combine(outputDir, sprintf "%02d.%s" 0 extension), bytes)

let files =
    [
//    (*
        @"3DBCKGR0.XLD", (XldFile.export XldFile.graphics)  // Large background images for 3D
        @"3DFLOOR0.XLD", (XldFile.export XldFile.graphics)  // Floor textures for 3D
        @"3DFLOOR1.XLD", (XldFile.export XldFile.graphics)  // Floor textures for 3D
        @"3DFLOOR2.XLD", (XldFile.export XldFile.graphics)  // Floor textures for 3D
        @"3DOBJEC0.XLD", (XldFile.export XldFile.graphics)  // Sprites for 3D
        @"3DOBJEC1.XLD", (XldFile.export XldFile.graphics)  // Sprites for 3D
        @"3DOBJEC2.XLD", (XldFile.export XldFile.graphics)  // Sprites for 3D
        @"3DOBJEC3.XLD", (XldFile.export XldFile.graphics)  // Sprites for 3D
        @"3DOVERL0.XLD", (XldFile.export XldFile.graphics)  // Misc sprites for 3D
        @"3DOVERL1.XLD", (XldFile.export XldFile.graphics)  // Misc sprites for 3D
        @"3DOVERL2.XLD", (XldFile.export XldFile.graphics)  // Misc sprites for 3D
        @"3DWALLS0.XLD", (XldFile.export XldFile.graphics)  // Wall textures for 3D
        @"3DWALLS1.XLD", (XldFile.export XldFile.graphics)  // Wall textures for 3D
        @"AUTOGFX0.XLD", (XldFile.export XldFile.graphics) // Tiny sprite maps?
        @"BLKLIST0.XLD", (XldFile.export XldFile.graphics) // ???
        @"COMBACK0.XLD", (XldFile.export XldFile.graphics)  // Combat backgrounds
        @"COMGFX0.XLD",  (XldFile.export XldFile.graphics)   // Combat sprites / particles etc
        @"EVNTSET0.XLD", (XldFile.export XldFile.graphics) // ???
        @"EVNTSET1.XLD", (XldFile.export XldFile.graphics)
        @"EVNTSET2.XLD", (XldFile.export XldFile.graphics)
        @"EVNTSET3.XLD", (XldFile.export XldFile.graphics)
        @"EVNTSET9.XLD", (XldFile.export XldFile.graphics)
        @"FBODPIX0.XLD", (XldFile.export XldFile.graphics)  // Playable character pictures
        @"FONTS0.XLD",   (XldFile.export XldFile.graphics)   // Fonts, unknown format
        @"ICONDAT0.XLD", (XldFile.export XldFile.graphics)  // Sprite-maps?
        @"ICONGFX0.XLD", (XldFile.export XldFile.graphics)  // Sprite-maps?
        @"ITEMGFX",      copyRaw "bin"     // Just an array of 16x16 sprites appended together
        // @"ITEMLIST.DAT", NONXLD            // Probably contains weapon damage etc stats
        // @"ITEMNAME.DAT", NONXLD            // Item names, char[20][], ordered in groups of DE, EN, FR (462 items)
        @"LABDATA0.XLD", (XldFile.export XldFile.graphics) // Not graphics, but definitely orderly
        @"LABDATA1.XLD", (XldFile.export XldFile.graphics)
        @"LABDATA2.XLD", (XldFile.export XldFile.graphics)
        @"MAPDATA1.XLD", (XldFile.export XldFile.graphics)       // 2D map data
        @"MAPDATA2.XLD", (XldFile.export XldFile.graphics)       // 2D map data
        @"MAPDATA3.XLD", (XldFile.export XldFile.graphics)       // 2D map data
        @"MONCHAR0.XLD", (XldFile.export XldFile.graphics)  // Monster combat spritemaps (heterogeneous)
        @"MONGFX0.XLD",  (XldFile.export XldFile.graphics)  // Tiny files
        @"MONGRP0.XLD",  (XldFile.export XldFile.graphics)  // Tiny files
        @"MONGRP1.XLD",  (XldFile.export XldFile.graphics)  // Tiny files
        @"MONGRP2.XLD",  (XldFile.export XldFile.graphics)  // Tiny files
        @"NPCGR0.XLD",   (XldFile.export XldFile.graphics)    // 2D NPC graphics. Spritemaps, 32 wide. (GR=Groß)
        @"NPCGR1.XLD",   (XldFile.export XldFile.graphics)    // 2D NPC graphics
        @"NPCKL0.XLD",   (XldFile.export XldFile.graphics)    // 2D NPCs on the world map (KL=Klein)
        @"PALETTE.000",  (XldFile.export XldFile.graphics)  // ?? 192 bytes
        @"PALETTE0.XLD", (XldFile.export XldFile.graphics) // Palette data, 576 bytes/palette. 64 byte header + 256*16bit colour
        @"PARTGR0.XLD",  (XldFile.export XldFile.graphics)  // 2D Player characters
        @"PARTKL0.XLD",  (XldFile.export XldFile.graphics)  // 2D Player characters on the world map
        @"PICTURE0.XLD", (XldFile.export XldFile.graphics)  // Heterogeneous spritemaps?
        @"SAMPLES0.XLD", (XldFile.export XldFile.samples)   // Raw audio samples
        @"SAMPLES1.XLD", (XldFile.export XldFile.samples)   // Raw audio samples
        @"SAMPLES2.XLD", (XldFile.export XldFile.samples)   // Raw audio samples
        @"SCRIPT0.XLD",  (XldFile.export (XldFile.raw "txt"))  // Plain text scripts
        @"SCRIPT2.XLD",  (XldFile.export (XldFile.raw "txt"))  // Plain text scripts
        @"SLAB",         copyRaw "bin"         // Some sort of bitfield, everything is either 0xc_ or 0xf_. 86400 bytes (2^7, 3^3, 5^2)
        @"SMLPORT0.XLD", (XldFile.export XldFile.graphics)  // Small portraits, mostly 34 wide.
        @"SMLPORT1.XLD", (XldFile.export XldFile.graphics)  // Small portraits, mostly 34 wide.
        @"SONGS0.XLD",   (XldFile.export XldFile.xmi)         // XMI music files, can be converted to MIDI (imperfectly)
        @"SPELLDAT.DAT", (XldFile.export XldFile.graphics) // ???
        @"TACTICO0.XLD", (XldFile.export XldFile.graphics)  // Battle sprites, all 32 wide.
        @"TRANSTB0.XLD", (XldFile.export XldFile.graphics) // ?? All files are 196608 bytes, some periodicity
        @"WAVELIB0.XLD", (XldFile.export XldFile.wavelib)   // Sound effects
        @"ENGLISH\EVNTTXT0.XLD", (XldFile.export XldFile.strings) // Translated text
        @"ENGLISH\EVNTTXT1.XLD", (XldFile.export XldFile.strings) // Translated text
        @"ENGLISH\EVNTTXT2.XLD", (XldFile.export XldFile.strings) // Translated text
        @"ENGLISH\EVNTTXT3.XLD", (XldFile.export XldFile.strings) // Translated text
        @"ENGLISH\EVNTTXT9.XLD", (XldFile.export XldFile.strings) // Translated text
        @"ENGLISH\FLICS0.XLD",   (XldFile.export XldFile.graphics) // Probably SMK video
        @"ENGLISH\MAPTEXT1.XLD", (XldFile.export XldFile.strings) // Translated text
        @"ENGLISH\MAPTEXT2.XLD", (XldFile.export XldFile.strings) // Translated text
        @"ENGLISH\MAPTEXT3.XLD", (XldFile.export XldFile.strings) // Translated text
        @"ENGLISH\SYSTEXTS",     (XldFile.export (XldFile.raw "txt"))   // Plain text [%04d:format string]
        @"ENGLISH\WORDLIS0.XLD", (XldFile.export (XldFile.chunkedStrings 21)) // Topic words in zero padded chunks of 21 bytes

        @"GERMAN\EVNTTXT0.XLD", (XldFile.export XldFile.strings)  // Translated text
        @"GERMAN\EVNTTXT1.XLD", (XldFile.export XldFile.strings)  // Translated text
        @"GERMAN\EVNTTXT2.XLD", (XldFile.export XldFile.strings)  // Translated text
        @"GERMAN\EVNTTXT3.XLD", (XldFile.export XldFile.strings)  // Translated text
        @"GERMAN\EVNTTXT9.XLD", (XldFile.export XldFile.strings)  // Translated text
        @"GERMAN\FLICS0.XLD",   (XldFile.export XldFile.graphics) // Probably SMK video
        @"GERMAN\MAPTEXT1.XLD", (XldFile.export XldFile.strings)  // Translated text
        @"GERMAN\MAPTEXT2.XLD", (XldFile.export XldFile.strings)  // Translated text
        @"GERMAN\MAPTEXT3.XLD", (XldFile.export XldFile.strings)  // Translated text
        @"GERMAN\SYSTEXTS",     (XldFile.export (XldFile.raw "txt"))   // Plain text [%04d:format string]
        @"GERMAN\WORDLIS0.XLD", (XldFile.export (XldFile.chunkedStrings 21)) // Topic words in zero padded chunks of 21 bytes

        // Map metadata?
        @"INITIAL\AUTOMAP1.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\AUTOMAP2.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\AUTOMAP3.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\CHESTDT0.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\CHESTDT1.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\CHESTDT2.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\CHESTDT5.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\MERCHDT0.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\MERCHDT1.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\MERCHDT2.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\NPCCHAR0.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\NPCCHAR1.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\NPCCHAR2.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\PRTCHAR0.XLD", (XldFile.export XldFile.graphics)
        @"INITIAL\PRTCHAR1.XLD", (XldFile.export XldFile.graphics)
        //*)
        @"INITIAL\PRTCHAR2.XLD", (XldFile.export XldFile.graphics)
    ]

[<EntryPoint>]
let main argv =
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance) // Required for code page 850 support in .NET Core

    files
    |> Seq.iter (fun (path, exportFunc) ->
        exportFunc (Path.Combine(xldDir, path)) (Path.Combine(mainOutputDir, path))
    )
    0

(*
Script parsing:
    ; = comment
    active_member_text %d
    ambient %d
    camera_jump x y ; Teleport camera to absolute coordinates
    camera_lock ; Start manually controlling camera
    camera_move x y ; Move camera via relative coordinates
    camera_unlock ; Stop manually controlling camera
    clear_quest_bit %d
    do_event_chain %d
    fade_from_black
    fade_from_white
    fade_to_black
    fade_to_white
    fill_screen %d
    fill_screen_0
    load_pal %d
    npc_jump npcid x y ; Teleport an npc
    npc_lock npcid ; Start manually controlling an NPC
    npc_move npcid x y ; Move NPC via relative coordinates
    npc_off %d
    npc_on %d
    npc_text %d stringid ; Make NPC talk. First param doesn't match other npc commands' npcids, bitfield?
    npc_turn npcid ; Script bug? Only appears once
    npc_turn npcid direction ; Turn NPC to face a certain direction (0 = ?, 1 = ?, 2 = ?, 3 = ?)
    npc_unlock npcid ; Stop manually controlling an NPC
    party_jump %d %d
    party_member_text %d stringid
    party_move x y ; 0 -1 = up, 1 0 = right
    party_off
    party_on
    party_turn %d
    pause frames ; Do nothing for a few frames
    play %d
    play_anim %d %d %d %d %d
    show_map ; Show 2D map
    show_pic %d
    show_pic %d %d %d
    show_picture %d %d %d
    song songid ; Play a music track
    sound %d %d %d %d %d
    sound_effect %d %d %d %d %d
    sound_fx_off
    start_anim %d %d %d %d
    start_anim %d %d %d %d %d
    stop_anim
    text stringid ; Trigger message / internal thoughts
    update frames ; Run the primary game loop for a few frames

Text/Convo parsing:
    ^ = newline
    {INK %03d} = Set font colour
    {BIG } = Large font size
    {FAT } = Large font size?
    {CNTR} = Centered
    {FAHI} = Fat + High?
    {LEFT} = Left justify?

    {LEAD} = Set context to party leader?
    {SUBJ} = Set context to target of action
    {COMB} = Set context to combatant
    {VICT} = Set context to victim of combatant
    {INVE} = Set context to recipient of an inventory move

    {NAME} = Name of current context
    {WEAP} = Weapon name
    {DAMG} = Damage
    {HE } = Third person nominative pronoun
    {HIM } = Third person accusative pronoun
    {HIS } = Third person possessive pronoun
    {SEXC} = Sex of current context
    {RACE} = Race of current context
    {CLAS} = Class of current context
    {PRIC} = Price of current goods?

    {WORDfoo} = Word reference
    {BLOK%03d} = ??
    {WRNR----} = ??
    {JUST} = ??
    {TECF} = ??
    {UNKN} = ??
    {NORS} = ??
    {HIGH} = ??
*)

