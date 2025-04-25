#if DEBUG
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

    public Engine Engine => (Engine)TryResolve<IEngine>();
    public ModApplier Mods => (ModApplier)TryResolve<IModApplier>();
    public IFileSystem Disk => TryResolve<IFileSystem>();
    public IAssetManager Assets => TryResolve<IAssetManager>();
    public SpellManager Spells => TryResolve<SpellManager>();
    public IWordLookup Words => TryResolve<IWordLookup>();
    public GameState Game => (GameState)TryResolve<IGameState>();
    public Party Party => (Party)Game?.Party;
    public SpriteSamplerSource Samplers => (SpriteSamplerSource)TryResolve<ISpriteSamplerSource>();
    public EventChainManager Chains => (EventChainManager)TryResolve<IEventManager>();
    public MapManager Maps => (MapManager)TryResolve<IMapManager>();
    public IMap Map => Maps?.Current;
    public CollisionManager Coll => (CollisionManager)TryResolve<ICollisionManager>();
    public SceneManager Scenes => (SceneManager)TryResolve<ISceneManager>();
    public LayoutManager LayoutManager => (LayoutManager)TryResolve<ILayoutManager>();
    public DialogManager Dialogs => (DialogManager)TryResolve<IDialogManager>();
    public CursorManager Cursors => (CursorManager)TryResolve<ICursorManager>();
    public InputManager Input => (InputManager)TryResolve<IInputManager>();
    public ConversationManager Convos => (ConversationManager)TryResolve<IConversationManager>();
    public Conversation Convo => Convos?.Conversation;
}
#endif
