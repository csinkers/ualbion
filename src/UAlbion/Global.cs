using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Formats;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Input;
using UAlbion.Game.Magic;
using UAlbion.Game.State;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Input;

namespace UAlbion;

public class G : Component
{
    public static G Instance { get; } = new();
    G() { }

    protected override void Subscribed()
    {
        _engine = (Engine)TryResolve<IEngine>();
        _disk = TryResolve<IFileSystem>();
        _mods = (ModApplier)TryResolve<IModApplier>();
        _assets = TryResolve<IAssetManager>();
        _words = TryResolve<IWordLookup>();
        _spells = TryResolve<SpellManager>();
        _game = (GameState)TryResolve<IGameState>();
        _samplers = (SpriteSamplerSource)TryResolve<ISpriteSamplerSource>();
        _chains = (EventChainManager)TryResolve<IEventManager>();
        _maps = (MapManager)TryResolve<IMapManager>();
        _coll = (CollisionManager)TryResolve<ICollisionManager>();
        _scenes = (SceneManager)TryResolve<ISceneManager>();
        _layoutManager = (LayoutManager)TryResolve<ILayoutManager>();
        _dialogs = (DialogManager)TryResolve<IDialogManager>();
        _cursors = (CursorManager)TryResolve<ICursorManager>();
        _input = (InputManager)TryResolve<IInputManager>();
        _convos = (ConversationManager)TryResolve<IConversationManager>();
    }

    public static Engine Engine => Instance._engine;
    public static ModApplier Mods => Instance._mods;
    public static IFileSystem Disk => Instance._disk;
    public static IAssetManager Assets => Instance._assets;
    public static SpellManager Spells => Instance._spells;
    public static IWordLookup Words => Instance._words;
    public static GameState Game => Instance._game;
    public static Party Party => (Party)Game?.Party;
    public static SpriteSamplerSource Samplers => Instance._samplers;
    public static EventChainManager Chains => Instance._chains;
    public static MapManager Maps => Instance._maps;
    public static IMap Map => Maps?.Current;
    public static CollisionManager Coll => Instance._coll;
    public static SceneManager Scenes => Instance._scenes;
    public static LayoutManager LayoutManager => Instance._layoutManager;
    public static LayoutNode Layout => LayoutManager?.GetLayout();
    public static DialogManager Dialogs => Instance._dialogs;
    public static CursorManager Cursors => Instance._cursors;
    public static InputManager Input => Instance._input;
    public static ConversationManager Convos => Instance._convos;
    public static Conversation Convo => Convos?.Conversation;

    Engine _engine;
    ModApplier _mods;
    IFileSystem _disk;
    IAssetManager _assets;
    SpellManager _spells;
    IWordLookup _words;
    GameState _game;
    SpriteSamplerSource _samplers;
    EventChainManager _chains;
    MapManager _maps;
    CollisionManager _coll;
    SceneManager _scenes;
    LayoutManager _layoutManager;
    DialogManager _dialogs;
    CursorManager _cursors;
    InputManager _input;
    ConversationManager _convos;
}
