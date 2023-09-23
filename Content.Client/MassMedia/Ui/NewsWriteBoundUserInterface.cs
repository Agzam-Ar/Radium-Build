using Robust.Shared.Timing;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Content.Shared.MassMedia.Systems;
using Content.Shared.MassMedia.Components;
using Content.Client.GameTicking.Managers;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Client.MassMedia.Ui
{
    [UsedImplicitly]
    public sealed class NewsWriteBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private NewsWriteMenu? _menu;

        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        private ClientGameTicker? _gameTicker;

        [ViewVariables]
        private string _windowName = Loc.GetString("news-read-ui-default-title");

        public NewsWriteBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {

        }

        protected override void Open()
        {
            _menu = new NewsWriteMenu(_windowName);

            _menu.OpenCentered();
            _menu.OnClose += Close;

            _menu.ShareButtonPressed += OnShareButtonPressed;
            _menu.DeleteButtonPressed += OnDeleteButtonPressed;

            _gameTicker = _entitySystem.GetEntitySystem<ClientGameTicker>();

            SendMessage(new NewsWriteArticlesRequestMessage());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            _menu?.Close();
            _menu?.Dispose();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_menu == null || state is not NewsWriteBoundUserInterfaceState cast)
                return;

            _menu.UpdateUI(cast.Articles, cast.ShareAvalible);
        }

        private void OnShareButtonPressed()
        {
            if (_menu == null || _menu.NameInput.Text.Length == 0)
                return;

            var stringContent = Rope.Collapse(_menu.ContentInput.TextRope);

            if (stringContent == null || stringContent.Length == 0)
                return;

            var stringName = _menu.NameInput.Text;

            var maxNameLength = _cfg.GetCVar(CCVars.NewsNameLimit);
            var maxContentLength = _cfg.GetCVar(CCVars.NewsContentLimit);

            var name = (stringName.Length <= maxNameLength ? stringName.Trim() : $"{stringName.Trim().Substring(0, maxNameLength)}...");
            var content = (stringContent.Length <= maxContentLength ? stringContent.Trim() : $"{stringContent.Trim().Substring(0, maxContentLength)}...");

            _menu.ContentInput.TextRope = new Rope.Leaf(string.Empty);
            _menu.NameInput.Text = string.Empty;
            SendMessage(new NewsWriteShareMessage(name, content));
        }

        private void OnDeleteButtonPressed(int articleNum)
        {
            if (_menu == null) return;

            SendMessage(new NewsWriteDeleteMessage(articleNum));
        }
    }
}