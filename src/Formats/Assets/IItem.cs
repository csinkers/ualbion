﻿using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public interface IItem : IContents { ItemId Id { get; } }