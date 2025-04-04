﻿// ReSharper disable InconsistentNaming
namespace UAlbion.Base;

#pragma warning disable CA1712 // Do not prefix enum values with type name
public enum Switch : ushort
{
    Switch0 = 0, // "get_switch 0" used as a NOP?
    DrirrSuggestedPressingBlueFloorPlate = 1,
    KengetDungeonSwitchActive = 2,
    KengetFireballSwitch1 = 3,
    KengetFireballSwitch2 = 4,
    KengetFireballSwitch3 = 5,
    KengetFireballSwitch4 = 6,
    KengetLavaPatch1 = 7,
    KengetLavaPatch2 = 8,
    KengetLavaPatch3 = 9,
    KengetLavaPatch4 = 10,
    KengetLavaPatch5 = 11,
    KengetLavaPatch6 = 12,
    KengetLavaPatch7 = 13,
    KengetLavaPatch8 = 14,
    KengetLavaPatch9 = 15,
    KengetLavaPatch10 = 16,
    KengetLavaPatch11 = 17,
    KengetLavaPatch12 = 18,
    KengetLavaPatch13 = 19,
    Switch20 = 20,
    KengetNoviceLeftRoom = 21,
    KhunagMentionedSecretPassage = 22,
    KhunagLedPartyToSecretPassage = 23,
    UnusedMaini24 = 24,
    KilledCairnain = 25,
    KengetFloorStructureActivated1 = 26,
    KengetFloorStructureActivated2 = 27,
    KengetGiantFlameActive = 28,
    ComplainedAboutHospitalRoom = 29,
    FoundCodePaper = 30,
    JoeMentionedConsoleSignificance = 31,
    JoeDisabledConsole = 32,
    FoundReactorCoreCode = 33,
    JoeMentionedTimeDependentControl = 34,
    TomMentionedServiceLevelAccess = 35,
    ControlLightsWallRemoved = 36,
    HasVisitedTorontoPt3 = 37,
    AttackedColonelPriver = 38,
    TransportCaveCurrentlyTeleporting = 39,
    ReactedToFirstTeleport = 40,
    UsedIntelligenceFlower = 41,
    UsedStrengthFlower = 42,
    UsedDexterityFlower = 43, // "more in control of body"
    UsedSpeedFlower = 44,
    UsedLuckFlower = 45,
    UsedMagicResistanceFlower = 46, // "spirit has become steadier"
    UsedMagicAbilityFlower = 47,
    OnMissionToObtainHighKnowledge = 48,
    OnMissionToDestroyShip = 49,
    RainerChoseToRemainWithDjiCantos = 50,
    AskedAltheaAboutSpellScrolls = 51,
    VisitedDesertWhileOnTorontoQuest = 52,
    UsedStaminaFlower = 53,
    AskedFerinaAboutTraining = 54,
    DrirrMentionedCaveSymbols = 55,
    CanAskKhunagToLeave = 56,
    DrinnoActivatedSwitch1 = 57,
    DrinnoActivatedSwitch2 = 58,
    KengetGuardsGaveWarning = 59,
    Switch60 = 60,
    HadDrirrBeforeShuttleTrip = 61,
    HadSiraBeforeShuttleTrip = 62,
    HadMellthasBeforeShuttleTrip = 63,
    HadHarrietBeforeShuttleTrip = 64,
    HadJoeBeforeShuttleTrip = 65,
    HadKhunagBeforeShuttleTrip = 66,
    HadSiobhanBeforeShuttleTrip = 67,
    EscapedFromToronto = 68,
    HeardAssassinIsInOldFormerBuilding = 69,
    HasTalkedToAliis = 70,
    SpokeToLiWrinn = 71,
    BradirAskedForForgiveness = 72,
    Switch73 = 73,
    SiraAndTomDiscussedSeedSignificance = 74,
    DrinnoMovedWallWithSwitch = 75,
    TalkedToSiraAndMellthasAboutRejoining = 76,
    Switch77 = 77,
    Switch78 = 78,
    Switch79 = 79,
    Switch80 = 80,
    Switch81 = 81,
    Switch82 = 82,
    Switch83 = 83,
    Switch84 = 84,
    TomPlacedPistolInLocker = 85,
    Switch86 = 86,
    Switch87 = 87,
    Switch88 = 88,
    Switch89 = 89,
    Switch90 = 90,
    Switch91 = 91,
    Switch92 = 92,
    Switch93 = 93,
    Switch94 = 94,
    Switch95 = 95,
    Switch96 = 96,
    Switch97 = 97,
    Switch98 = 98,
    Switch99 = 99,
    Switch100 = 100,
    Switch101 = 101,
    Switch102 = 102,
    Switch103 = 103,
    Switch104 = 104,
    Switch105 = 105,
    HClanCabinetOpened = 106,
    HClanRainerArchComment = 107,
    Switch108 = 108,
    HClanRainerPumpComment = 109,
    Switch110 = 110,
    Switch111 = 111,
    Switch112 = 112,
    Switch113 = 113,
    Switch114 = 114,
    Switch115 = 115,
    Switch116 = 116,
    Switch117 = 117,
    Switch118 = 118,
    HasExtinguishedADrinnoFireBowl = 119,
    Switch120 = 120,
    Switch121 = 121,
    Switch122 = 122,
    Switch123 = 123,
    Switch124 = 124,
    Switch125 = 125,
    Switch126 = 126,
    Switch127 = 127,
    Switch128 = 128,
    Switch129 = 129,
    Switch130 = 130,
    Switch131 = 131,
    Switch132 = 132,
    Switch133 = 133,
    Switch134 = 134,
    Switch135 = 135,
    Switch136 = 136,
    Switch137 = 137,
    Switch138 = 138,
    Switch139 = 139,
    Switch140 = 140,
    Switch141 = 141,
    Switch142 = 142,
    Switch143 = 143,
    Switch144 = 144,
    Switch145 = 145,
    Switch146 = 146,
    Switch147 = 147,
    Switch148 = 148,
    Switch149 = 149,
    Switch150 = 150,
    Switch151 = 151,
    Switch152 = 152,
    Switch153 = 153,
    Switch154 = 154,
    Switch155 = 155,
    Switch156 = 156,
    DrirrHasExperimentedWithFloorPlate = 157,
    Switch158 = 158,
    Switch159 = 159,
    Switch160 = 160,
    Switch161 = 161,
    Switch162 = 162,
    Switch163 = 163,
    Switch164 = 164,
    Switch165 = 165,
    Switch166 = 166,
    Switch167 = 167,
    Switch168 = 168,
    Switch169 = 169,
    Switch170 = 170,
    Switch171 = 171,
    Switch172 = 172,
    OffendedCelt = 173,
    Switch174 = 174,
    Switch175 = 175,
    Switch176 = 176,
    Switch177 = 177,
    Switch178 = 178,
    Switch179 = 179,
    Switch180 = 180,
    Switch181 = 181,
    Switch182 = 182,
    Switch183 = 183,
    Switch184 = 184,
    Switch185 = 185,
    Switch186 = 186,
    Switch187 = 187,
    Switch188 = 188,
    Switch189 = 189,
    Switch190 = 190,
    Switch191 = 191,
    Switch192 = 192,
    Switch193 = 193,
    Switch194 = 194,
    Switch195 = 195,
    Switch196 = 196,
    Switch197 = 197,
    Switch198 = 198,
    Switch199 = 199,
    Switch200 = 200,
    Switch201 = 201,
    Switch202 = 202,
    Switch203 = 203,
    Switch204 = 204,
    Switch205 = 205,
    Switch206 = 206,
    Switch207 = 207,
    Switch208 = 208,
    Switch209 = 209,
    Switch210 = 210,
    Switch211 = 211,
    Switch212 = 212,
    Switch213 = 213,
    Switch214 = 214,
    Switch215 = 215,
    Switch216 = 216,
    Switch217 = 217,
    Switch218 = 218,
    Switch219 = 219,
    Switch220 = 220,
    Switch221 = 221,
    Switch222 = 222,
    Switch223 = 223,
    Switch224 = 224,
    Switch225 = 225,
    Switch226 = 226,
    Switch227 = 227,
    Switch228 = 228,
    Switch229 = 229,
    Switch230 = 230,
    Switch231 = 231,
    Switch232 = 232,
    Switch233 = 233,
    Switch234 = 234,
    Switch235 = 235,
    Switch236 = 236,
    Switch237 = 237,
    Switch238 = 238,
    Switch239 = 239,
    Switch240 = 240,
    Switch241 = 241,
    Switch242 = 242,
    Switch243 = 243,
    Switch244 = 244,
    Switch245 = 245,
    Switch246 = 246,
    Switch247 = 247,
    Switch248 = 248,
    Switch249 = 249,
    Switch250 = 250,
    Switch251 = 251,
    Switch252 = 252,
    Switch253 = 253,
    Switch254 = 254,
    Switch255 = 255,
    Switch256 = 256,
    Switch257 = 257,
    Switch258 = 258,
    Switch259 = 259,
    Switch260 = 260,
    Switch261 = 261,
    Switch262 = 262,
    Switch263 = 263,
    Switch264 = 264,
    Switch265 = 265,
    Switch266 = 266,
    Switch267 = 267,
    Switch268 = 268,
    Switch269 = 269,
    Switch270 = 270,
    Switch271 = 271,
    Switch272 = 272,
    Switch273 = 273,
    Switch274 = 274,
    Switch275 = 275,
    Switch276 = 276,
    Switch277 = 277,
    Switch278 = 278,
    Switch279 = 279,
    Switch280 = 280,
    Switch281 = 281,
    Switch282 = 282,
    Switch283 = 283,
    Switch284 = 284,
    Switch285 = 285,
    Switch286 = 286,
    Switch287 = 287,
    Switch288 = 288,
    Switch289 = 289,
    Switch290 = 290,
    Switch291 = 291,
    Switch292 = 292,
    Switch293 = 293,
    Switch294 = 294,
    Switch295 = 295,
    Switch296 = 296,
    Switch297 = 297,
    Switch298 = 298,
    Switch299 = 299,
    Switch300 = 300,
    Switch301 = 301,
    Switch302 = 302,
    Switch303 = 303,
    Switch304 = 304,
    Switch305 = 305,
    Switch306 = 306,
    Switch307 = 307,
    Switch308 = 308,
    Switch309 = 309,
    Switch310 = 310,
    Switch311 = 311,
    Switch312 = 312,
    Switch313 = 313,
    Switch314 = 314,
    Switch315 = 315,
    Switch316 = 316,
    Switch317 = 317,
    Switch318 = 318,
    Switch319 = 319,
    Switch320 = 320,
    Switch321 = 321,
    Switch322 = 322,
    Switch323 = 323,
    Switch324 = 324,
    Switch325 = 325,
    Switch326 = 326,
    Switch327 = 327,
    Switch328 = 328,
    Switch329 = 329,
    Switch330 = 330,
    Switch331 = 331,
    Switch332 = 332,
    Switch333 = 333,
    Switch334 = 334,
    Switch335 = 335,
    Switch336 = 336,
    Switch337 = 337,
    Switch338 = 338,
    Switch339 = 339,
    Switch340 = 340,
    Switch341 = 341,
    Switch342 = 342,
    Switch343 = 343,
    Switch344 = 344,
    Switch345 = 345,
    Switch346 = 346,
    Switch347 = 347,
    Switch348 = 348,
    Switch349 = 349,
    Switch350 = 350,
    Switch351 = 351,
    Switch352 = 352,
    Switch353 = 353,
    Switch354 = 354,
    Switch355 = 355,
    Switch356 = 356,
    Switch357 = 357,
    Switch358 = 358,
    Switch359 = 359,
    Switch360 = 360,
    Switch361 = 361,
    Switch362 = 362,
    Switch363 = 363,
    Switch364 = 364,
    Switch365 = 365,
    Switch366 = 366,
    Switch367 = 367,
    Switch368 = 368,
    Switch369 = 369,
    Switch370 = 370,
    Switch371 = 371,
    Switch372 = 372,
    Switch373 = 373,
    Switch374 = 374,
    Switch375 = 375,
    Switch376 = 376,
    Switch377 = 377,
    Switch378 = 378,
    Switch379 = 379,
    Switch380 = 380,
    Switch381 = 381,
    Switch382 = 382,
    Switch383 = 383,
    Switch384 = 384,
    Switch385 = 385,
    Switch386 = 386,
    Switch387 = 387,
    Switch388 = 388,
    Switch389 = 389,
    Switch390 = 390,
    Switch391 = 391,
    Switch392 = 392,
    Switch393 = 393,
    Switch394 = 394,
    Switch395 = 395,
    Switch396 = 396,
    Switch397 = 397,
    Switch398 = 398,
    Switch399 = 399,
    Switch400 = 400,
    Switch401 = 401,
    Switch402 = 402,
    Switch403 = 403,
    Switch404 = 404,
    Switch405 = 405,
    Switch406 = 406,
    Switch407 = 407,
    HerrasKilled = 408,
    Switch409 = 409,
    Switch410 = 410,
    Switch411 = 411,
    Switch412 = 412,
    Switch413 = 413,
    Switch414 = 414,
    Switch415 = 415,
    Switch416 = 416,
    Switch417 = 417,
    Switch418 = 418,
    Switch419 = 419,
    Switch420 = 420,
    Switch421 = 421,
    Switch422 = 422,
    Switch423 = 423,
    Switch424 = 424,
    Switch425 = 425,
    Switch426 = 426,
    Switch427 = 427,
    Switch428 = 428,
    Switch429 = 429,
    Switch430 = 430,
    Switch431 = 431,
    HerrasWarned = 432,
    Switch433 = 433,
    Switch434 = 434,
    Switch435 = 435,
    Switch436 = 436,
    Switch437 = 437,
    Switch438 = 438,
    Switch439 = 439,
    Switch440 = 440,
    Switch441 = 441,
    Switch442 = 442,
    Switch443 = 443,
    Switch444 = 444,
    Switch445 = 445,
    Switch446 = 446,
    Switch447 = 447,
    Switch448 = 448,
    Switch449 = 449,
    Switch450 = 450,
    Switch451 = 451,
    Switch452 = 452,
    Switch453 = 453,
    Switch454 = 454,
    Switch455 = 455,
    Switch456 = 456,
    Switch457 = 457,
    Switch458 = 458,
    Switch459 = 459,
    Switch460 = 460,
    Switch461 = 461,
    Switch462 = 462,
    Switch463 = 463,
    Switch464 = 464,
    Switch465 = 465,
    Switch466 = 466,
    Switch467 = 467,
    Switch468 = 468,
    Switch469 = 469,
    Switch470 = 470,
    Switch471 = 471,
    Switch472 = 472,
    Switch473 = 473,
    Switch474 = 474,
    Switch475 = 475,
    Switch476 = 476,
    Switch477 = 477,
    Switch478 = 478,
    Switch479 = 479,
    Switch480 = 480,
    Switch481 = 481,
    Switch482 = 482,
    Switch483 = 483,
    Switch484 = 484,
    Switch485 = 485,
    Switch486 = 486,
    Switch487 = 487,
    Switch488 = 488,
    Switch489 = 489,
    Switch490 = 490,
    Switch491 = 491,
    Switch492 = 492,
    Switch493 = 493,
    Switch494 = 494,
    Switch495 = 495,
    Switch496 = 496,
    Switch497 = 497,
    Switch498 = 498,
    Switch499 = 499,
    Switch500 = 500,
    Switch501 = 501,
    Switch502 = 502,
    Switch503 = 503,
    ExpelledFromSouthWind = 504,
    Switch505 = 505,
    Switch506 = 506,
    Switch507 = 507,
    M122_MentionedGlowingFloor = 508,
    Switch509 = 509,
    Switch510 = 510,
    Switch511 = 511,
    Switch512 = 512,
    Switch513 = 513,
    Switch514 = 514,
    Switch515 = 515,
    Switch516 = 516,
    Switch517 = 517,
    Switch518 = 518,
    Switch519 = 519,
    Switch520 = 520,
    Switch521 = 521,
    Switch522 = 522,
    Switch523 = 523,
    Switch524 = 524,
    Switch525 = 525,
    Switch526 = 526,
    Switch527 = 527,
    Switch528 = 528,
    Switch529 = 529,
    Switch530 = 530,
    Switch531 = 531,
    Switch532 = 532,
    Switch533 = 533,
    Switch534 = 534,
    Switch535 = 535,
    Switch536 = 536,
    Switch537 = 537,
    Switch538 = 538,
    Switch539 = 539,
    Switch540 = 540,
    Switch541 = 541,
    Switch542 = 542,
    Switch543 = 543,
    Switch544 = 544,
    Switch545 = 545,
    Switch546 = 546,
    Switch547 = 547,
    Switch548 = 548,
    Switch549 = 549,
    Switch550 = 550,
    Switch551 = 551,
    Switch552 = 552,
    Switch553 = 553,
    Switch554 = 554,
    M122_ShownDogFlyMsg = 555,
    Switch556 = 556,
    Switch557 = 557,
    Switch558 = 558,
    M122_ArgimRewardedParty = 559,
    M122_GaveMusicToArgim = 560,
    M122_AskedArgimToMove = 561,
    Switch562 = 562,
    M122_ShownPlantTouchMsg = 563,
    ArgimDead = 564,
    Switch565 = 565,
    Switch566 = 566,
    Switch567 = 567,
    Switch568 = 568,
    Switch569 = 569,
    Switch570 = 570,
    Switch571 = 571,
    Switch572 = 572,
    Switch573 = 573,
    Switch574 = 574,
    ShownInsectStingMsg = 575,
    HClanCabinetAnnounced = 576,
    FestivalTimerActive = 577,
    FestivalTimerFired = 578,
    Switch579 = 579,
    Switch580 = 580,
    DrirrCommentedOnSeedBush = 581,
    Switch582 = 582,
    Switch583 = 583,
    Switch584 = 584,
    Switch585 = 585,
    Switch586 = 586,
    Switch587 = 587,
    Switch588 = 588,
    Switch589 = 589,
    Switch590 = 590,
    Switch591 = 591,
    Switch592 = 592,
    Switch593 = 593,
    GaveMusicToFrinos = 594,
    FoundBeroInDrinno = 595,
    Switch596 = 596,
    SiraAndMellthasTogether = 597,
    Switch598 = 598,
    Switch599 = 599,
    Switch600 = 600,
}
#pragma warning restore CA1712 // Do not prefix enum values with type name
